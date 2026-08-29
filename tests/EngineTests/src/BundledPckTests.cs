using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 组 B：Bundled/PCK 链路（Phase 2.1）。
    /// 测试内用 PckPacker 现场构造最小 PCK（源资源按 res:// 相对路径打入），
    /// 挂载后经 BundleFile.LoadAsset 按 AssetPath 真加载；并验证失败路径无假成功。
    /// </summary>
    public static class BundledPckTests
    {
        private const string InnerResourceResPath = "res://tests/engine_test_assets/pck_only/inner_resource.tres";
        private const string InnerSceneResPath = "res://tests/engine_test_assets/pck_only/inner_scene.tscn";
        private const string DecodeOnlyPngResPath = "res://tests/engine_test_assets/pck_only/deco.png";

        private static bool _mounted;

        public static void Register(List<EngineTestCase> cases)
        {
            cases.Add(new EngineTestCase("BundledPck", "B1_PckPackAndMount", PackAndMountAsync));
            cases.Add(new EngineTestCase("BundledPck", "B2_LoadTypedResourceFromMountedPck", LoadTypedResourceFromPckAsync));
            cases.Add(new EngineTestCase("BundledPck", "B3_LoadSceneFromMountedPck", LoadSceneFromPckAsync));
            cases.Add(new EngineTestCase("BundledPck", "B4_TextureDecodeFallbackFromBundleBytes", TextureDecodeFallbackAsync));
            cases.Add(new EngineTestCase("BundledPck", "B5_MissingPathMustReturnNull", MissingPathMustFailAsync));
            cases.Add(new EngineTestCase("BundledPck", "B6_PackageLoadMissingAssetMustFail", PackageMissingAssetMustFailAsync));
        }

        private static async Task PackAndMountAsync()
        {
            EngineAssert.False(ResourceLoader.Exists(InnerResourceResPath), "挂载前 PCK 内资源不应存在于工程");
            var sourceRoot = ProjectSettings.GlobalizePath("user://engine_tests/pck_src");
            if (Directory.Exists(sourceRoot))
            {
                Directory.Delete(sourceRoot, true);
            }

            Directory.CreateDirectory(sourceRoot);
            var resourceSourcePath = Path.Combine(sourceRoot, "inner_resource.tres");
            File.WriteAllText(resourceSourcePath, "[gd_resource type=\"Resource\" load_steps=2 format=3]\n\n" +
                "[ext_resource type=\"Script\" path=\"res://tests/engine_test_assets/TestResource.cs\" id=\"1\"]\n\n" +
                "[resource]\nscript = ExtResource(\"1\")\nTestValue = \"pck_inner_value\"\n");
            var sceneSourcePath = Path.Combine(sourceRoot, "inner_scene.tscn");
            File.WriteAllText(sceneSourcePath, "[gd_scene format=3]\n\n[node name=\"InnerScene\" type=\"Node\"]\n\n" +
                "[node name=\"InnerLabel\" type=\"Label\" parent=\".\"]\ntext = \"pck inner scene\"\n");
            var pckGlobalPath = ProjectSettings.GlobalizePath("user://engine_tests/engine_tests.pck");
            var packer = new PckPacker();
            var startError = packer.PckStart(pckGlobalPath);
            EngineAssert.Equal(Error.Ok, startError, "PckStart");
            var addError = packer.AddFile(InnerResourceResPath.Substring("res://".Length), resourceSourcePath);
            EngineAssert.Equal(Error.Ok, addError, "AddFile inner_resource.tres");
            addError = packer.AddFile(InnerSceneResPath.Substring("res://".Length), sceneSourcePath);
            EngineAssert.Equal(Error.Ok, addError, "AddFile inner_scene.tscn");
            var flushError = packer.Flush();
            EngineAssert.Equal(Error.Ok, flushError, "PckFlush");
            EngineAssert.True(File.Exists(pckGlobalPath), "PCK 文件已生成");
            global::GameFrameX.AssetSystem.AssetSystem.Initialize();
            var mounted = global::GameFrameX.AssetSystem.AssetSystem.MountGodotResourcePackByPath(pckGlobalPath, false, 0);
            EngineAssert.True(mounted, "MountGodotResourcePackByPath");
            _mounted = true;
            await Task.Yield();
            EngineAssert.True(ResourceLoader.Exists(InnerResourceResPath), "挂载后 PCK 内资源应可被 ResourceLoader 识别");
        }

        private static void RequireMounted()
        {
            if (_mounted == false)
            {
                throw new EngineTestSkippedException("B1 挂载步骤未执行，PCK 用例不可用");
            }
        }

        private static async Task LoadTypedResourceFromPckAsync()
        {
            RequireMounted();
            var bundleFile = BundleFile.LoadFromMemory(new byte[] { 1 });
            var asset = bundleFile.LoadAsset(InnerResourceResPath, typeof(TestResource));
            EngineAssert.NotNull(asset, "PCK 内 TestResource 加载结果");
            var testResource = asset as TestResource;
            EngineAssert.NotNull(testResource, "TestResource 实例");
            EngineAssert.Equal("pck_inner_value", testResource.TestValue, "PCK 内 TestValue");
            await Task.Yield();
        }

        private static async Task LoadSceneFromPckAsync()
        {
            RequireMounted();
            var bundleFile = BundleFile.LoadFromMemory(new byte[] { 1 });
            var asset = bundleFile.LoadAsset(InnerSceneResPath, typeof(PackedScene));
            EngineAssert.NotNull(asset, "PCK 内 PackedScene 加载结果");
            var packedScene = asset as PackedScene;
            EngineAssert.NotNull(packedScene, "PackedScene 实例");
            var instance = packedScene.Instantiate();
            EngineAssert.NotNull(instance, "PCK 内场景实例化节点");
            EngineAssert.NotNull(instance.GetNodeOrNull<Label>("InnerLabel"), "PCK 内场景 Label 子节点");
            instance.QueueFree();
            await Task.Yield();
        }

        private static async Task TextureDecodeFallbackAsync()
        {
            var pngBytes = File.ReadAllBytes(ProjectSettings.GlobalizePath("res://tests/engine_test_assets/tiny_4x4.png"));
            EngineAssert.True(pngBytes.Length > 0, "PNG 字节非空");
            var bundleFile = BundleFile.LoadFromMemory(pngBytes);
            var asset = bundleFile.LoadAsset(DecodeOnlyPngResPath, typeof(Texture2D));
            EngineAssert.NotNull(asset, "ResourceLoader 未命中时应按 bundle 字节解码纹理");
            var texture = asset as Texture2D;
            EngineAssert.NotNull(texture, "Texture2D 实例");
            EngineAssert.Equal(4, texture.GetWidth(), "解码纹理 width");
            EngineAssert.Equal(4, texture.GetHeight(), "解码纹理 height");
            await Task.Yield();
        }

        private static async Task MissingPathMustFailAsync()
        {
            var bundleFile = BundleFile.LoadFromMemory(new byte[] { 1 });
            var asset = bundleFile.LoadAsset("res://tests/engine_test_assets/pck_only/missing.tres", typeof(Resource));
            EngineAssert.IsNull(asset, "不存在路径必须返回 null（假成功已删）");
            var sceneAsset = bundleFile.LoadAsset("res://tests/engine_test_assets/pck_only/missing.tscn", typeof(PackedScene));
            EngineAssert.IsNull(sceneAsset, "不存在场景路径必须返回 null");
            await Task.Yield();
        }

        private static async Task PackageMissingAssetMustFailAsync()
        {
            var package = await TestAssetPackageFixture.GetPackageAsync();
            var handle = package.LoadAssetAsync(TestAssetPackageFixture.BrokenAssetAddress);
            await EngineTestWait.HandleFailureAsync(handle, "LoadAssetAsync broken_asset");
            EngineAssert.IsNull(handle.AssetObject, "失败句柄不得携带资产对象（假成功已删）");
        }
    }
}
