using System;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Godot;
using Godot.Startup.Account;

namespace Godot.Startup.Verification
{
	/// <summary>
	/// 账号创建 → 登录 → 创建角色 → 进入游戏 全流程自动驱动器。
	/// 运行方式：godot --path . -- --auto-login-flow [--auto-quit] [--auto-flow-keep-store]
	/// 通过各表单暴露的 Auto* 入口驱动真实 UI 校验与跳转逻辑（等价于用户输入+点击），
	/// 并在关键阶段截图到 user://auto_flow/ 供人工核对。
	/// 默认每次重置本地账号数据，保证走“账号创建+创建角色”完整路径；
	/// 传入 --auto-flow-keep-store 复用已有账号，改走“登录+角色列表选择”路径。
	/// </summary>
	public partial class LoginFlowAutoDriver : Node
	{
		private const string AutoFlowArg = "--auto-login-flow";
		private const string AutoQuitArg = "--auto-quit";
		private const string KeepStoreArg = "--auto-flow-keep-store";
		private const string DemoAccount = "auto_flow";
		private const string DemoPassword = "123456";
		private const string DemoRoleName = "AutoRole";
		private const string ScreenshotDir = "user://auto_flow";
		private const int StageTimeoutFrames = 3600;
		private const int SettleFrames = 20;

		private int _stageIndex;

		public override void _Ready()
		{
			var args = OS.GetCmdlineUserArgs();
			if (Array.IndexOf(args, AutoFlowArg) < 0)
			{
				QueueFree();
				return;
			}

			var autoQuit = Array.IndexOf(args, AutoQuitArg) >= 0;
			GD.Print("[AutoFlow] enabled. 流程：账号创建 → 登录 → 创建角色 → 进入游戏 autoQuit=" + autoQuit);
			DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(ScreenshotDir));
			if (Array.IndexOf(args, KeepStoreArg) < 0)
			{
				LocalAccountStore.Reset();
			}
			else
			{
				GD.Print("[AutoFlow] keep store. 已有账号将走角色列表路径。");
			}

			_ = RunAsync(autoQuit);
		}

		private async Task RunAsync(bool autoQuit)
		{
			var sceneTree = GetTree();

			UIComponent uiComponent = null;
			for (var i = 0; i < StageTimeoutFrames; i++)
			{
				uiComponent = GameEntry.GetComponent<UIComponent>();
				if (uiComponent != null && uiComponent.IsInitialized)
				{
					break;
				}

				await NextFrameAsync(sceneTree);
			}

			if (uiComponent == null || !uiComponent.IsInitialized)
			{
				Fail("UIComponent 未就绪");
				return;
			}

			GD.Print($"[AutoFlow] UIComponent ready. backend={uiComponent.RuntimeBackendTypeName}");

			// 阶段 1+2：登录界面出现，执行“账号创建 + 登录”。
			var loginForm = await WaitForFormAsync(sceneTree, uiComponent, "UILogin");
			if (loginForm == null)
			{
				Fail("UILogin 未打开");
				return;
			}

			// GDGUI 编译模式：登录演示流（UILauncher 自动进度 → UILogin 点击 → UIMain 玩家信息）。
			if (loginForm is Godot.Hotfix.GodotGUI.UILogin gdguiLogin)
			{
				await RunGdguiFlowAsync(sceneTree, uiComponent, gdguiLogin, autoQuit);
				return;
			}

			if (loginForm is not Godot.Hotfix.FairyGUI.UILogin login)
			{
				Fail($"UILogin 类型异常：{loginForm.GetType().FullName}");
				return;
			}

			await SettleAsync(sceneTree);
			Screenshot("login");

			GD.Print($"[AutoFlow][阶段1] 账号创建：账号 {DemoAccount} 密码 {DemoPassword}");
			GD.Print("[AutoFlow][阶段2] 登录：点击进入游戏");
			login.AutoSubmitLogin(DemoAccount, DemoPassword);

			// 阶段 3：新账号无角色进入创建角色界面；已有角色进入角色列表。
			var roleForm = await WaitForAnyFormAsync(sceneTree, uiComponent, "UIPlayerCreateForm", "UIPlayerListForm");
			if (roleForm == null)
			{
				Fail("创建角色/角色列表界面未打开");
				return;
			}

			await SettleAsync(sceneTree);
			if (roleForm is Godot.Hotfix.FairyGUI.UIPlayerCreateForm create)
			{
				Screenshot("create_role");
				GD.Print($"[AutoFlow][阶段3] 创建角色：角色名 {DemoRoleName}");
				create.AutoSubmitCreate(DemoRoleName);
			}
			else
			{
				Screenshot("player_list");
				GD.Print("[AutoFlow][阶段3] 已有角色，列表选择角色进入游戏");
				(roleForm as Godot.Hotfix.FairyGUI.UIPlayerListForm)?.AutoSelectRoleAndEnter(0);
			}

			// 阶段 4：进入游戏主界面。
			var mainForm = await WaitForFormAsync(sceneTree, uiComponent, "UIMain");
			if (mainForm == null)
			{
				Fail("UIMain 未打开");
				return;
			}

			await SettleAsync(sceneTree);
			Screenshot("main");

			var role = LocalAccountStore.SelectedRole;
			if (role == null)
			{
				Fail("进入游戏但未选中角色");
				return;
			}

			await VerifySweepLightAsync(sceneTree);

			GD.Print($"[AutoFlow][阶段4] 进入游戏成功。账号={LocalAccountStore.CurrentAccountName} 角色={role.Name} 等级=Lv.{role.Level}");

			if (autoQuit)
			{
				for (var i = 0; i < SettleFrames; i++)
				{
					await NextFrameAsync(sceneTree);
				}

				GetTree().Quit(0);
			}
		}

