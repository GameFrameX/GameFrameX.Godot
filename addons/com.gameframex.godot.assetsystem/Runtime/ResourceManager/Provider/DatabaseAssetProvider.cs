using System;
using Godot;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal sealed class DatabaseAssetProvider : ProviderOperation
    {
        [AssetSystemPreserve]
        public DatabaseAssetProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo) : base(manager, providerGUID, assetInfo)
        {
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            BeginLoadTimeRecord();
            DebugBeginRecording();
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (IsDone)
            {
                return;
            }

            if (_steps == ESteps.None)
            {
                // 检测资源文件是否存在
                if (ResourceLoader.Exists(MainAssetInfo.AssetPath) == false)
                {
                    var error = $"Not found asset : {MainAssetInfo.AssetPath}";
                    AssetSystemLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.CheckBundle;

                // 注意：模拟异步加载效果提前返回
                if (IsWaitForAsyncComplete == false)
                {
                    return;
                }
            }

            // 1. 检测资源包
            if (_steps == ESteps.CheckBundle)
            {
                if (LoadBundleFileOp.IsDone == false)
                {
                    return;
                }

                if (LoadBundleFileOp.Status != EOperationStatus.Succeed)
                {
                    InvokeCompletion(LoadBundleFileOp.Error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.Loading;
            }

            // 2. 加载资源对象
            // 迁移备注：编辑器模拟模式下资源就在工程内，直接通过 res:// 路径加载，等价于 Unity 侧的 AssetDatabase.LoadAssetAtPath。
            if (_steps == ESteps.Loading)
            {
                AssetObject = ResourceLoader.Load(MainAssetInfo.AssetPath, GetGodotTypeHint(MainAssetInfo.AssetType));
                _steps = ESteps.Checking;
            }

            // 3. 检测加载结果
            if (_steps == ESteps.Checking)
            {
                // 注意：加载结果为空或者类型不匹配都视为失败，不允许假成功
                if (BundleAssetLoadUtility.IsTypeMatch(AssetObject, MainAssetInfo.AssetType) == false)
                {
                    string error;
                    if (MainAssetInfo.AssetType == null)
                    {
                        error = $"Failed to load asset object : {MainAssetInfo.AssetPath} AssetType : null";
                    }
                    else
                    {
                        error = $"Failed to load asset object : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType}";
                    }
                    AssetSystemLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                }
                else
                {
                    InvokeCompletion(string.Empty, EOperationStatus.Succeed);
                }
            }
        }

        /// <summary>
        /// 获取 Godot 资源类型提示
        /// 语义与 AssetSystem.GodotExtensions.GetGodotTypeHint 一致（那里为 private 无法跨类复用）。
        /// </summary>
        [AssetSystemPreserve]
        private static string GetGodotTypeHint(Type assetType)
        {
            if (assetType == null || typeof(Resource).IsAssignableFrom(assetType) == false)
            {
                return string.Empty;
            }

            return assetType.Name;
        }
    }
}
