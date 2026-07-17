# ExcelBinder

## 검색 보조 도구 (선택)

대규모 탐색 시 `.search/ai-grep`을 보조 도구로 활용할 수 있다 (일반 검색은 기본 도구로 충분).

```bash
# 접두어 (Windows): PYTHONIOENCODING=utf-8 python .search/ai-grep

ai-grep relevant "query" --top 5     # 관련 파일 랭킹 (--top은 relevant 전용)
ai-grep search "keyword" --limit 10  # 전문 검색
ai-grep refs "ClassName" --context 3 # 심볼 참조 검색
ai-grep outline "file.cs"            # 파일 구조 확인
```

## 프로젝트 개요

엑셀 데이터를 추출하여 JSON/Binary로 변환하고, C# 코드를 자동 생성하는 WPF 데스크톱 애플리케이션

- **기술 스택**: C#, WPF, .NET 10.0, MVVM 패턴
- **라이브러리**: NPOI 2.7.5 (Excel 처리), Newtonsoft.Json 13.0.4 (JSON 직렬화), Scriban 6.5.2 (템플릿 엔진)
- **출력 포맷**: Binary (.bytes), JSON (.json)
- **상세 정보**: PROJECT_SUMMARY.md 참조

### 아키텍처
- **Models/**: 데이터 모델 (FeatureDefinition, SchemaDefinition, AppSettings, ProjectConstants)
- **ViewModels/**: MVVM ViewModel (MainViewModel, ExecutionViewModelBase + 카테고리별 실행 VM)
- **Views/**: XAML UI (MainWindow, DashboardView 등)
- **Services/**: 비즈니스 로직 (ExcelService, ExportService, CodeGeneratorService)
- **Services/Processors/**: Feature별 데이터 처리기 (IFeatureProcessor 구현 5종)

### 핵심 흐름
Excel Load → ExcelService.ReadExcel()/ReadMultipleSheets() → ExcelService.GetFilteredData() (`#` 필터) → Processor.ExecuteExportAsync()/ExecuteGenerateAsync() → ExportService (JSON/Binary) / CodeGeneratorService (C# 코드)

### 엑셀 필터링 규칙 (`#` 접두어)
- **열 무시**: 헤더 행에서 `#`이 붙은 열은 스키마 생성에서 제외 (바이너리/JSON 출력도 제외)
- **행 무시**: 데이터 행에서 첫 번째 유효 열의 값이 `#`으로 시작하면 해당 행 추출 제외
- **시트 무시**: 시트명이 `#`으로 시작하면 해당 시트 무시
- **헤더 행**: 1행, **데이터 시작 행**: 2행

### 도메인 용어
- **Feature**: 기능 카테고리 - StaticData, Logic, SchemaGen, Enum, Constants (`ProjectConstants.Categories`)
- **Processor**: Feature별 데이터 처리기 (`Services/Processors/`)
- **FeatureDefinition**: Feature 정의 JSON의 데이터 모델 (`Models/FeatureDefinition.cs`)
- **SchemaDefinition**: 엑셀 헤더 → C# 타입 매핑 정의 (`Models/SchemaDefinition.cs`)
- **CodeGen**: Scriban 템플릿 기반 C# 코드 자동 생성 (`Services/CodeGeneratorService.cs`)

### Feature 그룹화 (Dashboard)
- Dashboard 좌측 사이드바에 그룹 리스트 표시: "전체" (가상, 항상 첫 번째, 이름변경/삭제 불가) + 사용자 정의 그룹
- 그룹 정보는 `Features/_groups.json`에 저장 (FeatureDefinition은 그룹 소속 정보를 보유하지 않음 — 단방향 참조)
- 한 Feature는 여러 사용자 그룹에 동시 소속 가능 (다대다)
- 그룹 관리: 사이드바 항목 우클릭(이름변경/삭제), 빈 공간 우클릭 또는 "+" 버튼(새 그룹)
- Feature 매핑: Feature 카드 우클릭 → "그룹에 추가/제거" 서브메뉴 (체크박스 토글, 즉시 반영 + 저장)
- 그룹명 중복 거부, "전체"라는 이름 사용 불가
- 정합성: Feature 삭제 시 lazy GC (Refresh 시점에 dangling ID 자동 정리)
- 마지막 선택 그룹은 `AppSettings.LastSelectedGroupId`로 영속화

### 인앱 사용자 가이드 (GuideWindow)
- 별도 가이드 윈도우는 좌측 사이드바 트리(카테고리 → 토픽 2단) + 우측 본문(`FlowDocumentScrollViewer`) 구조
- 콘텐츠는 `Resources/Guides/*.xaml`(FlowDocument 형식)로 작성하고 `<Resource>`로 빌드 임베드, 트리는 `_index.json`(EmbeddedResource)이 정의
- 진입점: 메인 윈도우 우상단 `?` 버튼 + `F1` 단축키(메인은 `ShowDialog` 모달), FeatureBuilderView/SchemaEditorView 헤더 `?` 아이콘(모달리스 `Show`로 페이지 작업과 병행 가능)
- 첫 실행 시 `AppSettings.HasSeenGuide=false`이면 "시작하기" 토픽으로 자동 표시(`MainWindow.Loaded` → `Dispatcher.BeginInvoke(Background)`), "다시 보지 않기" 체크 후 닫기 시에만 영속화
- TreeView는 커스텀 `ControlTemplate`로 기본 파란 하이라이트를 부드러운 톤(`#DDE5F5`/`#EEF2FA`)으로 교체, 카테고리 노드 클릭은 본문에 영향 없음(`SelectedItemChanged`에서 `GuideTopic`만 통과)
- 본문은 `GuideViewModel`에서 로드 후 `ApplyReadabilityDefaults`로 FontSize 16/LineHeight 28/ColumnWidth 960 적용, `ApplySectionAlternation`으로 H2(SemiBold + 18~22pt) 단위 옅은 배경 교차(`#F8FAFC`/`#EEF2F8`)
- 휠 스크롤은 `PreviewMouseWheel`을 가로채 `WheelScrollLines × 1.5`만큼 `LineUp/LineDown` 호출(기본 동작 차단)
- 새 토픽 추가 시: `Resources/Guides/NN_X.xaml` 생성 → `_index.json`에 `{ id, title, resource }` 등록 → `.csproj`의 `<None Remove>`/`<Page Remove>`/`<Resource Include>` 3쌍 추가

### Enum 타입 필드 처리 (StaticData 스키마)
- SchemaEditor의 Data Type 드롭다운에서 `enum`을 선택한 경우에만 Enum Type 입력 활성
- Schema JSON 토큰: `"FieldName": "enum:MyEnumType"` (List는 `"List<enum:MyEnumType>"`)
- 엑셀 셀 표기: enum 멤버 이름 그대로 (예: `Fire`)
- Export 출력: Binary/JSON 모두 셀 값 문자열 그대로 기록
- 생성 코드 읽기: `System.Enum.Parse<MyEnumType>(reader.ReadString())` 인라인, **대소문자 구분**
- 빈 셀 또는 잘못된 enum 이름은 런타임 예외 발생 (조기 발견 정책)
- Underlying type은 enum 정의 측에서 결정 (SchemaEditor에서 검증하지 않음)

## 절대 규칙
- **타입 안전성**: 무조건 캐스팅 (Type)cast 남용 금지, dynamic 사용 최소화
- **빈 catch 블록 금지**: catch(e) {} 사용 금지
- **추측 금지**: 모호한 요청은 반드시 사용자에게 확인

## 빌드/실행

```bash
dotnet build ExcelBinder/

dotnet test ExcelBinder.Tests/

dotnet run --project ExcelBinder/

dotnet run --project ExcelBinder/ -- --feature [FeatureID] [옵션]
# 옵션: --export, --codegen, --both, --all
```

## 명명 규칙

- **클래스/메서드/속성**: PascalCase
- **private 필드**: _camelCase
- **인터페이스**: IPascalCase
- **파일명**: 클래스명과 동일

## 커밋 규칙

`.guides/COMMIT_RULES.md` 참조 — `[태그] 요약` 형식, 논리 단위별 그룹핑, force push 금지.