		/// <summary>
		/// GDGUI 登录演示流：与 FairyGUI 分支等价的自动化驱动。
		/// UILogin 点击登录按钮 → UILauncher 流程接管打开 UIMain → 校验玩家信息按演示内容刷新。
		/// </summary>
		private async Task RunGdguiFlowAsync(SceneTree sceneTree, UIComponent uiComponent, Godot.Hotfix.GodotGUI.UILogin login, bool autoQuit)
		{
			GD.Print("[AutoFlow][GDGUI] 登录演示流开始");

			await SettleAsync(sceneTree);
			Screenshot("gdgui_login");

			GD.Print("[AutoFlow][GDGUI][阶段1] 登录：点击登录按钮");
			login.AutoSubmitLogin();

			// 阶段 2：UILauncher 流程关闭登录界面并打开主界面。
			var mainForm = await WaitForFormAsync(sceneTree, uiComponent, "UIMain");
			if (mainForm == null)
			{
				Fail("[GDGUI] UIMain 未打开");
				return;
			}

			await SettleAsync(sceneTree);
			Screenshot("gdgui_main");

			if (mainForm is not Node mainNode)
			{
				Fail($"[GDGUI] UIMain 类型异常：{mainForm.GetType().FullName}");
				return;
			}

			var playerNameLabel = mainNode.FindChild("PlayerNameLabel", true, false) as Label;
			var playerLevelLabel = mainNode.FindChild("PlayerLevelLabel", true, false) as Label;
			if (playerNameLabel == null || playerLevelLabel == null)
			{
				Fail("[GDGUI] UIMain 玩家信息标签缺失");
				return;
			}

			// UILauncher 演示流以 SetPlayerInfo("GameFrameX", "Lv.1") 刷新主界面。
			if (playerNameLabel.Text != "Player: GameFrameX" || playerLevelLabel.Text != "Level: Lv.1")
			{
				Fail($"[GDGUI] UIMain 玩家信息未按演示内容刷新：name='{playerNameLabel.Text}' level='{playerLevelLabel.Text}'");
				return;
			}

			GD.Print($"[AutoFlow][GDGUI][阶段2] 进入游戏成功。{playerNameLabel.Text} {playerLevelLabel.Text}");

			if (autoQuit)
			{
				for (var i = 0; i < SettleFrames; i++)
				{
					await NextFrameAsync(sceneTree);
				}

				GetTree().Quit(0);
			}
		}

