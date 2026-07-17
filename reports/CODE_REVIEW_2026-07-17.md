# ExcelBinder 전체 코드 리뷰 (2026-07-17)

4개 영역(코어 파이프라인 / 앱·ViewModel 계층 / UX / 개발 위생) 병렬 리뷰 결과 종합.
"검증됨" 표시는 리뷰어가 코드를 직접 읽어 재확인한 항목.

## 요약

코드 기초 체력은 양호: 빈 catch 0건, 리소스 누수 없음, Binary writer/reader 포맷 완전 대칭, 컴파일러 경고 0건.
핵심 문제: ①Export/CodeGen 중 로딩 오버레이 미동작, ②settings.json 손상 시 앱 시작 불가,
③List 타입 export가 정상 입력에서 크래시 가능, ④Scriban 6.5.2 취약점 12건(Critical 1),
⑤테스트 프로젝트가 문서상으로만 존재.

## 사용자 결정 (2026-07-17)

- **수정**: Q1~Q5 / D1~D4 / B1~B7 / E1~E2 (총 18건)
- **보류(유지)**: Q6~Q10 / D7~D8 / B8~B19 / E3~E5
- D5·D6은 보류이나 D3(MainViewModel 분리) 수행 시 자연 해소되어 함께 처리
- 추가 요청 "전체 구조 점검" 수행 → 하단 [추가 구조 점검] 섹션 참조 (S1~S4 일부 조치 포함)
- Q3 언어 통일 방향: **한국어**

---

## 1순위 — 품질

### 사용자 측면

| # | 심각도 | 항목 | 위치 | 상태 |
|---|--------|------|------|------|
| Q1 | Major | 로딩 오버레이가 Export/CodeGen에서 안 뜸 — 오버레이는 `MainViewModel.IsBusy` 바인딩인데 실행 VM은 자신의 `IsBusy`만 설정. "지금 업데이트 확인"에서만 동작 | `MainWindow.xaml:135` ↔ `StaticDataExecutionViewModel.cs:45` 등 | 검증됨 · **수정 완료** |
| Q2 | Major | 에러가 영어 기술 메시지로 로그 창에만 전달. 치명 실패도 대화상자 없음 | `ExportService.cs:67,202`, `ExecutionViewModelBase.cs:109` | **수정 완료** |
| Q3 | Major | UI 한/영 혼용 광범위 (Dashboard 영어 버튼 vs 한국어 컨텍스트 메뉴, Settings 혼재) | Views 전반 | **수정 완료 (한국어 통일)** |
| Q4 | Major | "시작하기" 가이드의 "+" 버튼 안내가 실제와 다름 — "+"는 그룹 생성, Feature 생성은 "+ Create New Feature" | `Resources/Guides/01_GettingStarted.xaml:25` | **수정 완료** |
| Q5 | Minor | Schema Editor 가이드가 "준비 중입니다" 플레이스홀더 노출 | `Resources/Guides/03_SchemaEditor.xaml:11` | **수정 완료** |
| Q6 | Minor | 성공 시에도 매번 모달 로그 창이 뜸 — 성공은 상태바/토스트, 실패만 로그 창 권장 | `StaticDataProcessor.cs:80,126` | 보류 |
| Q7 | Minor | Feature 삭제 기능이 UI에 없음 (JSON 파일 직접 삭제 필요) | Dashboard | 보류 |
| Q8 | Minor | 빈 상태 안내 부재 (Feature 0개 Dashboard, 소스 파일 0개 목록, 경로 없을 때 무음 빈 화면) | `ExecutionViewModelBase.cs:71` 등 | 보류 |
| Q9 | Minor | 창 크기·위치 미기억, FeatureBuilder Cancel 시 미저장 변경 확인 없음 | `AppSettings.cs`, `FeatureBuilderView.xaml:262` | 보류 |
| Q10 | Minor | 버튼 라벨 ↔ 문서 용어 불일치 ("Execute Export"/"Generate Code" vs 가이드 "Export"/"CodeGen"), 스키마 상태 점 색 의미 라벨 없음 | `SourceFileListControl.xaml:51` | 보류 |

### 개발 측면

