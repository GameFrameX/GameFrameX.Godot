using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Godot;

namespace Godot.Startup.Account
{
	/// <summary>
	/// 本地账号与角色存储（客户端演示用，JSON 持久化到 user://account_store.json）。
	/// 承担流程：账号创建（不存在即注册）→ 登录校验 → 角色列表/创建角色 → 进入游戏。
	/// 说明：当前 Godot 移植尚未接服务器，账号体系先落本地；接入网络层后替换为远程协议。
	/// </summary>
	public static class LocalAccountStore
	{
		private const string StoreFilePath = "user://account_store.json";

		private static readonly object Gate = new();
		private static StoreData _store;

		/// <summary>当前登录的账号名；未登录为 null。</summary>
		public static string CurrentAccountName { get; private set; }

		/// <summary>当前选中/新建的角色；未确定为 null。</summary>
		public static RoleRecord SelectedRole { get; private set; }

		/// <summary>
		/// 登录：账号不存在时自动创建（账号创建步骤），存在时校验密码。
		/// </summary>
		/// <param name="account">账号名。</param>
		/// <param name="password">密码明文。</param>
		/// <param name="created">输出：本次是否新建了账号。</param>
		/// <param name="error">输出：失败原因（账号存在但密码错误等）。</param>
		/// <returns>登录是否成功。</returns>
		public static bool TryLoginOrCreate(string account, string password, out bool created, out string error)
		{
			created = false;
			error = string.Empty;
			if (string.IsNullOrWhiteSpace(account))
			{
				error = "账号不能为空";
				return false;
			}

			if (string.IsNullOrEmpty(password))
			{
				error = "密码不能为空";
				return false;
			}

			lock (Gate)
			{
				var store = Load();
				if (!store.Accounts.TryGetValue(account, out var record))
				{
					record = new AccountRecord
					{
						Account = account,
						PasswordHash = HashPassword(password),
						Roles = new List<RoleRecord>(),
					};
					store.Accounts.Add(account, record);
					Save(store);
					created = true;
					GD.Print($"[LocalAccountStore] 账号创建成功：{account}");
				}
				else if (record.PasswordHash != HashPassword(password))
				{
					error = "密码错误";
					GD.Print($"[LocalAccountStore] 登录失败（密码错误）：{account}");
					return false;
				}

				CurrentAccountName = account;
				SelectedRole = null;
				GD.Print($"[LocalAccountStore] 登录成功：{account} 角色数={record.Roles.Count}");
				return true;
			}
		}

		/// <summary>
		/// 获取当前账号的角色列表（未登录返回空列表）。
		/// </summary>
		public static List<RoleRecord> GetCurrentRoles()
		{
			lock (Gate)
			{
				var store = Load();
				if (CurrentAccountName == null || !store.Accounts.TryGetValue(CurrentAccountName, out var record))
				{
					return new List<RoleRecord>();
				}

				// 返回副本，避免外部改动内部状态。
				return new List<RoleRecord>(record.Roles);
			}
		}

		/// <summary>
		/// 为当前账号创建角色。角色名非空且不重复才允许创建。
		/// </summary>
		/// <param name="roleName">角色名。</param>
		/// <param name="role">输出：创建成功的角色。</param>
		/// <param name="error">输出：失败原因。</param>
		/// <returns>是否创建成功。</returns>
		public static bool TryCreateRole(string roleName, out RoleRecord role, out string error)
		{
			role = null;
			error = string.Empty;
			roleName = roleName?.Trim() ?? string.Empty;
			if (roleName.Length == 0)
			{
				error = "角色名不能为空";
				return false;
			}

			lock (Gate)
			{
				var store = Load();
				if (CurrentAccountName == null || !store.Accounts.TryGetValue(CurrentAccountName, out var record))
				{
					error = "尚未登录，请先登录";
					return false;
				}

				if (record.Roles.Exists(r => string.Equals(r.Name, roleName, StringComparison.Ordinal)))
				{
					error = "角色名已存在";
					return false;
				}

				role = new RoleRecord
				{
					Name = roleName,
					Level = 1,
					CreatedAtUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
				};
				record.Roles.Add(role);
				Save(store);
				SelectedRole = role;
				GD.Print($"[LocalAccountStore] 角色创建成功：{roleName} 账号={CurrentAccountName}");
				return true;
			}
		}

		/// <summary>
		/// 选中某个角色作为进入游戏的角色。
		/// </summary>
		public static void SelectRole(RoleRecord role)
		{
			SelectedRole = role;
			GD.Print($"[LocalAccountStore] 选中角色：{role?.Name ?? "<null>"}");
		}

		/// <summary>
		/// 清空本地存储并重置会话（自动验证用，保证每次都走“账号创建”路径）。
		/// </summary>
		public static void Reset()
		{
			lock (Gate)
			{
				_store = null;
				CurrentAccountName = null;
				SelectedRole = null;
				if (FileAccess.FileExists(StoreFilePath))
				{
				DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(StoreFilePath));
				}

				GD.Print("[LocalAccountStore] store reset.");
			}
		}

		private static StoreData Load()
		{
			if (_store != null)
			{
				return _store;
			}

			try
			{
				if (FileAccess.FileExists(StoreFilePath))
				{
					using var file = FileAccess.Open(StoreFilePath, FileAccess.ModeFlags.Read);
					var json = file.GetAsText();
					_store = JsonSerializer.Deserialize<StoreData>(json, JsonOptions) ?? new StoreData();
					return _store;
				}
			}
			catch (Exception exception)
			{
				GD.PushError($"[LocalAccountStore] load failed, fallback to empty store. {exception.Message}");
			}

			_store = new StoreData();
			return _store;
		}

		private static void Save(StoreData store)
		{
			try
			{
				var json = JsonSerializer.Serialize(store, JsonOptions);
				using var file = FileAccess.Open(StoreFilePath, FileAccess.ModeFlags.Write);
				file.StoreString(json);
			}
			catch (Exception exception)
			{
				GD.PushError($"[LocalAccountStore] save failed. {exception.Message}");
			}
		}

		private static string HashPassword(string password)
		{
			var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
			return Convert.ToHexString(bytes);
		}

		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			WriteIndented = true,
		};

		private sealed class StoreData
		{
			public Dictionary<string, AccountRecord> Accounts { get; set; } = new(StringComparer.Ordinal);
		}

		public sealed class AccountRecord
		{
			public string Account { get; set; } = string.Empty;

			public string PasswordHash { get; set; } = string.Empty;

			public List<RoleRecord> Roles { get; set; } = new();
		}

		public sealed class RoleRecord
		{
			public string Name { get; set; } = string.Empty;

			public uint Level { get; set; }

			public long CreatedAtUtc { get; set; }
		}
	}
}
