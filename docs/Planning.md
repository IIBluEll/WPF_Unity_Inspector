# Unity Project Inspector 기획서

## 1. 프로젝트 개요

### 1.1 프로젝트명
**Unity Project Inspector**

### 1.2 프로젝트 목적
Unity 프로젝트의 기본 상태와 빌드 결과를 Unity Editor 외부에서 빠르게 확인할 수 있는 **C# WPF 기반 데스크톱 분석 도구**를 제작한다.

초기 버전에서는 복잡한 Asset Dependency 분석이나 실시간 Editor 연동보다 다음 두 기능에 집중한다.

1. **Dashboard**
   - 프로젝트 기본 정보
   - 프로젝트 및 Assets 용량
   - 최근 빌드 정보
   - 빌드 결과 요약

2. **Build Analyzer**
   - Unity BuildReport 기반 빌드 분석
   - Asset별 빌드 포함 용량 확인
   - 용량이 큰 Asset 확인
   - Warning / Error 확인
   - Asset Type별 용량 분류

본 프로젝트의 핵심 목표는 다음과 같다.

> "Unity 프로젝트를 열지 않고도 최근 빌드 상태와 빌드 용량 문제를 빠르게 파악할 수 있는 개발자용 툴"

---

# 2. 개발 배경

Unity 프로젝트의 빌드 용량 문제를 확인하려면 보통 다음 과정을 거친다.

1. Unity Editor 실행
2. 프로젝트 로딩
3. Build 실행
4. Build Report 또는 Editor.log 확인
5. 용량이 큰 Asset 수동 탐색
6. Project 창에서 해당 Asset 검색
7. Import Settings 확인

프로젝트 규모가 커질수록 이 과정은 반복 작업이 된다.

특히 다음 문제를 빠르게 확인하기 어렵다.

- 어떤 Asset이 빌드 용량을 많이 차지하는가
- Audio, Texture, Model 중 어떤 분류가 가장 큰가
- 최근 빌드 결과와 출력 용량이 얼마인가
- 최근 빌드가 성공했는가
- Warning / Error가 얼마나 발생했는가

Unity Project Inspector는 이러한 정보를 별도의 WPF 프로그램에서 확인하도록 한다.

---

# 3. 타깃 사용자

## 3.1 주요 사용자

- Unity 클라이언트 개발자
- 인디 게임 개발자
- Unity 프로젝트 빌드 담당자
- QA / Technical Artist
- 프로젝트 최적화를 진행하는 개발자

## 3.2 사용 시나리오

### 시나리오 A - 빌드 용량 확인

개발자가 Android APK 빌드 후 Unity Project Inspector를 실행한다.

Dashboard에서 다음 정보를 확인한다.

- 최근 빌드 성공 여부
- 전체 빌드 크기
- 가장 많은 용량을 차지하는 Asset 종류

이후 Build Analyzer에서 용량이 큰 Asset 순으로 정렬하여 문제 Asset을 찾는다.

---

### 시나리오 B - Audio 용량 문제 추적

빌드 크기가 갑자기 증가하였다.

Build Analyzer에서 Asset Type을 `Audio`로 필터링한다.

다음과 같은 데이터를 확인한다.

| Asset | Size |
|---|---:|
| LobbyBGM.wav | 48 MB |
| StageBGM.wav | 42 MB |
| ResultBGM.wav | 31 MB |

개발자는 특정 BGM이 빌드 크기를 크게 차지하고 있음을 확인하고 Unity Import Settings를 수정한다.

---

### 시나리오 C - 프로젝트 상태 확인

여러 Unity 프로젝트를 관리하는 개발자가 프로젝트 폴더를 등록한다.

Dashboard에서 다음 정보를 빠르게 확인한다.

- Unity Version
- Project Disk Size
- Project Source Size
- Assets Size
- Last Build
- Build Platform
- Build Result

Unity Editor를 실행하지 않고 프로젝트 상태를 확인할 수 있다.

---

# 4. 프로젝트 범위

## 4.1 Version 1.0 MVP