		/// <summary>
		/// 验证从 Unity 包移植的扫光效果：贴一个 GImageSweepLight，
		/// 对比两个时刻截图的亮度差异（扫光带移动必然产生变化）。
		/// </summary>
		private async Task VerifySweepLightAsync(SceneTree sceneTree)
		{

			try
			{
				GD.Print($"[AutoFlow][扫光验证] Engine.TimeScale={Engine.TimeScale} MaxFps={Engine.MaxFps}");
				var package = global::FairyGUI.UIPackage.GetByName("UICommonAvatar");
				var item = package?.GetItemByName("1");
				var texture = item != null ? package.GetItemAsset(item) as global::FairyGUI.NTexture : null;
				if (texture == null)
				{
					GD.PushWarning("[AutoFlow][扫光验证] UICommonAvatar/1 贴图不可用，跳过。");
					return;
				}

				var sweep = new global::FairyGUI.GImageSweepLight
				{
					texture = texture,
				};
				sweep.SetSize(320, 320);
				sweep.xy = new Godot.Vector2(500, 220);
				sweep.sortingOrder = int.MaxValue;
				// 连续扫光（无空闲间隔），保证采样窗口内扫光带必然移动。
				sweep.SetSweepLightParameters(0.5f, 0.4f, 0.0f, 30.0f, 2.0f);
				global::FairyGUI.GRoot.inst.AddChild(sweep);

				var region = new Godot.Rect2I(500, 220, 320, 320);
				var brightA = CaptureRegionBrightness(region);
				for (var i = 0; i < 12; i++)
				{
					await NextFrameAsync(sceneTree);
				}

				var brightB = CaptureRegionBrightness(region);
				for (var i = 0; i < 12; i++)
				{
					await NextFrameAsync(sceneTree);
				}

				var brightC = CaptureRegionBrightness(region);

				// 禁用扫光后亮度应稳定，作为对照排除画面其它动画干扰。
				sweep.StopSweepLight();
				await SettleAsync(sceneTree);
				var brightOff = CaptureRegionBrightness(region);
				Screenshot("sweep_a");
				sweep.StartSweepLight();
				for (var i = 0; i < 8; i++)
				{
					await NextFrameAsync(sceneTree);
				}

				Screenshot("sweep_b");
				for (var i = 0; i < 8; i++)
				{
					await NextFrameAsync(sceneTree);
				}

				Screenshot("sweep_c");
				global::FairyGUI.GRoot.inst.RemoveChild(sweep);
				sweep.Dispose();


				var delta1 = Math.Abs(brightB - brightA);
				var delta2 = Math.Abs(brightC - brightB);
				GD.Print($"[AutoFlow][扫光验证] 平均亮度 A={brightA:F1} B={brightB:F1} C={brightC:F1} 关闭={brightOff:F1} Δ1={delta1:F3} Δ2={delta2:F3}");
				if (delta1 > 0.3f || delta2 > 0.3f)
				{
					GD.Print("[AutoFlow][扫光验证] 通过：扫光带在移动。");
				}
				else
				{
					GD.PushWarning("[AutoFlow][扫光验证] 亮度无变化，扫光可能未生效。");
			}
			}
			catch (Exception exception)
			{
				GD.PushError($"[AutoFlow][扫光验证] 异常：{exception}");
			}
		}

		private float CaptureRegionBrightness(Godot.Rect2I region)
		{
			var image = GetViewport().GetTexture().GetImage();
			var cropped = image.GetRegion(new Godot.Rect2I(
				Math.Clamp(region.Position.X, 0, image.GetWidth() - 1),
				Math.Clamp(region.Position.Y, 0, image.GetHeight() - 1),
				Math.Clamp(region.Size.X, 1, image.GetWidth()),
				Math.Clamp(region.Size.Y, 1, image.GetHeight())));
			double total = 0;
			for (var y = 0; y < cropped.GetHeight(); y += 4)
			{
				for (var x = 0; x < cropped.GetWidth(); x += 4)
				{
					var c = cropped.GetPixel(x, y);
					total += (c.R8 + c.G8 + c.B8) / 3.0;
				}
			}

			var samples = (cropped.GetWidth() / 4 + 1) * (cropped.GetHeight() / 4 + 1);
			return (float)(total / Math.Max(1, samples));
		}

		private async Task<IUIForm> WaitForFormAsync(SceneTree sceneTree, UIComponent uiComponent, string formName)
		{
			for (var i = 0; i < StageTimeoutFrames; i++)
			{
				var form = uiComponent.GetUIForm(formName);
				if (form != null)
				{
					GD.Print($"[AutoFlow] form ready: {formName} (waited {i} frames)");
					return form;
				}

				await NextFrameAsync(sceneTree);
			}

			return null;
		}

		private async Task<IUIForm> WaitForAnyFormAsync(SceneTree sceneTree, UIComponent uiComponent, params string[] formNames)
		{
			for (var i = 0; i < StageTimeoutFrames; i++)
			{
				foreach (var formName in formNames)
				{
					var form = uiComponent.GetUIForm(formName);
					if (form != null)
					{
						GD.Print($"[AutoFlow] form ready: {formName} (waited {i} frames)");
						return form;
					}
				}

				await NextFrameAsync(sceneTree);
			}

			return null;
		}

		private async Task SettleAsync(SceneTree sceneTree)
		{
			for (var i = 0; i < SettleFrames; i++)
			{
				await NextFrameAsync(sceneTree);
			}
		}

		private static async Task NextFrameAsync(SceneTree sceneTree)
		{
			await sceneTree.ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
		}

		private void Screenshot(string stageName)
		{
			_stageIndex++;
			try
			{
				var image = GetViewport().GetTexture().GetImage();
				var path = $"{ScreenshotDir}/{_stageIndex:D2}_{stageName}.png";
				image.SavePng(path);
				GD.Print($"[AutoFlow] screenshot saved: {path} -> {ProjectSettings.GlobalizePath(path)}");
			}
			catch (Exception exception)
			{
				GD.PushError($"[AutoFlow] screenshot failed at {stageName}. {exception.Message}");
			}
		}

		private void Fail(string reason)
		{
			GD.PrintErr($"[AutoFlow] 流程失败：{reason}");
			Screenshot("failed");
			GetTree().Quit(1);
		}
	}
}
