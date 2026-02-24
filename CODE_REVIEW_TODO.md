# Code Review TODO - ExcelBinder

> 작성일: 2026-02-24
> 리뷰 버전: v2.2.1
> 총 이슈: 24건 (HIGH 6 / MEDIUM 11 / LOW 7)
> AI Assistant 기능 제거로 H-01, H-02, L-07은 해당 없음 처리

---

## HIGH - 수정 필수

- [x] **H-01** ~~API 키 평문 저장 → DPAPI 암호화 적용~~ N/A (AI 기능 제거로 해당 없음)

- [x] **H-02** ~~`dynamic` 타입 사용 → 강타입 DTO 정의~~ N/A (AI 기능 제거로 해당 없음)

- [x] **H-03** `GetAwaiter().GetResult()` 데드락 위험 → `Task.Run` 래핑 ✅
  - `App.xaml.cs` CLI 실행 경로에 `Task.Run(async () => { ... })` 래핑 적용

- [x] **H-04** CLI 인자 범위 초과 위험 → 인덱스 범위 체크 추가 ✅
  - `App.xaml.cs`에서 `--feature`, `--bind` 인자에 `i + 1 >= args.Length` 체크 추가

- [x] **H-05** 숫자 파싱 CultureInfo 미지정 → `InvariantCulture` 적용 ✅
  - `ExportService.cs` 모든 `Parse` 호출에 `CultureInfo.InvariantCulture` 추가

- [x] **H-06** `RelayCommand<T>.Execute` null 캐스팅 → 타입/null 체크 추가 ✅
  - `CommonModels.cs`에서 `if (parameter is T typed) _execute(typed);` 패턴으로 변경

---

## MEDIUM - 수정 권장

- [x] **M-01** 예외 필터 논리 오류 → 명확한 예외 타입 사용 ✅
  - `catch (Exception ex) when (ex is FormatException or OverflowException)` 패턴으로 변경

- [x] **M-02** yield return + finally 리소스 관리 → List 반환 ✅
  - `ExcelService.ReadExcel` 반환 타입을 `List<string[]>`로 변경

- [x] **M-03** `_featureCache` 무한 성장 → 캐시 정리 로직 추가 ✅
  - 새 엔트리 추가 전 동일 파일 경로의 stale 키 제거

- [x] **M-04** 템플릿 캐시 키 최적화 → 해시 기반으로 변경 ✅
  - `(Length, HashCode)` 튜플 키로 변경하여 메모리 절약

- [x] **M-05** `AppServices` 정적 Service Locator → null 체크 추가 ✅
  - 미초기화 시 `InvalidOperationException` throw 패턴 적용

- [x] **M-06** 서비스 `new` 생성 남발 → 공유 인스턴스 필드로 전환 ✅
  - StaticData/Logic/Enum/Constants Processor에 readonly 필드 적용

- [x] **M-07** Processor 싱글턴 상태 공유 위험 → stateless 문서화 ✅
  - `FeatureProcessorFactory`에 stateless 요구사항 XML doc 추가

- [x] **M-08** Processor 매번 조회 → 생성자에서 캐싱 ✅
  - `ExecutionViewModelBase`에 `_processor` 필드 추가, 생성자에서 1회 조회

- [x] **M-09** `async void` 미처리 예외 → try-catch 추가 ✅
  - 6개 메서드에 catch 블록 추가 (Export/Generate 계열)

- [x] **M-10** `CanExecuteChanged` 미연결 → CommandManager 연동 ✅
  - `RelayCommand`, `RelayCommand<T>` 모두 `CommandManager.RequerySuggested` 연동

- [x] **M-11** 이벤트 구독 해제 누락 → Settings setter에서 관리 ✅
  - 명명된 핸들러로 전환, Settings 교체 시 이전 구독 해제

---

## LOW - 선택적 개선

- [x] **L-01** 빌드 경고 16건 해소 → 경고 0건 달성 ✅
  - CS8714/CS8621/CS8619: `ExportService.CreateHeaderMap` GroupBy에 null-forgiving 적용
  - CS8600: `EnumProcessor` FirstOrDefault 반환 타입 `string?`로 변경
  - CS8602: `EnumProcessor.GenerateEnum`, `ConstantsProcessor.ProcessSheet`에 null 가드 추가
  - CS8618: `FeatureBuilderViewModel.OnComplete` → `Action?`, `_key`/`_value` → `string.Empty` 초기화
  - CS8618: `SchemaEditorViewModel._selectedKey` → `string.Empty` 초기화

- [x] **L-02** 루트 디렉토리 .md 문서 정리 ✅
  - `AGENT_ROLES.md`, `DOCUMENTATION_STRUCTURE.md`, `END_TO_END_SCENARIOS.md`, `QUICK_REFERENCE.md` → `.guides/`로 이동

- [x] **L-03** `settings.json.example` 샘플 파일 추가 ✅
  - `ExcelBinder/settings.json.example` 생성 (전체 설정 키 포함)

- [x] **L-04** BoundFeatures 경로 중복 방지 → `Path.GetFullPath` 정규화 ✅
  - `MainViewModel.cs` ExecuteBindFeature, ExecuteBindFeatureFolder에 `Path.GetFullPath` 적용

- [x] **L-05** EnumProcessor 동일 엑셀 파일 반복 읽기 최적화 ✅
  - `ExcelService.ReadMultipleSheets` 메서드 추가 (1회 오픈으로 여러 시트 일괄 읽기)
  - `EnumProcessor.ProcessFile`에서 모든 시트를 한 번에 읽어 캐시 후 `GenerateEnum`에 전달

- [x] **L-06** `ExportService.CreateHeaderMap` null 처리 명확화 ✅
  - L-01에서 함께 해결 (GroupBy `x.h!` null-forgiving 적용)

- [x] **L-07** ~~`ChatMessage`에서 View 종속성 제거~~ N/A (AI 기능 제거로 해당 없음)