Version 1.0에서는 다음 기능만 구현한다.

### Dashboard

- Unity 프로젝트 선택
- 프로젝트명 표시
- Unity 버전 표시
- 프로젝트 Disk / Source 용량 계산
- Assets 폴더 용량 계산
- 마지막으로 Export된 Build 정보와 Report 생성 시각 표시
- Build Result 표시
- Build Output Size 표시
- Build Time 표시
- Asset Type별 Packed Size 요약

### Build Analyzer

- Build Report JSON 로드
- Asset 목록 표시
- Asset 이름
- Asset 경로
- Asset Type
- Packed Size
- Packed Size 기준 정렬
- Asset Type 필터
- Asset 이름 검색
- Warning 목록 표시
- Error 목록 표시
- Build Summary 표시

---

# 5. 제외 범위

Version 1.0에서는 다음 기능을 구현하지 않는다.

- 실시간 Unity Editor 통신
- Asset Dependency Graph
- Missing Reference 검사
- Unused Asset 탐색
- 중복 Asset 탐색
- Addressables 분석
- Scene Dependency 분석
- Git 연동
- CI/CD 연동
- 자동 Asset Import Setting 수정
- Unity Asset 직접 수정
- 다중 프로젝트 동시 분석

위 기능은 Version 2 이후 확장 기능으로 분리한다.

---

# 6. 전체 시스템 구조

```text
┌─────────────────────────────┐
│        Unity Project        │
│                             │
│ Assets                      │
│ ProjectSettings             │
│ Packages                    │
└──────────────┬──────────────┘
               │
               │ Project Info
               ▼
┌─────────────────────────────┐
│      Unity Editor Plugin    │
│                             │
│ BuildReport                 │
│ BuildPipeline               │
│ AssetDatabase               │
└──────────────┬──────────────┘
               │
               │ JSON Export
               ▼
┌─────────────────────────────┐
│      build_report.json      │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│      WPF Application        │
│                             │
│ Dashboard                   │
│ Build Analyzer              │
└─────────────────────────────┘
```

---

# 7. 개발 기술 스택

## 7.1 Desktop Application

- C#
- .NET 9
- WPF
- XAML

## 7.2 Architecture

- MVVM
- Dependency Injection 선택 적용
- Service Layer

## 7.3 Library

### MVVM

추천:

```text
CommunityToolkit.Mvvm
```

사용 목적:

- ObservableObject
- RelayCommand
- AsyncRelayCommand
- Property Notification

### JSON

기본적으로 다음 중 하나 사용한다.

```text
System.Text.Json
```

또는

```text
Newtonsoft.Json
```

MVP에서는 별도 의존성을 줄이기 위해 `System.Text.Json` 사용을 권장한다.

---

# 8. WPF 프로젝트 구조

```text
UnityProjectInspector
│
├─ Models
│   ├─ ProjectInfo.cs
│   ├─ BuildInfo.cs
│   ├─ AssetBuildInfo.cs
│   ├─ BuildMessage.cs
│   └─ BuildReportData.cs
│
├─ ViewModels
│   ├─ MainViewModel.cs
│   ├─ DashboardViewModel.cs
│   └─ BuildAnalyzerViewModel.cs
│
├─ Views
│   ├─ MainWindow.xaml
│   ├─ DashboardView.xaml
│   └─ BuildAnalyzerView.xaml
│
├─ Services
│   ├─ ProjectAnalyzerService.cs
│   ├─ BuildReportService.cs
│   ├─ ProjectFileService.cs
│   └─ SettingsService.cs
│
├─ Converters
│   ├─ ByteSizeConverter.cs
│   └─ BuildResultConverter.cs
│
├─ Resources
│   ├─ Styles
│   └─ Icons
│
├─ App.xaml
└─ App.xaml.cs
```

---

# 9. Unity Editor Plugin 구조

Unity 프로젝트에는 WPF 프로그램과 별도로 간단한 Editor Plugin을 제공한다.

