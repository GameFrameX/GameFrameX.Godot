using System;
using System.Threading.Tasks;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 单个引擎测试用例：组名 + 用例名 + 可 await 的异步用例体。
    /// </summary>
    public sealed class EngineTestCase
    {
        public string Group { get; }

        public string Name { get; }

        public Func<Task> Body { get; }

        public EngineTestCase(string group, string name, Func<Task> body)
        {
            Group = group;
            Name = name;
            Body = body;
        }
    }
}
