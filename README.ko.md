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

인디 게임 개발자를 위한 올인원 솔루션 · 인디 개발자의 꿈을 실현

<br />

[문서](https://gameframex.doc.alianblank.com) · [빠른 시작](#빠른-시작) · QQ 그룹: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | **한국어**

</div>

## 프로젝트 개요

GameFrameX Godot은 GameFrameX 프레임워크를 위한 Godot .NET 프로젝트이자 통합 샘플입니다. 재사용 가능한 런타임 모듈, 에디터 플러그인, 핫픽스 어셈블리, 리소스 기반 UI 샘플과 검증 씬을 통합합니다.

### 기능

- 수명 주기, 오브젝트 풀, 참조 풀, 이벤트, FSM, 타이머, 엔티티, 설정 및 전역 설정을 위한 모듈형 런타임 컴포넌트.
- 리소스 패키지, 버전 및 업데이트 확인, 다운로드 파이프라인, 프로시저, 핫픽스 어셈블리 로딩 워크플로.
- 네트워크, Web, Web Protobuf, 로컬라이제이션, 설정 테이블 및 채널 유틸리티.
- Godot GUI와 FairyGUI UI 어댑터 및 샘플. 폼 수명 주기, 그룹, 깊이, 비동기 로딩을 지원합니다.
- 에셋, 설정, 프로토콜, UI 및 프로젝트 워크플로를 확인하는 에디터 플러그인과 런타임 검증 씬.

## 빠른 시작

### 설치

필요 환경:

- Godot .NET 4.7.1.
- .NET 8 SDK. Android 빌드는 프로젝트 설정에 따라 .NET 9를 사용합니다.

저장소를 복제하고 Godot .NET 에디터에서 프로젝트 디렉터리를 연 다음 .NET 종속성을 복원합니다. GameFrameX 모듈은 `addons/`에 있으며 핵심 모듈은 `project.godot`에서 활성화됩니다.

실행할 수 있는 샘플 씬:

- `Scenes/Launcher.tscn`: 씬에 설정된 컴포넌트 체인.
- `Scenes/LauncherAuto.tscn`: 코드로 생성하는 컴포넌트 체인.
- `Scenes/Verification/AssetSystemRuntimeVerifier.tscn`: 에셋 시스템 런타임 검사.
- `Scenes/Verification/ProtoMessageRuntimeVerifier.tscn`: Protobuf HTTP 루프백 검사.

## 사용 예시

등록된 런타임 컴포넌트는 `GameEntry`를 통해 가져올 수 있습니다:

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

모듈별 설정과 API는 해당 `addons/com.gameframex.godot.*` 디렉터리의 README를 참조하세요.

## 아키텍처

- `addons/`: 재사용 가능한 GameFrameX Godot 모듈과 에디터 플러그인.
- `Scripts/`: 프로젝트 수준의 시작, 프로시저, UI 흐름 및 검증 스크립트.
- `Assets/`: 핫픽스 어셈블리, 생성된 설정 코드, UI 리소스 및 런타임 데이터.

`Scenes/Launcher.tscn`은 표준 컴포넌트 체인을 직렬화합니다. `Scenes/LauncherAuto.tscn`은 C#으로 동일한 체인을 구성하여 코드 기반 시작을 검증합니다.

## 플랫폼 지원

프로젝트는 Godot 4.7 .NET을 대상으로 하며 Mobile 렌더링 기능을 선언합니다. 일반 .NET 8 대상과 Android 전용 .NET 9 대상이 포함되어 있습니다. 출시 전 각 대상 플랫폼의 내보내기 동작을 검증하세요.

## 의존성

- Godot .NET SDK 4.7.1.
- .NET 8 SDK. Android 설정 대상은 .NET 9입니다.
- `SharpZipLib` 1.4.2.
- `addons/`의 GameFrameX 모듈: Asset, AssetSystem, Config, Download, Entity, Event, FSM, Localization, Network, Procedure, Setting, Timer, UI, Web, Web Protobuf.
- FairyGUI 및 GDGUI 모듈의 선택적 UI 통합.

## 문서 및 자료

- [GameFrameX 문서](https://gameframex.doc.alianblank.com)
- [GameFrameX GitHub 조직](https://github.com/GameFrameX)
- [GameFrameX Gitee 조직](https://gitee.com/GameFrameX)
- [Godot 문서](https://docs.godotengine.org/)
- [저장소 릴리스](https://github.com/GameFrameX/GameFrameX.Godot/releases)

## 커뮤니티 및 지원

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

## 변경 로그

프로젝트 변경 사항은 [저장소 릴리스](https://github.com/GameFrameX/GameFrameX.Godot/releases)와 [커밋 기록](https://github.com/GameFrameX/GameFrameX.Godot/commits/main/)에서 확인할 수 있습니다.

## 라이선스

자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.

<!--
EN: See [LICENSE](LICENSE) for license information.
zh-CN: 详见 [LICENSE](LICENSE) 文件。
zh-TW: 詳見 [LICENSE](LICENSE) 檔案。
ja: 詳しくは [LICENSE](LICENSE) をご参照ください。
ko: 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.
-->