```text
Assets
└─ UnityProjectInspector
   └─ Editor
      ├─ BuildReportExporter.cs
      ├─ BuildPostProcessor.cs
      └─ Models
         └─ BuildReportDto.cs
```

Unity Plugin의 역할은 최소화한다.

주요 책임은 다음과 같다.

- Build 종료 감지
- BuildReport 데이터 변환
- JSON 생성
- 지정된 위치에 저장

MVP에서는 Unity Plugin을 로컬 UPM Package 형태로 제공한다. Unity 프로젝트의 Package Manager에서 디스크 경로를 지정해 설치하며, 패키지 안의 `Editor` 코드는 Player 빌드에 포함되지 않도록 한다.

### 9.1 수집 범위

Version 1.0의 자동 Export는 `IPostprocessBuildWithReport`가 호출되는 정상 완료 BuildReport를 대상으로 한다.

Unity에서 빌드가 이른 단계에 실패하거나 사용자가 취소한 경우에는 `OnPostprocessBuild`가 호출되지 않을 수 있다. 따라서 Version 1.0에서는 다음과 같이 상태를 구분한다.

- 정상 완료된 빌드: `Succeeded`, `Failed`, `Cancelled`, `Unknown` 중 BuildReport가 제공한 결과 표시
- 콜백 전에 중단된 빌드: JSON을 갱신하지 않음
- WPF: 보고서 생성 시각을 표시하여 오래된 보고서를 최근 빌드로 오인하지 않도록 함

콜백 이전 실패까지 수집하는 기능은 별도 빌드 실행기 또는 Editor 로그 수집 설계가 필요하므로 Version 1.1 이후 검토 대상으로 둔다.

---

# 10. 데이터 흐름

```text
Unity Build 실행

        ↓

BuildPipeline.BuildPlayer()

        ↓

BuildReport 생성

        ↓

BuildPostProcessor 실행

        ↓

BuildReport DTO 변환

        ↓

build_report.json 생성

        ↓

Unity Project Inspector 실행

        ↓

BuildReportService JSON Load

        ↓

DashboardViewModel

BuildAnalyzerViewModel

        ↓

WPF UI 표시
```

> Build가 콜백 이전에 중단되면 JSON Export 단계가 실행되지 않을 수 있다. WPF는 마지막으로 정상 Export된 보고서의 생성 시각을 함께 표시한다.

---

# 11. JSON 데이터 설계

## 11.1 Build Report JSON 예시

```json
{
  "schemaVersion": 1,
  "projectName": "Dots Arena",
  "unityVersion": "6000.3.20f1",
  "assetSizeDefinition": "Unity BuildReport PackedAssetInfo.packedSize",
  "build": {
    "buildGuid": "3d1f26d6-3798-4a94-8e3f-a730cc9ac199",
    "platform": "Android",
    "result": "Succeeded",
    "outputPath": "Builds/Android/DotsArena.apk",
    "buildStartedAtUtc": "2026-09-17T04:18:45Z",
    "buildEndedAtUtc": "2026-09-17T04:20:00Z",
    "reportGeneratedAtUtc": "2026-09-17T04:20:01Z",
    "buildTimeSeconds": 74.3,
    "outputSizeBytes": 182345678,
    "warningCount": 13,
    "errorCount": 0
  },
  "assets": [
    {
      "name": "LobbyBGM.wav",
      "path": "Assets/Audio/BGM/LobbyBGM.wav",
      "type": "Audio",
      "packedSizeBytes": 50331648
    },
    {
      "name": "Stage01.png",
      "path": "Assets/Textures/Stage01.png",
      "type": "Texture",
      "packedSizeBytes": 33554432
    }
  ],
  "messages": [
    {
      "type": "Warning",
      "message": "Shader variant stripped."
    }
  ]
}
```

---

# 12. Model 설계

## 12.1 ProjectInfo

```csharp
public class ProjectInfo
{
    public string ProjectName { get; set; }
    public string ProjectPath { get; set; }
    public string UnityVersion { get; set; }

    public long ProjectDiskSizeBytes { get; set; }
    public long ProjectSourceSizeBytes { get; set; }
    public long AssetsSizeBytes { get; set; }
    public int SkippedFileCount { get; set; }
    public bool IsPartialSizeResult { get; set; }
}
```

