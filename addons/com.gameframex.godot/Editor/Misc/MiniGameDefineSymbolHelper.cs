#if TOOLS
using System;
using System.Collections.Generic;
using Godot;

namespace GameFrameX.Editor
{
    /// <summary>
    /// 小游戏平台分类。（对应 Unity 版 Editor/MiniGame 目录划分）
    /// </summary>
    public enum MiniGamePlatformCategory
    {
        /// <summary>
        /// 国内小游戏（DomesticMiniGames）。
        /// </summary>
        DomesticMiniGame,

        /// <summary>
        /// 国际小游戏（InternationalMiniGames）。
        /// </summary>
        InternationalMiniGame,

        /// <summary>
        /// 设备厂商渠道（DeviceOEMs）。
        /// </summary>
        DeviceOEM,

        /// <summary>
        /// 游戏平台（GamePlatforms）。
        /// </summary>
        GamePlatform,
    }

    /// <summary>
    /// 小游戏平台宏定义信息。
    /// </summary>
    public sealed class MiniGamePlatformInfo
    {
        /// <summary>
        /// 平台唯一标识（如 WeChat）。
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 中文平台名。
        /// </summary>
        public string NameZh { get; }

        /// <summary>
        /// 英文平台名。
        /// </summary>
        public string NameEn { get; }

        /// <summary>
        /// 平台分类。
        /// </summary>
        public MiniGamePlatformCategory Category { get; }

        /// <summary>
        /// 平台宏定义集合（首个为主宏 ENABLE_XXX_MINI_GAME，其余为厂商配套宏）。
        /// </summary>
        public string[] Symbols { get; }

        /// <summary>
        /// 主宏定义。
        /// </summary>
        public string PrimarySymbol => Symbols[0];

        /// <summary>
        /// 初始化小游戏平台宏定义信息。
        /// </summary>
        public MiniGamePlatformInfo(string key, string nameZh, string nameEn, MiniGamePlatformCategory category, string[] symbols)
        {
            Key = key;
            NameZh = nameZh;
            NameEn = nameEn;
            Category = category;
            Symbols = symbols;
        }
    }

    /// <summary>
    /// 小游戏宏定义帮助类。（对应 Unity 版 MiniGameDefineSymbolHelper，数据驱动实现）
    /// </summary>
    /// <remarks>
    /// 语义与 Unity 版一致：同一时刻仅允许一个小游戏平台宏；启用某平台时先关闭其它平台宏，
    /// 再开启该平台全部宏，并联动统一宏 ENABLE_WEBGL_MINI_GAME；全部平台关闭时统一宏自动移除。
    /// </remarks>
    public static class MiniGameDefineSymbolHelper
    {
        /// <summary>
        /// 小游戏统一宏定义。
        /// </summary>
        private const string UnifiedMiniGameScriptingDefineSymbol = "ENABLE_WEBGL_MINI_GAME";

        /// <summary>
        /// 统一宏刷新重入保护。
        /// </summary>
        private static bool m_Refreshing;