| # | 심각도 | 항목 | 위치 | 상태 |
|---|--------|------|------|------|
| D1 | Major | 테스트 프로젝트가 빈 껍데기 — .cs/.csproj 없음, 솔루션 미등록. `dotnet test` 실행 불가. TEST_GUIDE.md는 존재하지 않는 `CSVProcessor`/`Moq` 예시 사용 | `ExcelBinder.Tests/`, `.guides/TEST_GUIDE.md` | 검증됨 · **수정 완료** |
| D2 | Major | CLAUDE.md 아키텍처 서술 불일치 — `BinderData`/`ColumnInfo`/`RowData`/`ExcelLoaderService`/`CodeGenService` 전부 미존재 (실제: `ExcelService`, `CodeGeneratorService`). 매 세션 로드되므로 정정 우선 | `CLAUDE.md` | 검증됨 · **수정 완료** |
| D3 | Major | MainViewModel god object (31KB) — 업데이트/그룹 CRUD/네비게이션/설정 영속화 전담. 업데이트 관련 ~330줄부터 `UpdateViewModel`로 추출 권장 | `ViewModels/MainViewModel.cs` | **수정 완료** |
| D4 | Minor | 그룹 저장 책임 산재 — 서비스와 VM이 각자 컬렉션 수정+저장 | `MainViewModel.cs:359-473` vs `FeatureGroupService.cs:78-142` | **수정 완료** |
| D5 | Minor | category→ExecutionViewModel 매핑 switch 중복 | `App.xaml.cs:91-99` vs `MainViewModel.cs:499-507` | 해소 완료 (D3와 함께) |
| D6 | Minor | 릴리즈 노트 조회+포맷 로직 중복 (D3 추출 시 자연 해소) | `MainViewModel.cs:648-651` vs `:771-773` | 해소 완료 (D3와 함께) |
| D7 | Minor | PROJECT_SUMMARY.md의 `ExportService # 데이터 추출` 설명 오류, 실존하지 않는 `WORKFLOW_GUIDE.md` 참조 | `PROJECT_SUMMARY.md` | 보류 |
| D8 | Minor | .guides의 COMMIT_RULES/BUILD_GUIDE/CODE_STYLE 예시가 가공의 `CSVProcessor` 기반 | `.guides/*.md` | 보류 |

**테스트 시작점 추천**: ①`TypeParser`(순수 로직, 비용 최저) ②`ExcelService.GetFilteredData`의 `#` 필터링
③`ExternalTestData/`에 커밋된 골든 산출물(.bytes/.json) 기준 Export 회귀 테스트.

---

## 2순위 — 버그