WPF의 `ProjectName`은 선택한 프로젝트 루트 폴더명으로 정의한다. Build Report의 `projectName`은 Unity의 `Application.productName`으로 내보내며, 두 값이 다를 수 있다.

---

## 12.2 BuildInfo

```csharp
public class BuildInfo
{
    public string BuildGuid { get; set; }
    public string Platform { get; set; }
    public string Result { get; set; }
    public string OutputPath { get; set; }

    public DateTimeOffset BuildStartedAtUtc { get; set; }
    public DateTimeOffset BuildEndedAtUtc { get; set; }
    public DateTimeOffset ReportGeneratedAtUtc { get; set; }

    public double BuildTimeSeconds { get; set; }

    public ulong OutputSizeBytes { get; set; }

    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }
}
```

---

## 12.3 AssetBuildInfo

```csharp
public class AssetBuildInfo
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string Type { get; set; }

    public ulong PackedSizeBytes { get; set; }
}
```

`PackedSizeBytes`는 원본 파일 크기나 런타임 메모리 사용량이 아니라 Unity BuildReport의 `PackedAssetInfo.packedSize` 값이다. 같은 원본 Asset에서 생성된 여러 packed 항목이 있을 수 있으므로 화면에서는 `sourceAssetPath` 기준 합산 규칙을 적용하고, 전체 빌드 출력 크기와 동일한 값으로 취급하지 않는다.

---

## 12.4 BuildMessage

```csharp
public class BuildMessage
{
    public BUILD_MESSAGE_TYPE_ENUM Type { get; set; }

    public string Message { get; set; }
}
```

```csharp
public enum BUILD_MESSAGE_TYPE_ENUM
{
    INFO,
    WARNING,
    ERROR
}
```

---

## 12.5 BuildReportData

```csharp
public class BuildReportData
{
    public int SchemaVersion { get; set; }
    public string ProjectName { get; set; }
    public string UnityVersion { get; set; }
    public string AssetSizeDefinition { get; set; }

    public BuildInfo Build { get; set; }
    public List<AssetBuildInfo> Assets { get; set; }
    public List<BuildMessage> Messages { get; set; }
}
```

JSON과 DTO는 위의 중첩 구조를 공통 계약으로 사용한다. 필드를 변경할 때는 `schemaVersion`을 함께 올리고 WPF에서 지원 여부를 검사한다.

---

# 13. Dashboard 기획

## 13.1 화면 목적

현재 등록된 Unity 프로젝트의 전체 상태와 최근 빌드 결과를 한 화면에서 확인한다.

---

## 13.2 화면 구성

```text
┌────────────────────────────────────────────┐
│ Unity Project Inspector                    │
├───────────────┬────────────────────────────┤
│               │                            │
│ Dashboard     │ PROJECT ISOLATION          │
│ Build         │ Unity 6000.3.20f1          │
│ Settings      │                            │
│               │ ┌────────┐ ┌────────┐      │
│               │ │Project │ │Assets  │      │
│               │ │4.82 GB │ │2.31 GB │      │
│               │ └────────┘ └────────┘      │
│               │                            │
│               │ Last Build                 │
│               │ Android                    │
│               │ Success                    │
│               │ 182 MB                     │
│               │                            │
│               │ Asset Usage                │
│               │ Texture     312 MB         │
│               │ Audio       184 MB         │
│               │ Mesh         97 MB         │
│               │ Other        49 MB         │
└───────────────┴────────────────────────────┘
```

---

# 14. Dashboard 표시 데이터

## Project Card

표시 정보:

- Project Name
- Unity Version
- Project Path

---

## Storage Card

표시 정보:

- Project Disk Size
- Project Source Size
- Assets Size

예:

```text
Project Disk
4.82 GB

Project Source
2.46 GB

Assets
2.31 GB
```

---

## Last Build Card

표시 정보:

