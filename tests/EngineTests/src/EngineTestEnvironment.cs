namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 引擎测试环境信息：编译期 TOOLS 与运行期编辑器上下文。
    /// </summary>
    public static class EngineTestEnvironment
    {
#if TOOLS
        public const bool HasToolsDefine = true;
#else
        public const bool HasToolsDefine = false;
#endif
    }
}
