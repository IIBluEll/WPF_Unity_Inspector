# Unity Project Inspector 검증 기록 (2026-09-18)

## 범위

- Unity Editor: 6000.3.20f1, Windows Standalone 빌드. 원본 게임 프로젝트는 변경하지 않고 `%TEMP%\UPI_Verification_20260918`에 만든 임시 프로젝트에 로컬 `com.hm.unity-project-inspector` 패키지를 연결했습니다.
- WPF: `dotnet build .\WPF_UnityInspector.sln --no-restore` 성공, 경고 0개·오류 0개. 빌드한 DLL의 `BuildReportService`로 실제 Android JSON과 예외 입력을 로드했습니다.
- 별도로 기존 `D:\Fork\Dots-Boxes\.unityprojectinspector\build_report.json`과 실제 APK를 대조했습니다. 이 Android 보고서가 자동 내보내기에서 만들어졌는지는 확인하지 못했습니다.

## 확인된 결과

| 검사 | 결과 |
| --- | --- |
| 기존 Android JSON | Schema 2, 성공, Asset 2,945개, 중복·빈 경로 0개, Packed Size 합계 84,910,443 bytes |
| Android Artifact | JSON의 `artifactSizeBytes` 95,230,648 bytes와 현재 `hh.apk` 파일 크기 일치 |
| Android 메시지 | Warning 27개, Error 0개. `reportedErrorCount`는 1이므로 Unity 원본 집계와 표시 대상 집계가 다름 |
| WPF 로더 | 실제 JSON 로드 성공. Schema 99는 `NotSupportedException`, 잘못된 JSON은 `JsonException`, 파일 없음은 `FileNotFoundException` |
| 임시 Windows 성공 빌드 | `BuildPipeline.BuildPlayer`가 `Succeeded`를 반환하고 자동 JSON 생성 확인 |
| 임시 Windows 조기 실패 빌드 | `IPreprocessBuildWithReport`에서 `BuildFailedException`을 발생시켜 `Failed` 확인. `OnPostprocessBuild`가 실행되지 않았고 이전 JSON의 SHA-256은 변경되지 않음 |

### 수정 전 자동 내보내기 시점 결함

같은 Windows 성공 빌드에서 `OnPostprocessBuild`가 생성한 JSON을 보관한 뒤, 빌드 완료 후 Unity 메뉴 **Tools > Unity Project Inspector > Export Latest Build Report**로 다시 내보내 비교했습니다.

| 필드 | 자동 JSON (`OnPostprocessBuild`) | 빌드 완료 후 수동 내보내기 |
| --- | ---: | ---: |
| `build.result` | `Unknown` | `Succeeded` |
| `buildTimeSeconds` | `0` | `1.7661126` |
| `reportedOutputSizeBytes` | `0` | `89,219,210` |
| `artifactSizeBytes` | `667,136` | `667,136` |
| Asset 수 | 9 | 9 |

이 Windows 빌드의 `outputPath`는 `Verification.exe`였으므로 `artifactSizeBytes` 667,136 bytes는 EXE 파일만 측정한 값입니다. 함께 생성된 Data 폴더의 크기는 이 값에 포함되지 않습니다.

자동 JSON이 기록된 뒤 Unity 로그에는 `Build Finished, Result: Success.`가 나오고, 메서드 반환 시 `report.summary.result`는 `Succeeded`였습니다. **추론입니다:** 수정 전 `IPostprocessBuildWithReport` 콜백에서 BuildSummary의 일부 필드를 최종 확정 전에 읽었습니다. 자동 보고서의 결과·시간·Unity 보고 출력 크기가 실제 빌드와 달라, 수정 전 상태로는 기획서의 해당 MVP 완료 조건을 충족했다고 볼 수 없었습니다.

### 수정 전 UTC 변환 결함

테스트 빌드의 원본 `report.summary.buildStartedAt`은 `2026-09-18T05:19:03.1417109`, `DateTime.Kind = Unspecified`였습니다. 같은 시점의 `DateTime.UtcNow`는 `2026-09-18T05:19:05.1723967Z`였습니다. JSON의 `buildStartedAtUtc`는 `2026-09-17T20:19:03.1417109Z`로 원본 시각보다 9시간 앞당겨졌습니다. exporter의 `FormatUtcDateTime`이 `ToUniversalTime()`을 호출해 `Unspecified` 값을 로컬 시각으로 해석한 결과입니다. `buildEndedAtUtc`도 같은 함수를 사용하므로 함께 점검해야 합니다. `reportGeneratedAtUtc`는 `DateTime.UtcNow`에서 생성합니다.