- Platform
- Result
- Report Generated At
- Build Output Size
- Build Time

예:

```text
Last Build

Android

Succeeded

182 MB
01:14

2026-09-17 13:20
```

---

## Build Message Card

```text
Warnings
13

Errors
0
```

---

## Asset Usage

Asset Type별 packed Asset 용량을 표시한다.

이 값은 BuildReport에 기록된 packed Asset을 Type별로 합산한 분석 지표다. APK, AAB 또는 실행 폴더의 전체 출력 크기 구성비와 정확히 일치한다고 표현하지 않는다.

초기 지원 Type:

- Texture
- Audio
- Mesh
- Animation
- Shader
- Scene
- Other

예:

```text
Texture      312 MB
Audio        184 MB
Mesh          97 MB
Animation     42 MB
Other         49 MB
```

Version 1에서는 차트 라이브러리를 반드시 사용하지 않아도 된다.

ProgressBar 형태로 구현해도 충분하다.

---

# 15. Build Analyzer 기획

## 15.1 화면 목적

빌드 결과에 포함된 Asset 및 로그를 상세 분석한다.

---

## 15.2 화면 구성

```text
┌──────────────────────────────────────────────────────────┐
│ Build Analyzer                                           │
├──────────────────────────────────────────────────────────┤
│ Platform : Android                                       │
│ Build Output Size : 182 MB                               │
│ Build Time : 01:14                                       │
│ Warning : 13                                             │
│ Error : 0                                                │
├──────────────────────────────────────────────────────────┤
│ Search [____________________] Type [All ▼]               │
├──────────────────────────────────────────────────────────┤
│ Name                 Type       Packed Size  Path         │
│ -------------------------------------------------------- │
│ LobbyBGM.wav         Audio      48 MB      Assets/...     │
│ Stage01.png          Texture    32 MB      Assets/...     │
│ Character.fbx        Model      27 MB      Assets/...     │
│ ResultBGM.wav        Audio      24 MB      Assets/...     │
├──────────────────────────────────────────────────────────┤
│ Build Messages                                           │
│                                                         │
│ Warning Shader variant stripped                         │
│ Warning Texture format converted                        │
└──────────────────────────────────────────────────────────┘
```

---

# 16. Build Analyzer 기능 상세

## 16.1 Search

Asset Name 또는 Path 기반 검색.

검색 예:

```text
bgm
```

결과:

```text
LobbyBGM.wav
StageBGM.wav
ResultBGM.wav
```

---

## 16.2 Asset Type Filter

ComboBox 사용.

```text
All
Texture
Audio
Mesh
Animation
Shader
Scene
Other
```

---

## 16.3 Sorting

DataGrid Header 클릭으로 정렬.

지원 항목:

- Name
- Type
- Packed Size

기본 정렬:

```text
Packed Size DESC
```

가장 큰 Asset이 위에 표시된다.

---

# 17. Project Analyzer 기능

WPF 프로그램에서 Unity 프로젝트 경로를 입력받는다.

예:

```text
D:\Fork\Dots-Boxes
```

Unity 프로젝트 여부는 다음 폴더를 기준으로 확인한다.

```text
Assets
Packages
ProjectSettings
```

세 폴더가 존재하면 Unity Project로 판단한다.

---

# 18. Unity Version 탐색

다음 파일을 읽는다.

```text
ProjectSettings/ProjectVersion.txt
```

예:

```text
m_EditorVersion: 6000.3.20f1
m_EditorVersionWithRevision: 6000.3.20f1 (...)
```

WPF 프로그램에서 해당 파일을 파싱하여 Unity Version을 표시한다.

---

# 19. 프로젝트 용량 계산

계산 대상:

```text
Project Root (Disk Size)
Assets
```

다음 폴더는 Source Size 계산에서 제외한다.

```text
Library
Temp
Logs
obj
```

Dashboard에는 다음 값을 분리하여 표시한다.

```text
Project Source Size
Assets + Packages + ProjectSettings

Project Disk Size
Unity Project 전체 폴더
```

