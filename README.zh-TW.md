<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX Godot

[![License](https://img.shields.io/badge/license-blue.svg)](LICENSE)
[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Godot)](https://github.com/GameFrameX/GameFrameX.Godot/releases)
[![Godot Version](https://img.shields.io/badge/Godot-4.7-blue?logo=godotengine)](https://godotengine.org/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

[![Discord](https://img.shields.io/badge/-5865F2?logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/-181717?logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[![Bilibili](https://img.shields.io/badge/-00A1D6?logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/-C71D23?logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)

獨立遊戲前後端一體化解決方案 · 獨立遊戲開發者的圓夢大使

<br />

[文檔](https://gameframex.doc.alianblank.com) · [快速開始](#快速開始) · QQ群: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | **繁體中文** | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## 項目簡介

GameFrameX Godot 是 GameFrameX 框架的 Godot .NET 專案與整合範例，整合可重複使用的執行階段模組、編輯器外掛、熱更新組件、資源驅動 UI 範例與驗證場景。

### 功能特性

- 提供生命週期、物件池、參考池、事件、有限狀態機、計時器、實體、設定與全域設定等模組化執行階段元件。
- 提供資源包、版本與更新檢查、下載流程、流程狀態及熱更新組件載入能力。
- 提供網路、Web、Web Protobuf、本地化、設定表與頻道工具。
- 提供 Godot GUI 與 FairyGUI UI 適配及範例，支援介面生命週期、分組、層級與非同步載入。
- 提供編輯器外掛，以及用於資源、設定、協定、UI 與專案工作流程檢查的驗證場景。

## 快速開始

### 安裝

環境需求：

- Godot .NET 4.7.1。
- .NET 8 SDK；Android 建置依專案設定使用 .NET 9。

複製儲存庫，在 Godot .NET 編輯器中開啟專案目錄，並等待 .NET 依賴還原。GameFrameX 模組位於 `addons/`，核心模組透過 `project.godot` 啟用。

可執行以下場景：

- `Scenes/Launcher.tscn`：使用場景設定的元件鏈。
- `Scenes/LauncherAuto.tscn`：使用程式碼建立的元件鏈。
- `Scenes/Verification/AssetSystemRuntimeVerifier.tscn`：資源系統執行階段檢查。
- `Scenes/Verification/ProtoMessageRuntimeVerifier.tscn`：Protobuf HTTP 回環檢查。

## 使用範例

已註冊的執行階段元件可以透過 `GameEntry` 取得：

```csharp
using Godot;
using GameFrameX.Runtime;

public partial class GameManager : Node
{
    public override void _Ready()
    {
        var objectPool = GameEntry.GetComponent<ObjectPoolComponent>();
        var ui = GameEntry.GetComponent<GameFrameX.UI.Runtime.UIComponent>();
    }
}
```

模組設定與 API 請參閱對應 `addons/com.gameframex.godot.*` 目錄中的 README。

## 架構概覽

- `addons/`：可重複使用的 GameFrameX Godot 模組與編輯器外掛。
- `Scripts/`：專案級啟動、流程、UI 流程與驗證腳本。
- `Assets/`：熱更新組件、產生的設定程式碼、UI 資源與執行階段資料。

`Scenes/Launcher.tscn` 序列化標準元件鏈；`Scenes/LauncherAuto.tscn` 使用 C# 建立等價元件鏈，用於程式碼驅動的啟動驗證。

## 平台支援

專案目標為 Godot 4.7 .NET，並宣告使用 Mobile 渲染特性。專案檔包含通用 .NET 8 目標和 Android 專用的 .NET 9 目標。發布前請在 Godot .NET 編輯器中分別驗證各目標平台的匯出行為。

## 依賴

- Godot .NET SDK 4.7.1。
- .NET 8 SDK；Android 設定目標為 .NET 9。
- `SharpZipLib` 1.4.2。
- `addons/` 中的 GameFrameX 模組，包括 Asset、AssetSystem、Config、Download、Entity、Event、FSM、Localization、Network、Procedure、Setting、Timer、UI、Web 與 Web Protobuf。
- FairyGUI 與 GDGUI 模組提供可選的 UI 整合。

## 文檔與資源

- [GameFrameX 文檔](https://gameframex.doc.alianblank.com)
- [GameFrameX GitHub 組織](https://github.com/GameFrameX)
- [GameFrameX Gitee 組織](https://gitee.com/GameFrameX)
- [Godot 文檔](https://docs.godotengine.org/)
- [儲存庫發佈版本](https://github.com/GameFrameX/GameFrameX.Godot/releases)

## 社區與支援

![QQ](https://img.shields.io/badge/QQ-467608841%2F233840761-EB1923?style=for-the-badge&logo=qq&logoColor=white)
[![Bilibili](https://img.shields.io/badge/Bilibili-00A1D6?style=for-the-badge&logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/Gitee-C71D23?style=for-the-badge&logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)
[![Discord](https://img.shields.io/badge/Discord-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[<img src="https://cdn.jsdelivr.net/npm/devicon@2/icons/linkedin/linkedin-original.svg" height="28" alt="LinkedIn" />](https://www.linkedin.com/in/alianblank)
[![Reddit](https://img.shields.io/badge/Reddit-FF4500?style=for-the-badge&logo=reddit&logoColor=white)](https://www.reddit.com/r/GameFrameX/)
[![X](https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white)](https://x.com/alian_blank)
[![YouTube](https://img.shields.io/badge/YouTube-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://www.youtube.com/channel/UCD9QhSFJ5xZkn5NTSV-DVAw)
[![Bluesky](https://img.shields.io/badge/Bluesky-0285FF?style=for-the-badge&logo=bluesky&logoColor=white)](https://bsky.app/profile/alianblank.bsky.social)

## 更新日誌

專案變更請參閱[儲存庫發佈版本](https://github.com/GameFrameX/GameFrameX.Godot/releases)與[提交歷史](https://github.com/GameFrameX/GameFrameX.Godot/commits/main/)。

## 開源協議

詳見 [LICENSE](LICENSE) 檔案。

<!--
EN: See [LICENSE](LICENSE) for license information.
zh-CN: 详见 [LICENSE](LICENSE) 文件。
zh-TW: 詳見 [LICENSE](LICENSE) 檔案。
ja: 詳しくは [LICENSE](LICENSE) をご参照ください。
ko: 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.
-->