        /// <summary>
        /// 全部小游戏平台定义（宏集合与 Unity 版逐字对齐，含 OPPOSMINIGAME 等 Unity 原始拼写）。
        /// </summary>
        private static readonly MiniGamePlatformInfo[] Platforms = new[]
        {
            // 国内小游戏 DomesticMiniGames
            new MiniGamePlatformInfo("WeChat", "微信", "WeChat", MiniGamePlatformCategory.DomesticMiniGame, new[] { "ENABLE_WECHAT_MINI_GAME", "WEIXINMINIGAME" }),
            new MiniGamePlatformInfo("DouYin", "抖音", "DouYin", MiniGamePlatformCategory.DomesticMiniGame, new[] { "ENABLE_DOUYIN_MINI_GAME", "DOUYINMINIGAME", "TTSDK_MIX_ENGINE" }),
            new MiniGamePlatformInfo("KuaiShou", "快手", "KuaiShou", MiniGamePlatformCategory.DomesticMiniGame, new[] { "ENABLE_KUAISHOU_MINI_GAME", "KUAISHOUMINIGAME" }),
            new MiniGamePlatformInfo("Baidu", "百度", "Baidu", MiniGamePlatformCategory.DomesticMiniGame, new[] { "ENABLE_BAIDU_MINI_GAME", "BAIDUMINIGAME" }),
            new MiniGamePlatformInfo("Alipay", "支付宝", "Alipay", MiniGamePlatformCategory.DomesticMiniGame, new[] { "ENABLE_ALIPAY_MINI_GAME", "ALIPAYMINIGAME" }),
            new MiniGamePlatformInfo("Meituan", "美团", "Meituan", MiniGamePlatformCategory.DomesticMiniGame, new[] { "ENABLE_MEITUAN_MINI_GAME", "MEITUANMINIGAME" }),
            new MiniGamePlatformInfo("Bilibili", "哔哩哔哩", "Bilibili", MiniGamePlatformCategory.DomesticMiniGame, new[] { "ENABLE_BILIBILI_MINI_GAME", "BILIBILIMINIGAME" }),
            new MiniGamePlatformInfo("JingDong", "京东", "JingDong", MiniGamePlatformCategory.DomesticMiniGame, new[] { "ENABLE_JINGDONG_MINI_GAME", "JINGDONGMINIGAME" }),
            new MiniGamePlatformInfo("Taobao", "淘宝", "Taobao", MiniGamePlatformCategory.DomesticMiniGame, new[] { "ENABLE_TAOBAO_MINI_GAME", "TAOBAOMINIGAME" }),

            // 国际小游戏 InternationalMiniGames
            new MiniGamePlatformInfo("Discord", "Discord", "Discord", MiniGamePlatformCategory.InternationalMiniGame, new[] { "ENABLE_DISCORD_MINI_GAME", "DISCORDMINIGAME" }),
            new MiniGamePlatformInfo("YouTube", "YouTube", "YouTube", MiniGamePlatformCategory.InternationalMiniGame, new[] { "ENABLE_YOUTUBE_MINI_GAME", "YOUTUBEMINIGAME" }),
            new MiniGamePlatformInfo("Facebook", "Facebook", "Facebook", MiniGamePlatformCategory.InternationalMiniGame, new[] { "ENABLE_FACEBOOK_MINI_GAME", "FACEBOOKMINIGAME" }),
            new MiniGamePlatformInfo("GooglePlay", "Google Play", "Google Play", MiniGamePlatformCategory.InternationalMiniGame, new[] { "ENABLE_GOOGLEPLAY_MINI_GAME", "GOOGLEPLAYMINIGAME" }),
            new MiniGamePlatformInfo("TikTok", "TikTok", "TikTok", MiniGamePlatformCategory.InternationalMiniGame, new[] { "ENABLE_TIKTOK_MINI_GAME", "TIKTOKMINIGAME" }),
            new MiniGamePlatformInfo("CrazyGames", "CrazyGames", "CrazyGames", MiniGamePlatformCategory.InternationalMiniGame, new[] { "ENABLE_CRAZYGAMES_MINI_GAME", "CRAZYGAMESMINIGAME" }),
            new MiniGamePlatformInfo("Poki", "Poki", "Poki", MiniGamePlatformCategory.InternationalMiniGame, new[] { "ENABLE_POKI_MINI_GAME", "POKIMINIGAME" }),

            // 设备厂商 DeviceOEMs
            new MiniGamePlatformInfo("Vivo", "vivo", "Vivo", MiniGamePlatformCategory.DeviceOEM, new[] { "ENABLE_VIVO_MINI_GAME", "VIVOMINIGAME" }),
            new MiniGamePlatformInfo("OPPO", "OPPO", "OPPO", MiniGamePlatformCategory.DeviceOEM, new[] { "ENABLE_OPPO_MINI_GAME", "OPPOSMINIGAME" }),
            new MiniGamePlatformInfo("Xiaomi", "小米", "Xiaomi", MiniGamePlatformCategory.DeviceOEM, new[] { "ENABLE_XIAOMI_MINI_GAME", "XIAOMIMINIGAME" }),
            new MiniGamePlatformInfo("Huawei", "华为", "Huawei", MiniGamePlatformCategory.DeviceOEM, new[] { "ENABLE_HUAWEI_MINI_GAME", "HUAWEIMINIGAME" }),

            // 游戏平台 GamePlatforms
            new MiniGamePlatformInfo("TapTap", "TapTap", "TapTap", MiniGamePlatformCategory.GamePlatform, new[] { "ENABLE_TAPTAP_MINI_GAME", "TAPTAPMINIGAME" }),
        };

        /// <summary>
        /// 首次访问时订阅宏变更事件：任何路径（菜单/宏窗口/外部对齐）修改宏后，统一宏状态都对齐。
        /// </summary>
        static MiniGameDefineSymbolHelper()
        {
            ScriptingDefineSymbols.DefineSymbolsChanged += OnDefineSymbolsChanged;
        }