Version 1 MVP에서는 다음 기준을 사용한다.

```text
Project Disk Size
프로젝트 루트의 전체 파일 크기

Project Source Size
Assets + Packages + ProjectSettings

Assets Size
Assets 폴더 전체 크기
```

`Library`, `Temp`, `Logs`, `obj`는 Project Disk Size에는 포함하고 Project Source Size에는 포함하지 않는다. 디렉터리 순회에서는 재분석 지점(Reparse Point)을 따라가지 않는다. 접근할 수 없는 파일은 건너뛰되, 누락 파일 수와 부분 계산 상태를 UI에 표시한다. 계산은 UI 스레드 밖에서 실행하고 취소할 수 있게 한다.

---

# 20. BuildReport Export 방식

Unity Editor에서 Build가 종료되면 다음 Interface를 사용한다.

```csharp
IPostprocessBuildWithReport
```

구조 예:

```csharp
public class BuildPostProcessor : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        ExportBuildReport(report);
    }

    private void ExportBuildReport(BuildReport report)
    {
    }
}
```

Unity가 빌드 종료 후 자동으로 JSON을 생성하도록 한다.

주의:

- `OnPostprocessBuild`는 빌드가 이른 단계에 실패하거나 취소되면 호출되지 않을 수 있다.
- JSON은 임시 파일에 먼저 쓴 뒤 최종 파일명으로 교체하여 WPF가 작성 중인 파일을 읽지 않도록 한다.
- `schemaVersion`을 기록하여 향후 JSON 구조 변경을 구분한다.

---

# 21. Build Report 저장 위치

추천:

```text
<ProjectRoot>/.unityprojectinspector/
```

구조:

```text
.unityprojectinspector
│
├─ build_report.json
└─ builds
   ├─ 2026-09-17_132000.json
   └─ 2026-09-18_101500.json
```

Version 1에서는 다음 파일만 사용해도 된다.

```text
build_report.json
```

`.unityprojectinspector/`는 생성 데이터이므로 Unity 프로젝트의 `.gitignore`에 추가하도록 설치 안내에 명시한다.

Version 1.1부터 Build History를 지원한다.

---

# 22. Application Settings

WPF 프로그램 자체 설정은 다음 위치에 저장한다.

```text
%AppData%/UnityProjectInspector/
```

예:

```text
settings.json
```

내용:

```json
{
  "lastProjectPath": "D:/Fork/Dots-Boxes"
}
```

---

# 23. 주요 Service 역할

## ProjectAnalyzerService

역할:

- Unity 프로젝트 검증
- 프로젝트명 탐색
- Unity Version 탐색
- Disk / Source / Assets 용량 계산

예:

```csharp
public interface IProjectAnalyzerService
{
    Task<ProjectInfo> AnalyzeProject_async(string projectPath);
}
```

---

## BuildReportService

역할:

- JSON 파일 탐색
- JSON Deserialize
- Build 정보 반환

```csharp
public interface IBuildReportService
{
    Task<BuildReportData> LoadBuildReport_async(string projectPath);
}
```

---

## SettingsService

역할:

- 최근 프로젝트 경로 저장
- 설정 불러오기

---

# 24. ViewModel 역할

## MainViewModel

책임:

- 현재 페이지 관리
- 프로젝트 선택 상태 관리

---

## DashboardViewModel

책임:

- 프로젝트 데이터 로드
- Build Summary 생성
- Asset Type별 통계 계산

---

## BuildAnalyzerViewModel

책임:

- Asset 목록 표시
- Search
- Filter
- Sorting
- Warning / Error 표시

---

# 25. 예외 처리

## Unity 프로젝트가 아닌 폴더 선택

표시:

```text
선택한 폴더는 Unity 프로젝트가 아닙니다.
```

검증 조건:

```text
Assets
Packages
ProjectSettings
```

---

## Build Report 없음

Dashboard:

```text
No Build Data
```

Build Analyzer:

```text
Build Report가 존재하지 않습니다.

Unity에서 Build 후 다시 확인하십시오.
```

