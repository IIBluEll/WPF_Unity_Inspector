# WPF_Unity_Inspector
C# WPF로 만든 유니티 개발 툴 프로그램

## 준비 및 실행

- WPF 앱은 .NET 9 SDK로 빌드합니다. 빌드된 앱을 실행하려면 .NET 9 Desktop Runtime이 필요합니다.
- Unity 패키지 `com.hm.unity-project-inspector`는 Unity Package Manager의 **Add package from disk**에서 패키지의 `package.json`을 선택해 설치할 수 있습니다. 패키지는 Unity 2022.3 이상을 최소 버전으로 선언하지만, 실제 빌드 검증은 Unity 6000.3.20f1에서만 했습니다.

```powershell
dotnet build .\WPF_UnityInspector.sln
dotnet run --project .\WPF_UnityInspector.csproj
```

Unity 프로젝트를 빌드하면 패키지가 `<UnityProject>/.unityprojectinspector/build_report.json`을 생성합니다. WPF 앱에서 **Select Unity Project**를 눌러 `Assets`, `Packages`, `ProjectSettings`가 있는 프로젝트 루트를 선택하세요. 마지막으로 선택한 경로는 `%LocalAppData%\HM\UnityProjectInspector\last-project.txt`에 저장되고 다음 실행 시 다시 읽습니다. 앱을 열어둔 상태에서 새 빌드를 했다면 프로젝트를 다시 선택하거나 앱을 재실행해야 새 JSON을 읽습니다.

## 표시값의 의미

- Project는 선택한 프로젝트 루트 폴더명, Unity Version은 `ProjectSettings/ProjectVersion.txt`의 `m_EditorVersion`입니다. JSON의 `projectName`은 Unity `Application.productName`이므로 Project 표시값과 다를 수 있습니다.
- Project Disk는 프로젝트 루트의 파일 크기 합계입니다. Project Source는 루트의 `Assets`, `Packages`, `ProjectSettings` 합계이고, Assets는 `Assets` 합계입니다. 재분석 지점은 따라가지 않으며 읽을 수 없는 항목이 있으면 부분 계산 상태를 표시합니다.
- Artifact Size는 Unity `outputPath`가 가리키는 파일 또는 디렉터리를 측정한 `artifactSizeBytes`입니다. Windows Standalone처럼 `outputPath`가 EXE 파일이면 함께 생성된 Data 폴더는 포함하지 않습니다. Unity가 보고한 `reportedOutputSizeBytes`, Asset별 `packedSizeBytes` 합계와 산출 기준이 다릅니다.
- Asset 목록의 Packed Size는 Unity `PackedAssetInfo.packedSize`를 같은 경로별로 합산한 값입니다. Warning/Error 숫자는 JSON의 표시 대상 메시지 개수이며 Unity 원본 `reportedWarningCount`/`reportedErrorCount`와 다를 수 있습니다.
- Report Generated는 **마지막으로 JSON을 내보낸 시각**입니다. 가장 최근에 시도한 빌드의 시각을 보장하지 않습니다.

## 현재 검증 상태

2026-09-18 검증에서 Unity 6000.3.20f1의 자동 내보내기가 빌드 결과 확정 전에 실행되는 문제를 발견하고 Unity 패키지 v0.2.2에서 수정했습니다. Windows 성공 빌드를 Editor가 열린 상태와 배치 모드에서 재실행해 자동 JSON의 `build.result = Succeeded`, 정상 Build Time·Unity 보고 출력 크기·UTC 시작/종료 시각을 확인했습니다. 기존 Android 보고서는 수정 전 데이터이며, Android에서의 수정 후 빌드는 아직 검증하지 않았습니다.

수정 후에도 빌드 전 단계에서 실패한 테스트에서는 기존 JSON이 갱신되지 않았습니다. 취소 빌드와 Unity 2022.3은 아직 실제 검증하지 않았습니다. 자세한 수치와 재현 범위는 [검증 기록](docs/VERIFICATION_2026-09-18.md)에 있습니다. 제품 범위와 계획은 [기획서](docs/Planning.md)를 참고하세요.
