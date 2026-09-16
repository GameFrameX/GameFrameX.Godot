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

			GD.Print($"[AutoFlow][阶段4] 进入游戏成功。账号={LocalAccountStore.CurrentAccountName} 角色={role.Name} 等级=Lv.{role.Level}");
			GD.Print("[AutoFlow] === 全流程完成：账号创建 → 登录 → 创建角色 → 进入游戏 ===");

			if (autoQuit)
			{
				for (var i = 0; i < SettleFrames; i++)
				{
					await NextFrameAsync(sceneTree);
				}

				GetTree().Quit(0);
			}
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
