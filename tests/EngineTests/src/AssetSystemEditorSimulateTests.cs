using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 组 A：AssetSystem EditorSimulate 链路（Phase 2.2）。
    /// 前置：TOOLS 构建 + 编辑器上下文（headless -e --script 入口）。
    /// </summary>
    public static class AssetSystemEditorSimulateTests
    {
        public static void Register(List<EngineTestCase> cases)
        {
            cases.Add(new EngineTestCase("AssetSystem", "A1_EditorSimulate_InitializeAndManifest", InitializeAndManifestAsync));
            cases.Add(new EngineTestCase("AssetSystem", "A2_LoadTypedResourceFromTres", LoadTypedResourceAsync));
            cases.Add(new EngineTestCase("AssetSystem", "A3_LoadPackedSceneAndInstantiate", LoadSceneResourceAsync));
            cases.Add(new EngineTestCase("AssetSystem", "A4_LoadTexture4x4", LoadTextureAsync));
            cases.Add(new EngineTestCase("AssetSystem", "A5_HandleRefCountAndRelease", HandleRefCountAsync));
            cases.Add(new EngineTestCase("AssetSystem", "A6_LoadAudioStreamResource", LoadAudioStreamAsync));
        }

        private static async Task InitializeAndManifestAsync()
        {
            var package = await TestAssetPackageFixture.GetPackageAsync();
            EngineAssert.NotNull(package, "ResourcePackage");
            EngineAssert.Equal(EOperationStatus.Succeed, package.InitializeStatus, "package.InitializeStatus");
            EngineAssert.Equal(TestAssetPackageFixture.PackageName, package.PackageName, "package.PackageName");
            var probeHandle = package.LoadAssetAsync(TestAssetPackageFixture.ResourceAddress);
            await EngineTestWait.HandleAsync(probeHandle, "probe LoadAssetAsync");
            probeHandle.Release();
        }

        private static async Task LoadTypedResourceAsync()
        {
            var package = await TestAssetPackageFixture.GetPackageAsync();
            var handle = package.LoadAssetAsync<Resource>(TestAssetPackageFixture.ResourceAddress);
            await EngineTestWait.HandleAsync(handle, "LoadAssetAsync test_resource");
            EngineAssert.True(handle.IsDone, "IsDone");
            EngineAssert.NotNull(handle.AssetObject, "AssetObject");
            var resource = handle.AssetObject as Resource;
            EngineAssert.NotNull(resource, "Resource 实例");
            var script = resource.GetScript().As<Script>();
            EngineAssert.NotNull(script, "资源脚本应附着（TestResource.cs）");
            EngineAssert.True(script.ResourcePath.EndsWith("TestResource.cs"), "资源脚本路径");
            EngineAssert.Equal("engine_test_tres_value", resource.Get("TestValue").AsString(), "TestValue");
            handle.Release();
        }

        private static async Task LoadSceneResourceAsync()
        {
            var package = await TestAssetPackageFixture.GetPackageAsync();
            var handle = package.LoadAssetAsync<PackedScene>(TestAssetPackageFixture.SceneAAddress);
            await EngineTestWait.HandleAsync(handle, "LoadAssetAsync test_scene");
            var packedScene = handle.AssetObject as PackedScene;
            EngineAssert.NotNull(packedScene, "PackedScene 实例");
            var instance = handle.InstantiateSync();
            EngineAssert.NotNull(instance, "InstantiateSync 节点");
            var markerScript = instance.GetScript().As<Script>();
            EngineAssert.NotNull(markerScript, "根节点脚本应附着");
            EngineAssert.True(markerScript.ResourcePath.EndsWith("TestSceneMarker.cs"), "根节点脚本路径");
            EngineAssert.Equal("scene_A", instance.Get("SceneMarker").AsString(), "SceneMarker");
            EngineAssert.NotNull(instance.GetNodeOrNull<Label>("Label"), "场景 Label 子节点");
            instance.QueueFree();
            handle.Release();
        }

        private static async Task LoadTextureAsync()
        {
            var package = await TestAssetPackageFixture.GetPackageAsync();
            var handle = package.LoadAssetAsync<Texture2D>(TestAssetPackageFixture.TextureAddress);
            await EngineTestWait.HandleAsync(handle, "LoadAssetAsync test_png");
            var texture = handle.AssetObject as Texture2D;
            EngineAssert.NotNull(texture, "Texture2D 实例");
            EngineAssert.Equal(4, texture.GetWidth(), "texture width");
            EngineAssert.Equal(4, texture.GetHeight(), "texture height");
            handle.Release();
        }

        private static async Task HandleRefCountAsync()
        {
            var package = await TestAssetPackageFixture.GetPackageAsync();
            var first = package.LoadAssetAsync<Resource>(TestAssetPackageFixture.ResourceAddress);
            var second = package.LoadAssetAsync<Resource>(TestAssetPackageFixture.ResourceAddress);
            await EngineTestWait.HandleAsync(first, "first handle");
            await EngineTestWait.HandleAsync(second, "second handle");
            EngineAssert.True(first.IsValid, "首次 Release 前句柄有效");
            first.Release();
            EngineAssert.False(first.IsValid, "首次 Release 后句柄应失效");
            EngineAssert.True(second.IsValid, "引用计数>0 时第二句柄应仍有效");
            second.Release();
            EngineAssert.False(second.IsValid, "引用归零后第二句柄应失效");
        }

        private static async Task LoadAudioStreamAsync()
        {
            var package = await TestAssetPackageFixture.GetPackageAsync();
            var handle = package.LoadAssetAsync<AudioStream>(TestAssetPackageFixture.AudioAddress);
            await EngineTestWait.HandleAsync(handle, "LoadAssetAsync test_audio");
            var stream = handle.AssetObject as AudioStream;
            EngineAssert.NotNull(stream, "AudioStream 实例");
            EngineAssert.True(stream.GetLength() > 0d, "音频长度应大于 0");
            handle.Release();
        }
    }
}
