using System;
using System.Collections.Generic;
using Godot;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal sealed class DatabaseAllAssetsProvider : ProviderOperation
    {
        [AssetSystemPreserve]
        public DatabaseAllAssetsProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo) : base(manager, providerGUID, assetInfo)
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
            // 迁移备注：编辑器模拟模式下按模拟清单记录的 IncludeAssetsInEditor 逐个从工程内加载（等价于 Unity 侧 AssetDatabase.LoadAssetAtPath）。
            if (_steps == ESteps.Loading)
            {
                var typeHint = GetGodotTypeHint(MainAssetInfo.AssetType);
                var result = new List<object>();
                foreach (var assetPath in LoadBundleFileOp.BundleFileInfo.IncludeAssetsInEditor)
                {
                    var assetObject = ResourceLoader.Load(assetPath, typeHint);
                    if (BundleAssetLoadUtility.IsTypeMatch(assetObject, MainAssetInfo.AssetType))
                    {
                        result.Add(assetObject);
                    }
                }

                AllAssetObjects = result.ToArray();
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
                        error = $"Failed to load all assets : {MainAssetInfo.AssetPath} AssetType : null";
                    }
                    else
                    {
                        error = $"Failed to load all assets : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType}";
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