마지막 보고서가 존재하는 경우에는 `reportGeneratedAtUtc`를 표시한다. 현재 실행한 빌드가 콜백 이전에 중단되었을 가능성이 있으므로 단순히 "최근 빌드"라고 단정하지 않는다.

---

## JSON 파싱 실패

표시:

```text
Build Report를 읽을 수 없습니다.
```

Log 기록:

```text
JSON Deserialize Exception
```

---

## 프로젝트 접근 권한 실패

```text
프로젝트 폴더에 접근할 수 없습니다.
```

---

# 26. UI 디자인 방향

전체적으로 개발자 툴 성격의 UI를 사용한다.

추천:

- Dark Theme
- Sidebar Navigation
- Card 기반 Dashboard
- DataGrid 기반 분석
- Accent Color 최소 사용
- 숫자 가독성 우선

디자인 참고 방향:

```text
Visual Studio
JetBrains Rider
GitKraken
Unity Hub
```

게임 UI처럼 디자인하지 않는다.

---

# 27. 주요 UX

## 첫 실행

```text
Unity Project Inspector

[ Select Unity Project ]
```

프로젝트 선택 후:

```text
Analyzing Project...
```

완료 후 Dashboard 이동.

---

## 재실행

마지막으로 사용한 프로젝트를 자동으로 불러온다.

```text
Last Project

Dots Arena

D:\Fork\Dots-Boxes
```

---

# 28. 개발 우선순위

## Phase 1

### BuildReport 기술 검증

- 작은 Unity 샘플 프로젝트에서 실제 Build 1회 실행
- `BuildSummary`, `PackedAssets`, `BuildStep.messages` 수집 가능 여부 확인
- 같은 `sourceAssetPath`가 여러 packed 항목으로 나오는 경우의 합산 규칙 확인
- 성공 빌드, 실패 빌드, 취소 빌드에서 콜백 호출 여부 기록
- Version 1 JSON 스키마 확정

---

## Phase 2

### WPF 기본 구조와 JSON 표시

- 프로젝트 생성
- MVVM 구성
- MainWindow
- JSON Deserialize
- 샘플 Asset DataGrid
- Loading, Empty, Error 상태

---

## Phase 3

### Unity Project 분석

- Folder Picker
- Unity Project 검증
- Project Name
- Unity Version
- Project Disk / Source Size
- Assets Size

---

## Phase 4

### Unity Build Exporter

- BuildPostProcessor
- BuildReport 읽기
- JSON Export

---

## Phase 5

### Build Analyzer

- JSON Deserialize
- Asset DataGrid
- Search
- Filter
- Sorting

---

## Phase 6

### Dashboard 연동

- Last Build
- Build Output Size
- Build Time
- Warning
- Error
- Asset Type 통계

---

## Phase 7

### Polish

- Loading UI
- Error Handling
- Empty State
- Settings
- Dark Theme
- README

---

## Phase 8

### UI 완성

- Sidebar Navigation
- Dashboard 카드 구성
- Build Analyzer 화면 구성
- 키보드 탐색과 긴 경로 표시 확인

---

# 29. MVP 완료 조건

다음 조건을 만족하면 Version 1.0 완료로 판단한다.

### 프로젝트

- Unity 프로젝트 폴더를 선택할 수 있다.
- Unity 프로젝트 여부를 검증할 수 있다.
- Unity Version을 확인할 수 있다.
- Project Disk Size를 확인할 수 있다.
- Project Source Size를 확인할 수 있다.
- Assets Size를 확인할 수 있다.

### Build

- Unity Build 종료 시 JSON이 생성된다.
- WPF에서 JSON을 읽을 수 있다.
- Build Result를 표시할 수 있다.
- Build Output Size를 표시할 수 있다.
- Build Time을 표시할 수 있다.
- Report 생성 시각을 표시할 수 있다.

### Asset

- Build 포함 Asset 목록을 볼 수 있다.
- packed Asset Size를 볼 수 있다.
- Size 기준 정렬할 수 있다.
- Asset Type 필터를 사용할 수 있다.
- 검색할 수 있다.

