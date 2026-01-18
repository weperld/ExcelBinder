# Changelog

All notable changes to this project will be documented in this file.

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