| # | 심각도 | 항목 | 위치 | 상태 |
|---|--------|------|------|------|
| B1 | Critical | settings.json 손상 시 앱 시작 불가 — `LoadSettings`에 try/catch 없음 + 전역 예외 핸들러 전무. `SaveSettings`는 `File.WriteAllText` 직접 덮어쓰기라 저장 중 종료 시 손상 가능. **로드 방어 + 임시파일→`File.Replace` 원자적 저장을 한 세트로 수정** (`FeatureGroupService.SaveGroups`도 동일) | `FeatureService.cs:23-31,36-40`, `FeatureGroupService.cs:58-76` | 검증됨 · **수정 완료** |
| B2 | Critical | List 필드 export `row[idx]` 인덱스 초과 크래시 — NPOI는 후행 빈 셀 미저장으로 행 배열 길이가 제각각인데 List 경로만 경계 미검사. List 마지막 셀을 비운 정당한 데이터에서 export 전체 실패. 스칼라 경로의 `idx < row.Length ? row[idx] : ""` 가드를 List 루프에도 적용 | `ExportService.cs:81,164` | 검증됨 · **수정 완료** |
| B3 | Critical(추정) | NPOI 수식 셀 `ToString()`이 계산 결과 아닌 수식 문자열 반환 가능(무음 왜곡) + 숫자 셀 읽기(현재 culture) ↔ 파싱(Invariant) 불일치로 콤마 소수점 로케일에서 전면 실패. `cell.CellType` 기반 타입 읽기 리팩터 1건으로 동시 해소 (NPOI 2.7.5 실측 후 진행) | `ExcelService.cs:62,102` ↔ `ExportService.cs:189-196` | **수정 완료** |
| B4 | Major | Constants/Enum 코드 생성 시 문자열 이스케이프 누락 — 값에 `"`/`\`/개행 포함 시 컴파일 불가 또는 왜곡(`C:\temp`의 `\t`→탭). 수동 escape helper 권장 (Microsoft.CodeAnalysis는 과잉) | `ConstantsProcessor.cs:132-140`, `EnumProcessor.cs:188` | **수정 완료** |
| B5 | Major | CLI 모드 exit code 미설정 — 모든 실패 경로(feature 미발견, 인자 오류, processor 예외, category 오타 무음 스킵)가 0으로 종료 → CI가 실패를 성공으로 판정. `Environment.ExitCode=1` + try/catch 필요 | `App.xaml.cs:32-128` | **수정 완료** |
| B6 | Major | export 예외 시 손상된 .bytes가 기존 정상 산출물을 덮은 채 잔존 — 임시 파일→원자적 교체 권장 | `ExportService.cs:31-49` | **수정 완료** |
| B7 | Major | List 컬럼명이 헤더에 없으면 무음 빈 리스트 (스칼라는 예외 — 비일관). 스키마 오타가 무증상 데이터 손실로 이어짐 | `ExportService.cs:75-78` | **수정 완료 (경고 로그)** |
| B8 | Major | LogicParser Name/Formula 컬럼 경계 검사 누락 + 빈 행 skip 없어 invalid 코드 생성 가능 | `LogicParserService.cs:30,33` | 보류 |
| B9 | Major | SchemaEditor 저장 실패를 성공으로 통지 — catch에서도 `OnComplete?.Invoke(true)` | `SchemaEditorViewModel.cs:60-67` | 보류 |
| B10 | Minor | `ParseInt` float 근사 검사가 `int.MaxValue` 부근 정당한 값 오탐 (상위 ~128개 int) | `ExportService.cs:206-211` | 보류 |
| B11 | Minor | CLI 실행이 사용자 GUI 설정(LastFeatureId)을 덮어씀 | `MainViewModel.cs:154-162`, `App.xaml.cs:88` | 보류 |
| B12 | Minor | CLI 헤드리스 모드에서도 GitHub 업데이트 확인 네트워크 호출 발화 | `MainViewModel.cs:270-273` | 보류 |
| B13 | Minor | CLI `--bind`가 인자마다 설정 재저장 (N회 파일 쓰기) | `App.xaml.cs:55-59` | 보류 |
| B14 | Minor | RefreshFeatures CLI 경로 3회 중복 호출 | `App.xaml.cs:84`, `MainViewModel.cs:258,288` | 보류 |
| B15 | Minor | `EnumType` 없는 `enum` 토큰이 무음 string 격하 (포맷 대칭이라 크래시는 없음) | `CodeGeneratorService.cs:104-108`, `ExportService.cs:94-97` | 보류 |
| B16 | Minor | 코드 생성 시 C# 식별자 검증 부재 — 시트/컬럼명에 공백·특수문자·예약어 시 컴파일 실패 | `ConstantsProcessor.cs:113`, `EnumProcessor.cs:167` | 보류 |
| B17 | Minor | TemplateEngine 캐시 키 `(Length, GetHashCode)` 해시 충돌 이론적 위험 | `TemplateEngineService.cs:24` | 보류 |
| B18 | Minor | FeatureService static 캐시 — 삭제된 파일 엔트리 영구 잔존 (영향 미미) | `FeatureService.cs:18,45-71` | 보류 |
| B19 | Minor | 존재하지 않는 시트명 지정 시 무음 빈 결과 반환 | `ExcelService.cs:49-50,89-90` | 보류 |

---

## 3순위 — 기타

| # | 심각도 | 항목 | 위치 | 상태 |
|---|--------|------|------|------|
| E1 | High | Scriban 6.5.2 취약점 12건 (Critical 1: GHSA-5wr9-m6jw-xx44, High 8, Moderate 3) — 빌드 경고 NU1902/1903/1904로 실측 확인. 사용자 제공 .liquid 실행 경로라 노출면 실질적. **패치 버전 업그레이드 최우선** (골든 출력 테스트 먼저 확보 권장). NPOI/Newtonsoft은 양호 | `ExcelBinder.csproj` | **수정 완료** |
| E2 | Minor | README가 미존재 "AI Assistant" 기능 광고 (최근 커밋에서 기능 제거됨) | `README.md:30` | **수정 완료** |
| E3 | Minor | CHANGELOG가 2.0.0에서 멈춤 — 현재 v2.9.0, 2.1~2.9 내역 누락 | `ExcelBinder/CHANGELOG.md` | 보류 |
| E4 | Minor | 커밋된 잡파일 `WORK_HISTORY.json` 제거 검토 | 저장소 루트 | 보류 |
| E5 | 참고 | `#` 시트 무시 규칙은 EnumProcessor만 코어에서 강제 — StaticData/Logic은 UI 선택 목록에 의존 | `EnumProcessor.cs:93` | 보류 |

---

## 추가 구조 점검 (2026-07-17, 사용자 요청)

