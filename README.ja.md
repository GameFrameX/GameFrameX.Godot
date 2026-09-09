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

インディゲーム開発者向けオールインワンソリューション · インディ開発者の夢を支援

<br />

[ドキュメント](https://gameframex.doc.alianblank.com) · [クイックスタート](#クイックスタート) · QQグループ: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | **日本語** | [한국어](README.ko.md)

</div>

## プロジェクト概要

GameFrameX Godot は、GameFrameX フレームワーク向けの Godot .NET プロジェクトおよび統合サンプルです。再利用可能なランタイムモジュール、エディタープラグイン、ホットフィックスアセンブリ、リソース駆動 UI サンプル、検証シーンをまとめています。

### 機能概要

- ライフサイクル、オブジェクトプール、参照プール、イベント、FSM、タイマー、エンティティ、設定、グローバル設定のモジュール型ランタイムコンポーネント。
- リソースパッケージ、バージョン確認、更新、ダウンロード、プロシージャ、ホットフィックスアセンブリ読み込みのワークフロー。
- ネットワーク、Web、Web Protobuf、ローカライズ、設定テーブル、チャンネルユーティリティ。
- Godot GUI と FairyGUI の UI アダプターおよびサンプル。フォームのライフサイクル、グループ、深度、非同期読み込みに対応。
- アセット、設定、プロトコル、UI、プロジェクトワークフローを確認するエディタープラグインと検証シーン。

## クイックスタート

### インストール

必要な環境：

- Godot .NET 4.7.1。
- .NET 8 SDK。Android ビルドではプロジェクト設定により .NET 9 を使用します。

リポジトリをクローンし、Godot .NET エディターでプロジェクトディレクトリを開いて .NET 依存関係を復元します。GameFrameX モジュールは `addons/` に配置され、コアモジュールは `project.godot` で有効化されています。

実行できるサンプルシーン：

- `Scenes/Launcher.tscn`：シーン設定によるコンポーネントチェーン。
- `Scenes/LauncherAuto.tscn`：コードで作成するコンポーネントチェーン。
- `Scenes/Verification/AssetSystemRuntimeVerifier.tscn`：アセットシステムのランタイム検証。
- `Scenes/Verification/ProtoMessageRuntimeVerifier.tscn`：Protobuf HTTP ループバック検証。

## 使用例

登録済みのランタイムコンポーネントは `GameEntry` から取得できます：

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

モジュール固有の設定と API については、対応する `addons/com.gameframex.godot.*` ディレクトリの README を参照してください。

## アーキテクチャ

- `addons/`：再利用可能な GameFrameX Godot モジュールとエディタープラグイン。
- `Scripts/`：プロジェクト固有の起動、プロシージャ、UI フロー、検証スクリプト。
- `Assets/`：ホットフィックスアセンブリ、生成された設定コード、UI リソース、ランタイムデータ。

`Scenes/Launcher.tscn` は標準コンポーネントチェーンをシリアライズします。`Scenes/LauncherAuto.tscn` は C# で同等のチェーンを構築し、コード駆動の起動を検証します。

## プラットフォーム対応

プロジェクトは Godot 4.7 .NET を対象とし、Mobile レンダリング機能を宣言しています。通常の .NET 8 ターゲットと Android 専用の .NET 9 ターゲットが含まれます。出荷前に各対象プラットフォームのエクスポートを検証してください。

## 依存関係

- Godot .NET SDK 4.7.1。
- .NET 8 SDK。Android 設定のターゲットは .NET 9 です。
- `SharpZipLib` 1.4.2。
- `addons/` の GameFrameX モジュール（Asset、AssetSystem、Config、Download、Entity、Event、FSM、Localization、Network、Procedure、Setting、Timer、UI、Web、Web Protobuf）。
- FairyGUI と GDGUI モジュールによるオプションの UI 統合。

## ドキュメントとリソース

- [GameFrameX ドキュメント](https://gameframex.doc.alianblank.com)
- [GameFrameX GitHub 組織](https://github.com/GameFrameX)
- [GameFrameX Gitee 組織](https://gitee.com/GameFrameX)
- [Godot ドキュメント](https://docs.godotengine.org/)
- [リポジトリのリリース](https://github.com/GameFrameX/GameFrameX.Godot/releases)

## コミュニティとサポート

[![Discord](https://img.shields.io/badge/Discord-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[<img src="https://cdn.jsdelivr.net/npm/devicon@2/icons/linkedin/linkedin-original.svg" height="28" alt="LinkedIn" />](https://www.linkedin.com/in/alianblank)
[![Reddit](https://img.shields.io/badge/Reddit-FF4500?style=for-the-badge&logo=reddit&logoColor=white)](https://www.reddit.com/r/GameFrameX/)
[![X](https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white)](https://x.com/alian_blank)
[![YouTube](https://img.shields.io/badge/YouTube-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://www.youtube.com/channel/UCD9QhSFJ5xZkn5NTSV-DVAw)
[![Bluesky](https://img.shields.io/badge/Bluesky-0285FF?style=for-the-badge&logo=bluesky&logoColor=white)](https://bsky.app/profile/alianblank.bsky.social)
[![Bilibili](https://img.shields.io/badge/Bilibili-00A1D6?style=for-the-badge&logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/Gitee-C71D23?style=for-the-badge&logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)
![QQ](https://img.shields.io/badge/QQ-467608841%2F233840761-EB1923?style=for-the-badge&logo=qq&logoColor=white)

## 変更履歴

変更については、[リポジトリのリリース](https://github.com/GameFrameX/GameFrameX.Godot/releases)と[コミット履歴](https://github.com/GameFrameX/GameFrameX.Godot/commits/main/)を参照してください。

## ライセンス

詳しくは [LICENSE](LICENSE) をご参照ください。

<!--
EN: See [LICENSE](LICENSE) for license information.
zh-CN: 详见 [LICENSE](LICENSE) 文件。
zh-TW: 詳見 [LICENSE](LICENSE) 檔案。
ja: 詳しくは [LICENSE](LICENSE) をご参照ください。
ko: 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.
-->
