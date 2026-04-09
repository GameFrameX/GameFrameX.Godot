#if TOOLS

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public static class EditorSimulateModeHelper
    {
        /// <summary>
        /// 编辑器下模拟构建清单
        /// </summary>
        [AssetSystemPreserve]
        public static SimulateBuildResult SimulateBuild(string buildPipelineName, string packageName)
        {
            return Editor.AssetSystemEditorSimulateBuilder.SimulateBuild(buildPipelineName, packageName);
        }

        /// <summary>
        /// 编辑器下模拟构建清单
        /// </summary>
        [AssetSystemPreserve]
        public static SimulateBuildResult SimulateBuild(EDefaultBuildPipeline buildPipeline, string packageName)
        {
            return SimulateBuild(buildPipeline.ToString(), packageName);
        }
    }
}
#else
namespace GameFrameX.AssetSystem
{ 
    [AssetSystemPreserve]
    public static class EditorSimulateModeHelper
    {
        [AssetSystemPreserve]
        public static SimulateBuildResult SimulateBuild(string buildPipelineName, string packageName) 
        {
            throw new System.Exception("Only support in Godot editor tools mode !");
        }

        [AssetSystemPreserve]
        public static SimulateBuildResult SimulateBuild(EDefaultBuildPipeline buildPipeline, string packageName)
        {
            throw new System.Exception("Only support in Godot editor tools mode !");
        }
    }
}
#endif
