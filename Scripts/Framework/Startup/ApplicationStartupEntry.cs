using Godot;

namespace GameFrameX.Startup.Application
{
    /// <summary>
    /// 应用启动入口（LauncherAuto 自动场景用）。
    /// 代码动态创建 GameEntry 组件链，等价 Launcher.tscn 手工摆放的组件节点，
    /// 双场景最终结果一致：UILogin 打开 + 登录协议冒烟往返。
    /// 迁移备注（Unity → Godot）：
    /// 1. Unity 版挂 StartupOptions 走 StartupRunner 完整启动流程；Godot 侧
    ///    组件链现状不经 StartupRunner 直达 UILogin，此处对齐该现状，
    ///    StartupRunner 流程接入属后续任务。
    /// 2. 场景 _Ready 传播期 Root 处于 busy setting up children 状态
    ///    （同 AssetSystem 驱动节点修复），组件链经 deferred 挂载。
    /// </summary>
    public sealed partial class ApplicationStartupEntry : Node
    {
        public override void _Ready()
        {
            GetTree().Root.CallDeferred(Node.MethodName.AddChild, BuildGameEntryChain());
        }

        private static Node BuildGameEntryChain()
        {
            // 节点名与组件构成对齐 Launcher.tscn（gfx.scn 迁移）手工链
            var gfx = new GameFrameX.Runtime.BaseComponent { Name = "GFX" };
            AddComponent(gfx, new GameFrameX.Runtime.ObjectPoolComponent(), "ObjectPool");
            AddComponent(gfx, new GameFrameX.Runtime.ReferencePoolComponent(), "ReferencePool");
            AddComponent(gfx, new GameFrameX.Event.Runtime.EventComponent(), "Event");
            AddComponent(gfx, new GameFrameX.Fsm.Runtime.FsmComponent(), "FSM");
            var procedure = new GameFrameX.Procedure.Runtime.ProcedureComponent();
            // 代码建链无场景序列化注入，此处复刻 Launcher.tscn Procedure 节点的 [Export] 配置，
            // 使双场景走同一条 StartProcedureInternal 启动路径
            procedure.ConfigureProcedures(new string[]
            {
                "Godot.Startup.Procedure.ProcedureCreateDownloader",
                "Godot.Startup.Procedure.ProcedureDownloadWebFiles",
                "Godot.Startup.Procedure.ProcedureGameLauncherState",
                "Godot.Startup.Procedure.ProcedureGetAppVersionInfoState",
                "Godot.Startup.Procedure.ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState",
                "Godot.Startup.Procedure.ProcedureGetGlobalInfoState",
                "Godot.Startup.Procedure.ProcedureLauncherState",
                "Godot.Startup.Procedure.ProcedurePatchDone",
                "Godot.Startup.Procedure.ProcedurePatchInit",
                "Godot.Startup.Procedure.ProcedureUpdateManifest",
                "Godot.Startup.Procedure.ProcedureUpdateStaticVersion",
            }, "Godot.Startup.Procedure.ProcedureLauncherState");
            AddComponent(gfx, procedure, "Procedure");
            AddComponent(gfx, new GameFrameX.Timer.Runtime.TimerComponent(), "Timer");
            AddComponent(gfx, new GameFrameX.Web.Runtime.WebComponent(), "Web");
            AddComponent(gfx, new GameFrameX.Web.ProtoBuff.Runtime.WebProtoBuffComponent(), "WebProtoBuff");
            AddComponent(gfx, new GameFrameX.Setting.Runtime.SettingComponent(), "Setting");
            AddComponent(gfx, new GameFrameX.Network.Runtime.NetworkComponent(), "Network");
            AddComponent(gfx, new GameFrameX.Localization.Runtime.LocalizationComponent(), "Localization");
            AddComponent(gfx, new GameFrameX.GlobalConfig.Runtime.GlobalConfigComponent(), "GlobalInfo");
            AddComponent(gfx, new GameFrameX.Config.Runtime.ConfigComponent(), "Config");
            AddComponent(gfx, new GameFrameX.Entity.Runtime.EntityComponent(), "Entity");
            AddComponent(gfx, new GameFrameX.Download.Runtime.DownloadComponent(), "Download");
            var ui = new GameFrameX.UI.Runtime.UIComponent();
            AddComponent(gfx, ui, "UI");
            AddComponent(ui, new Node(), "GDGUI");
            AddComponent(ui, new Node(), "FGUI");
            return gfx;
        }

        private static void AddComponent(Node parent, Node component, string name)
        {
            component.Name = name;
            parent.AddChild(component);
        }
    }
}