## 확인하지 못한 항목

- 사용자가 진행 중인 빌드를 취소했을 때 콜백 호출 여부. 배치 모드의 조기 실패는 취소와 동일한 검증이 아닙니다.
- Unity 2022.3에서의 패키지 설치·빌드·JSON 로드.
- 다른 플랫폼에서 자동 내보내기 시점과 `DateTime.Kind`가 동일한지 여부.

## 수정 후 재검증 (패키지 v0.2.2)

`BuildPostProcessor`가 빌드 종료 후 Editor 업데이트에서 최종 BuildReport를 내보내도록 수정했습니다. 배치 모드가 바로 종료될 때는 Editor 종료 콜백에서 내보냅니다. 새 빌드의 GUID가 일치하고 `BuildResult.Unknown`이 아닌 보고서만 기록합니다. `DateTime.Kind = Unspecified`인 BuildSummary 시각은 이번 Unity 6000.3.20f1 테스트에서 관찰된 UTC 값으로 직렬화했습니다.

| 검사 | 자동 JSON 결과 |
| --- | --- |
| Editor가 열린 상태의 Windows 성공 빌드 | `Succeeded`, `buildTimeSeconds = 2.018372`, `reportedOutputSizeBytes = 89,219,210` |
| 위 빌드의 시작 시각 | 원본 `2026-09-18T05:41:14.7334512`와 JSON `2026-09-18T05:41:14.7334512Z` 일치 |
| 배치 모드 종료 시 Windows 성공 빌드 | `Succeeded`, `buildTimeSeconds = 2.1599117`, `reportedOutputSizeBytes = 89,219,210` |
| 위 빌드의 시작 시각 | 원본 `2026-09-18T05:37:10.3462644`와 JSON `2026-09-18T05:37:10.3462644Z` 일치 |
| 빌드 전 단계 실패 | `Failed` 확인, 이전 자동 JSON의 SHA-256 `00CF7AE252F6CFE8CEC68928E1EEC32F2A03A68777807F876A6D437C922A219E` 유지 |
| WPF `BuildReportService` 로드 | 수정 후 자동 JSON을 `Succeeded`, `2.018372초`로 읽음 |

위 Windows 재검증 시점에는 Android 수정 후 빌드, Unity 2022.3, 실제 취소 빌드를 확인하지 못했습니다. 테스트한 Unity 프로젝트는 로컬 파일 패키지를 사용했습니다. `D:\Fork\Dots-Boxes`는 Git URL 패키지를 사용하므로 로컬 소스 수정만으로 그 프로젝트의 `PackageCache`가 갱신되지는 않습니다.

## 실제 Android 빌드의 PackedAssetInfo 집계 검증

`D:\Fork\Dots-Boxes`의 이번 Android 빌드에서 Unity 원본 `Library/LastBuild.buildreport`를 별도 임시 Unity 프로젝트로 복사해 `BuildReport.GetLatestReport()`로 읽었습니다. 원본과 복사본의 SHA-256은 `A4B276648D796FE4BB3A7915D50AB509B6B7DA2F27726F101BEA6C23F13B8F32`로 일치합니다. 원본 보고서와 JSON의 Build GUID는 모두 `a38a5d57cd33489fa70948885e315b61`입니다.

원본 `PackedAssetInfo`의 경로를 exporter와 같은 방식으로 정규화하고, 대소문자를 구분하지 않는 경로 키로 `packedSize`를 합산했습니다. 이후 JSON의 모든 `assets[].path`와 `packedSizeBytes`를 경로별로 대조했습니다.

| 검사 | 결과 |
| --- | ---: |
| 원본 PackedAssetInfo 항목 | 5,105개 |
| 원본의 고유 경로 | 2,945개 |
| 원본에서 2회 이상 나타난 경로 | 155개 |
| 한 경로의 최대 등장 횟수 | 603회 |
| JSON 자산 행 | 2,945개 |
| 원본·JSON Packed Size 총합 | 양쪽 모두 84,910,443 bytes |
| 경로 누락 또는 경로별 용량 불일치 | 0개 |

따라서 이 빌드에서는 동일 경로의 원본 항목이 여러 번 등장해도 exporter의 합산 결과가 JSON에 정확히 반영되었습니다. 이 검증은 해당 Android 빌드 한 건에 대한 결과입니다. 제품 코드 변경은 없습니다.