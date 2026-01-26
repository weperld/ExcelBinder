# Changelog

All notable changes to this project will be documented in this file.

## [2.0.0] - 2026-01-26

### Added
- **Enum 및 Constant 코드 생성 기능 도입**
  - **Enum 생성기**: `Definition` 시트와 각 열거형 값 시트를 조합하여 C# Enum 스크립트를 생성. `[Flags]` 어트리뷰트 및 `using System;` 자동 추가 지원.
  - **상수(Constant) 생성기**: 시트별로 독립된 `public static partial class` 상수를 생성. `string`, `float` 등 타입별 자동 포맷팅 지원.
- **카테고리별 전용 엑셀 템플릿 생성 기능**
  - Enum 및 Constants 작성을 즉시 시작할 수 있도록 필수 헤더가 포함된 엑셀 파일을 생성하는 기능 추가.

### Refactor
- **아키텍처 대규모 개편 (MVVM 강화)**
  - **카테고리별 ViewModel/View 분리**: `ExecutionViewModelBase`를 도입하고 각 카테고리(StaticData, Logic, SchemaGen, Enum, Constants)를 독립된 ViewModel 및 View로 격리하여 유지보수성 및 확장성 극대화.
  - **MainViewModel 경량화**: 거대 객체였던 `MainViewModel`에서 실행 관련 로직을 각 카테고리별 ViewModel로 분산.
  - **UI 컴포넌트화**: 헤더, 설정 창, 파일 리스트 등 중복 UI 요소를 `UserControl`로 분리하여 전역 재사용성 확보.
- **테스트 데이터 구조 재편성**
  - `ExternalTestData` 폴더를 카테고리별(StaticData, Logic, Enum, Constants)로 분류하여 독립적인 테스트 환경 구축.

### UI/UX
- **실행 화면 레이아웃 최적화**
  - `Feature Configuration` 영역에 `WrapPanel`을 적용하여 카테고리별 사용 항목에 따라 유동적이고 정돈된 배치를 제공.
  - 사용하지 않는 설정 항목 및 스키마 상태 표시기를 카테고리에 따라 동적으로 숨김 처리.
- **파일 리스트 사용성 개선**
  - **부드러운 스크롤**: 모든 리스트 뷰에 픽셀 단위 스크롤(`VirtualizingPanel.ScrollUnit="Pixel"`) 적용.
  - **전체 영역 토글**: 작은 체크박스 대신 항목 전체 영역을 클릭하여 선택 상태를 전환할 수 있도록 개선.
  - **UI 일관성 확보**: 각 카테고리 간의 들여쓰기, 구분선, 버튼 규격 등을 통일하여 일관된 사용자 경험 제공.

### Fixed
- **CLI 모드 안정화**: 아키텍처 변경에 맞춰 CLI 명령 수행 시에도 신규 카테고리와 프로세서가 정상적으로 작동하도록 수정.
- **레이아웃 이슈 해결**: 특정 카테고리에서 여백이 사라지거나 정렬이 어긋나던 문제들을 전수 조사하여 수정.

## [1.3.0] - 2026-01-25

### Changed
- **전역 행 처리 규칙 변경 및 표준화**
  - 기존 '1행 무시, 2행 헤더' 규칙을 폐기하고, 모든 카테고리(StaticData, Logic, SchemaGen)에서 **'1행 헤더, 2행 데이터 시작'** 규칙을 일관되게 적용.
  - 관련 서비스(`ExcelService`, `ExportService`, `LogicParserService`, `SchemaGeneratorService`, `SchemaEditorViewModel`) 로직 전면 수정.

### Refactor
- **매직 넘버 및 매직 스트링 제거 (ProjectConstants)**
  - `ProjectConstants.cs`에 Excel 행 인덱스, CLI 인자, 파일명 접미사, 타입 구분자 등을 추가하여 전역 상수 관리 체계 강화.
