using System;
using System.Collections.Generic;
using System.IO;
using GameFrameX.Setting.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 存档文件存储整文件读写通道测试（对齐 Unity setting 包 2.1.0 新增能力：
    /// 文件名校验、本地磁盘后端两阶段原子写、工厂保留首个注册语义）。
    /// 纯托管 I/O（经 FileHelper/PathHelper），不触碰 Godot 原生运行时。
    /// </summary>
    public sealed class SaveSlotFileStorageTests : IDisposable
    {
        private string m_RootDirectory;

        public SaveSlotFileStorageTests()
        {
            m_RootDirectory = Path.Combine(Path.GetTempPath(), "SaveSlotFileStorageTests-" + Guid.NewGuid().ToString("N"));
            SaveSlotFileStorageBackendFactory.ResetForTests();
            SaveSlotFileStorageBackendFactory.SetSaveSlotFileStorageRoot(m_RootDirectory);
        }

        public void Dispose()
        {
            SaveSlotFileStorageBackendFactory.ResetForTests();
            if (Directory.Exists(m_RootDirectory))
            {
                Directory.Delete(m_RootDirectory, true);
            }
        }

        private LocalDiskSaveSlotFileStorageBackend CreateBackend()
        {
            return new LocalDiskSaveSlotFileStorageBackend(m_RootDirectory);
        }

        [Fact]
        public void NameValidator_RejectsIllegalNames()
        {
            Assert.True(SaveSlotFileStorageNameValidator.IsValid("profile1_battle.dat"));
            Assert.True(SaveSlotFileStorageNameValidator.IsValid("save-01.json"));
            Assert.False(SaveSlotFileStorageNameValidator.IsValid(null));
            Assert.False(SaveSlotFileStorageNameValidator.IsValid(string.Empty));
            Assert.False(SaveSlotFileStorageNameValidator.IsValid("../escape.sav"));
            Assert.False(SaveSlotFileStorageNameValidator.IsValid("a/b.dat"));
            Assert.False(SaveSlotFileStorageNameValidator.IsValid("a b.dat"));
            Assert.False(SaveSlotFileStorageNameValidator.IsValid(new string('a', 97)));
            Assert.True(SaveSlotFileStorageNameValidator.IsValid(new string('a', 96)));
            Assert.Throws<SaveSlotFileStorageNameInvalidException>(() => SaveSlotFileStorageNameValidator.EnsureValid("a/b.dat"));
        }

        [Fact]
        public void Backend_WriteReadRoundTrip_OverwritesSameName()
        {
            LocalDiskSaveSlotFileStorageBackend backend = CreateBackend();
            byte[] first = new byte[] { 1, 2, 3 };
            byte[] second = new byte[] { 9 };

            backend.Write("slot.dat", first);
            Assert.Equal(first, backend.Read("slot.dat"));

            backend.Write("slot.dat", second);
            Assert.Equal(second, backend.Read("slot.dat"));
            Assert.True(backend.Exists("slot.dat"));

            // 两阶段写完成后 Temp 目录为空，正式区可见文件
            string tempDirectory = Path.Combine(m_RootDirectory, "Temp");
            Assert.True(Directory.Exists(tempDirectory));
            Assert.Empty(Directory.GetFiles(tempDirectory));
            Assert.Single(Directory.GetFiles(m_RootDirectory, "slot.dat"));
        }

        [Fact]
        public void Backend_ReadMissingFile_ThrowsNotFound()
        {
            LocalDiskSaveSlotFileStorageBackend backend = CreateBackend();

            Assert.Throws<SaveSlotFileStorageFileNotFoundException>(() => backend.Read("missing.dat"));
        }

        [Fact]
        public void Backend_DeleteReportsExistence_ListsFilesOnlyInRoot()
        {
            LocalDiskSaveSlotFileStorageBackend backend = CreateBackend();

            Assert.False(backend.Delete("slot.dat"));

            backend.Write("alpha.dat", new byte[] { 1 });
            backend.Write("beta.sav", new byte[] { 2, 3 });

            List<string> fileNames = new List<string>(backend.ListFiles());
            Assert.Equal(new HashSet<string> { "alpha.dat", "beta.sav" }, new HashSet<string>(fileNames));

            Assert.True(backend.Delete("alpha.dat"));
            Assert.False(backend.Exists("alpha.dat"));
            Assert.False(backend.Delete("alpha.dat"));
            Assert.Equal(new string[] { "beta.sav" }, backend.ListFiles());
        }

        [Fact]
        public void Backend_ListFiles_MissingDirectoryReturnsEmpty()
        {
            LocalDiskSaveSlotFileStorageBackend backend = new LocalDiskSaveSlotFileStorageBackend(Path.Combine(m_RootDirectory, "not-created"));

            Assert.Empty(backend.ListFiles());
        }

        [Fact]
        public void Factory_ChannelRegistrationKeepsFirst()
        {
            SaveSlotFileStorageBackendFactory.ResetForTests();
            SaveSlotFileStorageBackendFactory.RegisterChannelBackend(() => new LocalDiskSaveSlotFileStorageBackend(m_RootDirectory));
            SaveSlotFileStorageBackendFactory.RegisterChannelBackend(() => new LocalDiskSaveSlotFileStorageBackend(Path.Combine(m_RootDirectory, "other")));

            ISaveSlotFileStorageBackend backend = SaveSlotFileStorageBackendFactory.Create();
            backend.Write("channel.dat", new byte[] { 7 });
            Assert.True(File.Exists(Path.Combine(m_RootDirectory, "channel.dat")));
        }

        [Fact]
        public void Factory_SetRoot_InvalidThrows()
        {
            Assert.Throws<SaveSlotFileStorageException>(() => SaveSlotFileStorageBackendFactory.SetSaveSlotFileStorageRoot(null));
            Assert.Throws<SaveSlotFileStorageException>(() => SaveSlotFileStorageBackendFactory.SetSaveSlotFileStorageRoot(string.Empty));
        }

        [Fact]
        public void Factory_RegisterChannelBackend_NullThrows()
        {
            Assert.Throws<SaveSlotFileStorageException>(() => SaveSlotFileStorageBackendFactory.RegisterChannelBackend(null));
        }
    }
}
