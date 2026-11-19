// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents and other related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//
//  使用本项目须严格遵守相应法律法规与开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe any legal rights or interests of others as prohibited by laws and regulations!
//
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes or liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  CNB 仓库：https://cnb.cool/GameFrameX
//  CNB Repository:  https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using GameFrameX.Runtime;

namespace GameFrameX.Setting.Runtime
{
    /// <summary>
    /// 本地磁盘存档文件存储后端。目录即真相：写入经 <c>Temp</c> 目录两阶段原子写，枚举直接扫描根目录；全部 I/O 经 <see cref="FileHelper"/> 与 <see cref="PathHelper"/>，不直接调用 <c>System.IO</c> 文件 API。
    /// </summary>
    public sealed class LocalDiskSaveSlotFileStorageBackend : ISaveSlotFileStorageBackend
    {
        private const string TempDirectoryName = "Temp";

        private readonly string m_RootDirectory;

        /// <summary>
        /// 使用指定根目录初始化后端。
        /// </summary>
        /// <param name="rootDirectory">根目录（应位于可读写目录，如 <c>user://</c> 持久化目录之下）。</param>
        public LocalDiskSaveSlotFileStorageBackend(string rootDirectory)
        {
            if (string.IsNullOrEmpty(rootDirectory))
            {
                throw new SaveSlotFileStorageException("Save slot file storage root directory is invalid.");
            }

            m_RootDirectory = rootDirectory;
        }

        /// <summary>
        /// 写入存档文件，同名整文件覆盖。
        /// </summary>
        /// <remarks>
        /// 两阶段原子写：先写 <c>{root}/Temp/{fileName}</c>，再移动到 <c>{root}/{fileName}</c>；每次写入前清空 Temp。移动失败时正式区保留旧文件。
        /// </remarks>
        /// <param name="fileName">已通过校验的文件名。</param>
        /// <param name="contents">文件内容字节。</param>
        /// <exception cref="SaveSlotFileStorageException">I/O 失败时抛出。</exception>
        public void Write(string fileName, byte[] contents)
        {
            try
            {
                string tempDirectory = PathHelper.Combine(m_RootDirectory, TempDirectoryName);
                Directory.CreateDirectory(m_RootDirectory);
                Directory.CreateDirectory(tempDirectory);
                FileHelper.CleanDirectory(tempDirectory);

                string tempFilePath = PathHelper.Combine(tempDirectory, fileName);
                string finalFilePath = PathHelper.Combine(m_RootDirectory, fileName);
                FileHelper.WriteAllBytes(tempFilePath, contents);
                FileHelper.Move(tempFilePath, finalFilePath);
            }
            catch (Exception exception)
            {
                throw new SaveSlotFileStorageException("Failed to write save slot file '" + fileName + "'.", exception);
            }
        }

        /// <summary>
        /// 读取存档文件。
        /// </summary>
        /// <remarks>
        /// 只读正式区，<c>Temp</c> 目录内的文件永不对外可见。
        /// </remarks>
        /// <param name="fileName">已通过校验的文件名。</param>
        /// <returns>文件内容字节。</returns>
        /// <exception cref="SaveSlotFileStorageFileNotFoundException">文件不存在时抛出。</exception>
        /// <exception cref="SaveSlotFileStorageException">I/O 失败时抛出。</exception>
        public byte[] Read(string fileName)
        {
            string finalFilePath = PathHelper.Combine(m_RootDirectory, fileName);
            if (!FileHelper.IsExists(finalFilePath))
            {
                throw new SaveSlotFileStorageFileNotFoundException(fileName);
            }

            try
            {
                return FileHelper.ReadAllBytes(finalFilePath);
            }
            catch (Exception exception)
            {
                throw new SaveSlotFileStorageException("Failed to read save slot file '" + fileName + "'.", exception);
            }
        }

        /// <summary>
        /// 检查存档文件是否存在。
        /// </summary>
        /// <param name="fileName">已通过校验的文件名。</param>
        /// <returns>是否存在。</returns>
        public bool Exists(string fileName)
        {
            return FileHelper.IsExists(PathHelper.Combine(m_RootDirectory, fileName));
        }

        /// <summary>
        /// 删除存档文件。
        /// </summary>
        /// <param name="fileName">已通过校验的文件名。</param>
        /// <returns>文件存在并删除返回 <c>true</c>；不存在返回 <c>false</c>。</returns>
        /// <exception cref="SaveSlotFileStorageException">I/O 失败时抛出。</exception>
        public bool Delete(string fileName)
        {
            string finalFilePath = PathHelper.Combine(m_RootDirectory, fileName);
            if (!FileHelper.IsExists(finalFilePath))
            {
                return false;
            }

            try
            {
                FileHelper.Delete(finalFilePath);
                return true;
            }
            catch (Exception exception)
            {
                throw new SaveSlotFileStorageException("Failed to delete save slot file '" + fileName + "'.", exception);
            }
        }

        /// <summary>
        /// 列出根目录下全部存档文件名。
        /// </summary>
        /// <remarks>
        /// 目录即真相：直接扫描根目录，不维护索引文件；<c>Temp</c> 目录与不合规文件名被排除。目录不存在时返回空数组，永不抛出。
        /// </remarks>
        /// <returns>文件名数组。</returns>
        /// <exception cref="SaveSlotFileStorageException">扫描目录失败时抛出。</exception>
        public string[] ListFiles()
        {
            List<string> filePaths = new List<string>();
            try
            {
                FileHelper.GetAllFiles(filePaths, m_RootDirectory);
            }
            catch (Exception exception)
            {
                throw new SaveSlotFileStorageException("Failed to list save slot files.", exception);
            }

            List<string> fileNames = new List<string>();
            string tempPrefix = PathHelper.NormalizePath(PathHelper.Combine(m_RootDirectory, TempDirectoryName)) + "/";
            foreach (string filePath in filePaths)
            {
                string normalizedPath = PathHelper.NormalizePath(filePath);
                if (normalizedPath != null && normalizedPath.StartsWith(tempPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                int lastSeparatorIndex = normalizedPath == null ? -1 : normalizedPath.LastIndexOf('/');
                string fileName = lastSeparatorIndex >= 0 ? normalizedPath.Substring(lastSeparatorIndex + 1) : normalizedPath;
                if (!SaveSlotFileStorageNameValidator.IsValid(fileName))
                {
                    continue;
                }

                fileNames.Add(fileName);
            }

            return fileNames.ToArray();
        }
    }
}