| # | 항목 | 위치 | 상태 |
|---|------|------|------|
| S1 | Services → ViewModels 역방향 의존 = 순환 의존. Processor 5종이 `IExecutionViewModel`을 파라미터로 받고 `vm.ShowLogs()`(UI 트리거) 콜백 — Processor 단위 테스트의 직접적 장애물 | `IFeatureProcessor.cs:19-20`, `StaticDataProcessor.cs:80,126` | **최소 조치 완료**: ShowLogs 호출 13곳을 호출측 VM으로 이동. 인터페이스 전면 재설계는 보류 |
| S2 | ViewModel이 View(GuideWindow)를 직접 생성 — 기존 `IDialogService` 패턴 우회 | `MainViewModel.cs:601-615` | **수정 완료** (D3와 함께, `IDialogService.ShowGuideWindow` 신설) |
| S3 | ExcelService 내부 3중 중복 — workbook open switch 3벌, 셀 읽기 루프 2벌. B3 수정 시 한쪽 경로만 고쳐지는 사고 유발 구조 | `ExcelService.cs:16-21,40-45,77-82,58-63,98-103` | **수정 완료** (B3와 함께 helper 통합) |
| S4 | Constants/EnumProcessor 코드젠 보일러플레이트 중복 (헤더 탐색, 네임스페이스 래핑 등) | `ConstantsProcessor.cs:75-129` ↔ `EnumProcessor.cs:102-205` | **부분 조치** (B4에서 escape/헤더 helper만 공유). 전면 template-method화는 과잉으로 보류 |
| S5 | AppServices static locator + MainViewModel 생성자 부작용(디스크 I/O+네트워크) → MainViewModel 단위 테스트 불가 | `AppServices.cs`, `MainViewModel.cs:226-273` | 보류 (테스트 대상은 서비스 계층이라 당장 실해 없음, 전면 DI는 과잉) |
| S6 | Views 코드비하인드는 전부 얇고 View 관심사만 — 문제 없음. `LogService.cs:25`의 `App.Current` 참조는 기술적 계층 위반이나 null-안전 | — | 조치 불요 |

## 잘 되어 있는 부분

- 빈 catch 블록 0건 (절대 규칙 준수), 대부분 예외 필터(`when`) 사용
- 워크북 리소스 누수 없음 (`using` + `finally` Dispose)
- Binary writer ↔ 생성 코드 reader 포맷 완전 대칭 (프리미티브/enum/List/필드 순서 전부 1:1 확인)
- UpdateCheckService 네트워크 예외·부분 파일 정리·취소 토큰 처리 견고
- LogService의 CLI 데드락 방지 BeginInvoke 마셜링 명확
- 그룹 삭제 확인 대화상자 + 부작용 안내 모범적, FeatureGroup lazy GC 정합성 유지
- `FileShare.ReadWrite`로 엑셀에서 열어둔 파일도 읽기 가능
- .gitignore/csproj 위생 양호 (obj/bin 오염 0건, 버전 3자 동기화, 경고 억제 없음)

## 실행 계획 (2026-07-17 전체 완료)

```
Phase 0. D2+E2+Q4+Q5 (문서)                              — 완료 (0922e16)
Phase 1. D1 (테스트 프로젝트+골든 4종, 54개 테스트)        — 완료 (2282c09)
Phase 2. B1+B6 원자적 저장 / B2+B7 List 가드 / B5 exit code — 완료 (631f7c0)
Phase 3. E1 Scriban 6.5.2→7.2.5 (취약점 0건)              — 완료 (32dcd2c)
Phase 4. B3 CellType 리팩터 + S3 통합 (골든 무변화)        — 완료 (528290f)
Phase 5. Q1 오버레이 (3ca3728) / Q2+Q3+B4 (a1e67a3)       — 완료
Phase 6. D4+S1 (b2a36fa) / D3+S2+D5+D6 (f4c7318)          — 완료
```

### 수정 중 확정된 동작 변경 (주의)
- **List 빈 셀 의미**: List 컬럼의 빈 셀(중간/후행)은 이제 건너뛴 가변 길이
  리스트로 처리 (기존: 후행 빈 셀은 크래시, 중간 빈 셀은 int면 크래시)
- **불리언 셀**: `TRUE`/`FALSE` → `true`/`false`로 읽힘 (Export는 무영향,
  Constants 생성은 컴파일 가능 리터럴로 개선)
- **날짜 셀**: 셀 서식·로케일 의존 문자열 → invariant `yyyy-MM-dd HH:mm:ss` 고정
- **수식 셀**: 수식 텍스트 → 캐시된 계산 값
- SkillData 골든 재생성 (스키마 Power float→int 변경 미반영 stale 골든)