### Log

- Warning을 확인할 수 있다.
- Error를 확인할 수 있다.

### 데이터 정확성

- JSON에 `schemaVersion`이 기록된다.
- WPF가 지원하지 않는 schemaVersion을 명확한 오류로 표시한다.
- Asset 용량과 전체 Build Output 용량의 의미가 UI에서 구분된다.
- 콜백 이전에 중단된 빌드를 새 보고서로 오인하지 않는다.

---

# 30. 향후 확장 계획

## Version 1.1 - Build History

```text
Build History

2026-09-17
182 MB

2026-09-18
191 MB
+9 MB
```

기능:

- Build 기록 저장
- Build 크기 비교
- Build Time 비교
- Warning 변화 비교

---

## Version 1.2 - Build Diff

이전 Build와 현재 Build의 Asset 차이를 분석한다.

예:

```text
Build Output Size

Previous
182 MB

Current
213 MB

+31 MB
```

Asset 변화:

```text
LobbyBGM.wav

Previous
12 MB

Current
48 MB

+36 MB
```

---

## Version 2.0 - Asset Analyzer

추가 기능:

- 전체 Asset 탐색
- Asset Type 통계
- 대용량 Asset 탐색
- 동일 이름 Asset 탐색

---

## Version 2.1 - Dependency Analyzer

```text
Player.prefab

References

PlayerMaterial.mat
└ Player_BaseColor.png
```

기능:

- Asset Dependency
- Referenced By
- Dependency Graph

---

## Version 2.2 - Project Diagnostics

추가 기능:

- Missing Reference
- Missing Script
- Missing Meta
- Duplicate Asset
- Large Texture
- Large Audio
- Unused Asset 후보

---

# 31. 포트폴리오 설명 포인트

이 프로젝트에서 보여줄 수 있는 기술은 다음과 같다.

## C#

- File IO
- JSON Serialization
- Async Processing
- LINQ
- Collection
- Exception Handling

## WPF

- XAML
- MVVM
- Binding
- ICommand
- DataGrid
- CollectionView
- Converter
- ResourceDictionary

## Unity

- Unity Editor Extension
- BuildPipeline
- BuildReport
- AssetDatabase
- Editor Callback

## Architecture

- Desktop App + Unity Plugin 분리
- DTO 기반 데이터 통신
- Service Layer
- MVVM

---

# 32. Git Repository 구성 제안

```text
UnityProjectInspector
│
├─ src
│   └─ UnityProjectInspector
│
├─ unity-package
│   └─ com.hmlee.unity-project-inspector
│      ├─ package.json
│      └─ Editor
│         ├─ BuildReportExporter.cs
│         ├─ BuildPostProcessor.cs
│         └─ Models
│            └─ BuildReportDto.cs
│
├─ docs
│   ├─ Planning.md
│   ├─ Architecture.md
│   └─ Screenshots
│
├─ samples
│   └─ build_report.json
│
├─ README.md
└─ LICENSE
```

---

# 33. README 핵심 문구 예시

```text
Unity Project Inspector is a lightweight desktop tool
for inspecting Unity project information and build reports.

Features

- Unity Project Dashboard
- Build Output Size Analysis
- Packed Asset Size Analysis
- Build Warning / Error Viewer
- Build Asset Filtering
```

---

# 34. 최종 개발 방향

초기 버전에서는 기능을 과도하게 확장하지 않는다.

핵심 개발 흐름은 다음과 같다.

```text
Project Select
      ↓
Project Analyze
      ↓
Dashboard
      ↓
Unity Build
      ↓
BuildReport JSON
      ↓
Build Analyzer
```

Version 1의 핵심 가치는 다음 두 가지다.

1. **Unity 프로젝트 상태를 빠르게 확인한다.**
2. **빌드 용량이 어디에서 발생하는지 빠르게 확인한다.**

이 두 기능이 안정적으로 동작한 이후 Build History, Build Diff, Asset Dependency 등의 기능을 단계적으로 추가한다.
