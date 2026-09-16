using System.Collections.Generic;
using Godot;

namespace GameFrameX.LeanCLR;

// LeanCLR 热更新演示脚本（纯 C#，由 LeanCLR 解释执行，不经 Godot Mono/CoreCLR）。
// 通过 Assets/LeanCLR/LeanCLRHotfix.csproj 编译为 LeanCLRHotfix.dll，
// 节点由同目录 HotfixDemo.lcs 声明式绑定，避免与 Godot C# 模块的 .cs 争夺。
public class HotfixDemo : Node
{
#if LEANCLR_HOTFIX_V2
    private const string Version = "hotfix-v2";
#else
    private const string Version = "hotfix-v1";
#endif

    // 热更迁移验证字段：交换程序集后由 LeanCLRHotReloadHost 按名迁移（公有字段参与迁移）。
    public int TickCount;

    public override void _EnterTree()
    {
        GD.Print("[LeanCLRHotfix] _EnterTree version=" + Version + " owner=" + Name);
    }

    public override void _Ready()
    {
        GD.Print("[LeanCLRHotfix] _Ready version=" + Version + " tick=" + TickCount);
        RunCSharpInteropCheck();
    }

    public override void _Process(double delta)
    {
        TickCount += 1;
        if (TickCount % 60 == 0)
        {
            GD.Print("[LeanCLRHotfix] running version=" + Version + " tick=" + TickCount);
        }
    }

    // C# 互操作自检：引擎 API 封装（Node.Name/GetParent/StringName）与
    // BCL 泛型/字符串/数学（List<T>.Sort、string.Join、Math.Sqrt）均在 LeanCLR 解释执行。
    private void RunCSharpInteropCheck()
    {
        string selfName = Name.ToString();
        Node parent = GetParent();
        string parentName = parent != null ? parent.Name.ToString() : "<none>";

        List<int> scores = new List<int>();
        scores.Add(3);
        scores.Add(1);
        scores.Add(2);
        scores.Sort();
        string sorted = string.Join(",", scores);

        Vector2 v = new Vector2(3.0f, 4.0f);
        double vecLength = System.Math.Sqrt((double)(v.X * v.X + v.Y * v.Y));

        bool pass = selfName == "Demo"
            && parentName == "LeanCLRHotReloadVerifier"
            && sorted == "1,2,3"
            && System.Math.Abs(vecLength - 5.0) < 0.0001;

        GD.Print("[LeanCLRHotfix] csharp-interop version=" + Version
            + " self=" + selfName
            + " parent=" + parentName
            + " sorted=" + sorted
            + " veclen=" + vecLength
            + " pass=" + (pass ? "TRUE" : "FALSE"));
    }
}
