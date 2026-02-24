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

- [ ] **M-01** 예외 필터 논리 오류 → 명확한 예외 타입 사용
  - `ExportService.cs:183`
  - `when (!(ex is Exception && ...))` 조건이 읽기 어렵고 혼란스러움
  - `catch (FormatException)` 등 명확한 예외 타입으로 변경

- [ ] **M-02** yield return + finally 리소스 관리 → 문서화 또는 List 반환
  - `ExcelService.cs:37-70`
  - 호출자가 열거 완료하지 않으면 workbook 해제 지연 가능
  - 대부분 `.ToList()`로 사용 중이므로 반환 타입을 `List`로 변경 고려

- [ ] **M-03** `_featureCache` 무한 성장 → 캐시 정리 로직 추가
  - `FeatureService.cs:18`
  - static ConcurrentDictionary에 이전 키가 제거되지 않음
  - 파일 경로당 하나의 엔트리만 유지하도록 변경

- [ ] **M-04** 템플릿 캐시 키 최적화 → 해시값 또는 파일 경로 기반으로 변경
  - `TemplateEngineService.cs:13, 24`
  - 템플릿 문자열 전체를 Dictionary 키로 사용

- [ ] **M-05** `AppServices` 정적 Service Locator → DI 컨테이너 도입 또는 null 체크
  - `AppServices.cs`
  - CLI 모드나 테스트에서 NullReferenceException 위험
  - 최소한 null 체크 추가, 이상적으로는 DI 도입

- [ ] **M-06** 서비스 `new` 생성 남발 (13곳) → 생성자 주입 또는 공유 인스턴스
  - Processors 전반 (`StaticDataProcessor`, `LogicProcessor`, `EnumProcessor` 등)
  - 동일 서비스가 메서드마다 반복 생성됨

- [ ] **M-07** Processor 싱글턴 상태 공유 위험 → stateless 보장 또는 매번 생성
  - `FeatureProcessorFactory.cs:9-16`
  - static Dictionary에 프로세서 싱글턴 보관

- [ ] **M-08** Processor 매번 조회 → 생성자에서 한 번만 가져오기
  - `ExecutionViewModelBase.cs:46-49`
  - 프로퍼티 접근마다 `GetProcessor()` 호출

- [ ] **M-09** `async void` 11곳 미처리 예외 → try-catch 추가 또는 AsyncRelayCommand
  - Execution ViewModels 전반
  - 예외 발생 시 앱 크래시 위험

- [ ] **M-10** `CanExecuteChanged` 미연결 → CommandManager 연동 또는 CommunityToolkit 도입
  - `CommonModels.cs:81, 90`
  - 빌드 경고 CS0067 발생, IsBusy 상태와 미연동

- [ ] **M-11** 이벤트 구독 해제 누락 → Settings 교체 시 이전 구독 해제
  - `MainViewModel.cs:137-138`
  - PropertyChanged, CollectionChanged 람다 구독 후 해제 없음

---

## LOW - 선택적 개선

- [ ] **L-01** 빌드 경고 16건 해소
  - CS8602 (null 역참조), CS8600, CS8618, CS8714, CS0067 등
  - Nullable 경고 0건 목표

- [ ] **L-02** 루트 디렉토리 .md 문서 정리
  - 에이전트 전용 문서를 `.guides/` 등 하위 디렉토리로 이동

- [ ] **L-03** `settings.json.example` 샘플 파일 추가
  - 새 환경 설정 가이드 부재

- [ ] **L-04** BoundFeatures 경로 중복 방지 → `Path.GetFullPath` 정규화
  - `MainViewModel.cs` (ExecuteBindFeatureFolder 등)

- [ ] **L-05** EnumProcessor 동일 엑셀 파일 반복 읽기 최적화
  - `EnumProcessor.cs:80, 134`
  - 파일 1회 오픈 후 시트 데이터 캐시

- [ ] **L-06** `ExportService.CreateHeaderMap` null 처리 명확화
  - `ExportService.cs:51-56`
  - nullable annotation 불일치 (CS8714)

- [x] **L-07** ~~`ChatMessage`에서 View 종속성 제거~~ N/A (AI 기능 제거로 해당 없음)