        /// <summary>
        /// 获取指定分类下的全部小游戏平台。
        /// </summary>
        /// <param name="category">平台分类。</param>
        /// <returns>该分类下的平台列表。</returns>
        public static IReadOnlyList<MiniGamePlatformInfo> GetPlatforms(MiniGamePlatformCategory category)
        {
            var result = new List<MiniGamePlatformInfo>();
            foreach (var platform in Platforms)
            {
                if (platform.Category == category)
                {
                    result.Add(platform);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取全部小游戏平台。
        /// </summary>
        /// <returns>全部平台列表。</returns>
        public static IReadOnlyList<MiniGamePlatformInfo> GetAllPlatforms()
        {
            return Platforms;
        }

        /// <summary>
        /// 按主宏定义查找平台。
        /// </summary>
        /// <param name="primarySymbol">平台主宏定义。</param>
        /// <returns>匹配的平台；未找到时返回 null。</returns>
        public static MiniGamePlatformInfo GetPlatformBySymbol(string primarySymbol)
        {
            foreach (var platform in Platforms)
            {
                if (string.Equals(platform.PrimarySymbol, primarySymbol, StringComparison.Ordinal))
                {
                    return platform;
                }
            }

            return null;
        }

        /// <summary>
        /// 开启指定小游戏平台的适配宏定义，并互斥关闭其它平台。
        /// </summary>
        /// <param name="key">平台唯一标识。</param>
        public static void Enable(string key)
        {
            var platform = FindPlatform(key);
            if (platform == null)
            {
                return;
            }

            DisableOtherMiniGameScriptingDefineSymbols(platform);
            foreach (var define in platform.Symbols)
            {
                ScriptingDefineSymbols.AddScriptingDefineSymbol(define);
            }

            RefreshUnifiedMiniGameScriptingDefineSymbol();
            GD.Print($"小游戏宏定义 [{string.Join(", ", platform.Symbols)}] 已经打开");
        }

        /// <summary>
        /// 关闭指定小游戏平台的适配宏定义。
        /// </summary>
        /// <param name="key">平台唯一标识。</param>
        public static void Disable(string key)
        {
            var platform = FindPlatform(key);
            if (platform == null)
            {
                return;
            }

            foreach (var define in platform.Symbols)
            {
                ScriptingDefineSymbols.RemoveScriptingDefineSymbol(define);
            }

            RefreshUnifiedMiniGameScriptingDefineSymbol();
            GD.Print($"小游戏宏定义 [{string.Join(", ", platform.Symbols)}] 已经关闭");
        }

        /// <summary>
        /// 关闭除指定平台以外的其它小游戏平台宏定义。
        /// </summary>
        /// <param name="currentPlatform">保留的平台。</param>
        private static void DisableOtherMiniGameScriptingDefineSymbols(MiniGamePlatformInfo currentPlatform)
        {
            foreach (var platform in Platforms)
            {
                if (ReferenceEquals(platform, currentPlatform))
                {
                    continue;
                }

                foreach (var define in platform.Symbols)
                {
                    ScriptingDefineSymbols.RemoveScriptingDefineSymbol(define);
                }
            }
        }

        /// <summary>
        /// 根据当前各平台宏定义状态刷新小游戏统一宏定义：任一平台开启则统一宏开启，否则移除。
        /// </summary>
        public static void RefreshUnifiedMiniGameScriptingDefineSymbol()
        {
            if (m_Refreshing)
            {
                return;
            }

            m_Refreshing = true;
            try
            {
                bool hasAnyMiniGameDefine = false;
                foreach (var platform in Platforms)
                {
                    foreach (var define in platform.Symbols)
                    {
                        if (ScriptingDefineSymbols.HasScriptingDefineSymbol(define))
                        {
                            hasAnyMiniGameDefine = true;
                            break;
                        }
                    }

                    if (hasAnyMiniGameDefine)
                    {
                        break;
                    }
                }

                bool hasUnifiedMiniGameDefine = ScriptingDefineSymbols.HasScriptingDefineSymbol(UnifiedMiniGameScriptingDefineSymbol);
                if (hasAnyMiniGameDefine && !hasUnifiedMiniGameDefine)
                {
                    ScriptingDefineSymbols.AddScriptingDefineSymbol(UnifiedMiniGameScriptingDefineSymbol);
                }
                else if (!hasAnyMiniGameDefine && hasUnifiedMiniGameDefine)
                {
                    ScriptingDefineSymbols.RemoveScriptingDefineSymbol(UnifiedMiniGameScriptingDefineSymbol);
                }
            }
            finally
            {
                m_Refreshing = false;
            }
        }

        private static void OnDefineSymbolsChanged()
        {
            try
            {
                RefreshUnifiedMiniGameScriptingDefineSymbol();
            }
            catch (Exception exception)
            {
                GD.PushWarning($"小游戏统一宏定义刷新失败: {exception.Message}");
            }
        }

        private static MiniGamePlatformInfo FindPlatform(string key)
        {
            foreach (var platform in Platforms)
            {
                if (string.Equals(platform.Key, key, StringComparison.Ordinal))
                {
                    return platform;
                }
            }

            GD.PushWarning($"未知的小游戏平台标识: {key}");
            return null;
        }
    }
}
#endif