- **코드 중복 제거 및 구조 개선 (ExportService)**
  - `CreateHeaderMap`, `GetColumnIndex` 메서드 추출을 통해 `ExportToBinary`와 `ExportToJson` 간의 중복 로직 제거.
  - `ParsePrimitive` 및 `WritePrimitive` 리팩토링으로 데이터 파싱 로직 단일화.
- **TypeParser 개선**
  - `List<T>` 처리 시 사용되던 하드코딩된 오프셋 숫자를 상수로 교체하여 가독성 및 유지보수성 향상.

### Docs
- **문서 최신화 및 가독성 개선**
  - `README.md`, `README_EXT.md`, `Requirements.md`의 행 규칙 설명 및 `#` 주석 처리 예시를 실제 동작과 일치하도록 업데이트.
  - 루트 `README.md` 레이아웃 재구성 및 AI 프롬프트 템플릿 추가를 통한 접근성 강화.

## [1.2.3] - 2026-01-23

### Added
- **스키마 에디터 UI 개선**
  - 스키마 에디터 상단에 현재 편집 중인 엑셀 파일의 전체 경로를 표시하여 사용자 편의성 강화.

### Fixed
- **UI 가시성 및 동작 최적화**
  - Execution View에서 `StaticData` 카테고리가 아닐 경우 하단 `Format` 섹션을 숨기도록 개선.
  - Schema Editor에서 `Cancel` 버튼 클릭 시 로그 창이 출력되지 않도록 수정.
- **테스트 데이터 정합성**
  - 전역 행 규칙(1행 무시)에 맞춰 테스트용 엑셀 파일 마이그레이션 완료.

### Changed
- **가이드라인 업데이트**
  - 로컬 변경 사항 분류 커밋 규칙 명문화.

## [1.2.2] - 2026-01-23

### Fixed
- **Schema Editor 필터링 및 행 규칙 수정**
  - Schema Editor에서 전역 행 규칙(1행 무시, 2행 헤더)이 적용되지 않던 문제를 수정하여 2행을 헤더로 정상 인식하도록 변경.
  - 헤더 명칭에 `#` 접두사가 붙은 컬럼이 Schema Editor 목록에 노출되지 않도록 필터링 추가.
- **테스트 데이터 마이그레이션**
  - 기존 테스트용 엑셀 파일들을 전역 행 규칙에 맞게 상단에 더미 행을 추가하여 데이터 정합성 확보.

### Added
- **가이드라인 업데이트**
  - 릴리즈 시 별도 지시가 없을 경우 깃허브 릴리즈를 기본 수행하도록 가이드라인 추가.

## [1.2.1] - 2026-01-23

### Added
- **전역 행 처리 규칙 도입**
  - 모든 시트의 첫 번째 행을 무시하고, 두 번째 행을 헤더(기준 행)로 사용하며, 세 번째 행부터 데이터를 추출하는 전역 규칙 적용.

### Fixed
- **데이터 안정성 강화 (ExportService)**
  - 엑셀 헤더에서 컬럼 검색 실패 시, 인덱스 0번 데이터를 임의로 가져오던 로직을 예외 발생(Exception)으로 변경하여 데이터 오염 방지.
- **컴파일 에러 수정**
  - 최적화 과정에서 누락되었던 `FeatureService.LoadFeatures` 메서드 복구.
  - `AIAssistantViewModel`의 `IsBusy` 속성 중복 선언 해결.

## [1.2.0] - 2026-01-23

### Added
- **비동기 처리(Async/Await) 도입**
  - 데이터 내보내기(Export) 및 코드 생성 작업을 비동기 방식으로 전환하여 대규모 작업 중 UI 프리징 현상 제거.
- **로딩 상태 관리(IsBusy)**
  - 작업 진행 중 사용자의 중복 조작을 방지하기 위해 로딩 오버레이(ProgressBar) 및 상태 관리 로직 추가.

