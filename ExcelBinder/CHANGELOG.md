# Changelog

All notable changes to this project will be documented in this file.

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
