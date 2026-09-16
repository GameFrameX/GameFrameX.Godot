using Godot;

namespace GameFrameX.Runtime;

public partial class LeanClrSmokeTest : Node
{
    public override void _Ready()
    {
        GD.Print("[SMOKE] LeanCLR 实例化 GameFrameX 程序集类成功");
        GD.Print("[SMOKE] Engine.MaxFps 静态外观 = " + Engine.MaxFps);
        GD.Print("[SMOKE] OS.GetName 静态外观 = " + OS.GetName());
        GD.Print("[SMOKE] Mathf.DegToRad(180) = " + Mathf.DegToRad(180f));
        GD.Print("[SMOKE] GameEntry 类型解析 = " + typeof(GameEntry).FullName);
        GetTree().Quit();
    }
}