### Changed
- **성능 및 리소스 최적화**
  - **Scriban 템플릿 캐싱**: 반복적인 코드 생성 시 템플릿 파싱 비용을 최소화하기 위해 `ConcurrentDictionary` 기반 캐싱 적용.
  - **헤더 인덱스 맵핑**: 데이터 내보내기 시 매 행마다 수행하던 컬럼 검색을 인덱스 맵을 통해 상수 시간(O(1))으로 단축.
  - **특징(Feature) 정의 캐싱**: 파일 시스템 I/O를 줄이기 위해 로드된 특징 파일에 대한 메모리 캐싱 적용.
  - **HttpClient 재사용**: `AIService` 내 `HttpClient`를 정적 멤버로 전환하여 소켓 고갈 방지 및 리소스 효율 개선.

### Fixed
- 비동기 작업 중 예외 발생 시 로딩 상태가 해제되지 않던 문제를 `try-finally` 블록을 통해 해결.

## [1.1.0] - 2026-01-19

### Added
- **카테고리별 UI 가시성 제어 (Feature Builder & Execution View)**
  - 사용자가 선택한 카테고리(`StaticData`, `Logic`, `SchemaGen`)에 따라 불필요한 입력 항목 및 경로 정보를 자동으로 숨김 처리하여 UI 편의성 개선.
- **매직 넘버 및 매직 스트링 상수화**
  - `ProjectConstants.cs`를 신설하여 프로젝트 전반에서 사용되는 상수(카테고리명, 데이터 타입, 파일 확장자, AI 설정 등)를 통합 관리.

### Changed
- **전략 패턴(Strategy Pattern) 도입 및 구조 리팩토링**
  - `IFeatureProcessor` 인터페이스와 카테고리별 프로세서(`StaticDataProcessor`, `LogicProcessor`, `SchemaGenProcessor`)를 도입하여 카테고리별 실행 로직 및 UI 규칙을 캡슐화.
  - `FeatureProcessorFactory`를 통한 객체 생성으로 ViewModel 내 복잡한 `if-else` 분기 제거.
- **공통 서비스 로직 추출**
  - `TypeParser`: 여러 클래스에 산재해 있던 복잡한 타입 파싱 로직을 통합.
  - `ExcelService.GetFilteredData`: 엑셀 데이터 필터링(# 접두사 처리) 로직을 공통 서비스로 이동하여 코드 중복 제거.

### Fixed
- `Logic` 카테고리 선택 시 사용되지 않는 `Export Path` 및 `Schema Path`가 UI에 노출되던 문제 수정.

## [1.0.2] - 2026-01-18

### Added
- **`#` 접두사 기반 데이터 필터링 규칙 도입**
  - **헤더 필터링**: 헤더 명칭이 `#`으로 시작하는 컬럼을 스키마 생성 및 데이터 추출 대상에서 자동 제외하는 기능 추가. (메모용 컬럼 활용 가능)
  - **행 필터링**: 첫 번째 유효 컬럼(Id)의 값이 `#`으로 시작하는 행을 데이터 추출 시 무시하는 기능 추가. (임시 데이터 비활성화 기능)
- **에러 보고 개선**: 데이터 추출 중 오류 발생 시, 실제 엑셀 시트상의 행 번호를 출력하도록 개선하여 디버깅 편의성 증대.

### Changed
- `SchemaGeneratorService.cs`: `#` 헤더 제외 로직 및 첫 번째 유효 컬럼 자동 Key 설정 로직 적용.
- `ExportService.cs`: `ExportToBinary`, `ExportToJson` 메서드에 행 필터링 규칙 적용.

### Fixed
- `.gitignore`: `*.zip` 파일이 저장소에 포함되지 않도록 수정.

## [1.0.1] - 2026-01-17
### Fixed
- Release workflow 태그 인식 오류 수정.

## [1.0.0] - 2026-01-17
### Added
- 초기 릴리즈: 범용 엑셀 추출 및 코드 생성 엔진 기능 탑재.
