using System;
using Godot;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal sealed class DatabaseSubAssetsProvider : ProviderOperation
    {
        [AssetSystemPreserve]
        public DatabaseSubAssetsProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo) : base(manager, providerGUID, assetInfo)
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

            // 2. 加载资源对象集合
            // ponytail: Godot 没有 Unity 的“子资产（SubAsset）”概念，这里将语义映射为“加载同路径主资源 + 类型过滤”，
            // 即请求 Texture2D 子资产时返回的集合里要么是主资源本身（类型匹配），要么为空集合（类型不匹配，按 YooAsset 语义不算失败）。
            // 升级路径：若未来需要 .tres 内嵌 SubResource 级别的枚举，改为加载主资源后遍历其子资源并按类型过滤。
            if (_steps == ESteps.Loading)
            {
                var mainAsset = ResourceLoader.Load(MainAssetInfo.AssetPath, GetGodotTypeHint(MainAssetInfo.AssetType));
                if (mainAsset != null)
                {
                    AllAssetObjects = BundleAssetLoadUtility.FilterByType(new object[] { mainAsset }, MainAssetInfo.AssetType);
                }

                _steps = ESteps.Checking;
            }

            // 3. 检测加载结果
            if (_steps == ESteps.Checking)
            {
                if (AllAssetObjects == null)
                {
                    string error;
                    if (MainAssetInfo.AssetType == null)
                    {
                        error = $"Failed to load sub assets : {MainAssetInfo.AssetPath} AssetType : null";
                    }
                    else
                    {
                        error = $"Failed to load sub assets : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType}";
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
