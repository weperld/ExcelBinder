# 📋 ExcelBinder 요구사항 및 기능 명세

본 문서는 ExcelBinder 프로젝트의 핵심 요구사항과 구현된 기능을 정의합니다.

---

## 1. ⚙️ 핵심 기능 (Core Logic)

- [x] **범용 추출 엔진**
  - [x] `StaticData`, `Logic`, `SchemaGen` 카테고리별 특화 로직.
  - [x] **시트(Sheet) 기반 처리**: 엑셀 파일명이 아닌 시트명을 기준으로 스키마 매칭.
  - [x] **필터링 규칙 (`#` 접두사)**:
    - [x] `#`으로 시작하는 헤더(컬럼)는 스키마 및 데이터 추출 대상에서 자동 제외.
    - [x] 첫 번째 유효 컬럼(주로 ID)의 값이 `#`으로 시작하는 행은 데이터 추출 시 제외.
  - [x] 바이너리(.bytes) 및 JSON 동시 추출 지원.
- [x] **FDF (Feature Definition File) 시스템**
  - [x] 외부 JSON 설정을 통한 동적 기능 바인딩.
  - [x] 프로젝트별 독립적인 설정 관리 (Paths, Mappings, Templates).
- [x] **Scriban 템플릿 엔진 연동**
  - [x] `.liquid` 템플릿을 통한 100% 자유로운 코드 생성 제어.

## 🖥️ 2. UI/UX 요구사항 (Windows GUI)

- [x] **메인 대시보드 (Main Dashboard)**
  - [x] 등록된 기능을 카드 형태로 시각화.
  - [x] 각 카드별 즉시 편집(Edit) 기능 제공.
- [x] **Feature Builder (설정 편집기)**
  - [x] 복잡한 JSON 구조를 GUI 폼으로 편집 및 생성.
  - [x] 윈도우 탐색기 연동을 통한 경로 선택.
- [x] **작업 실행 뷰 (Execution View)**
  - [x] 대상 파일 자동 스캔 및 필터링.
  - [x] 체크박스를 이용한 일괄 작업 관리.

## ⌨️ 3. 실행 환경 및 인프라

- [x] **Windows Desktop (WPF)**: .NET 10.0 기반의 사용자 친화적 환경.
- [x] **CLI (Command Line Interface)**: CI/CD 연동을 위한 인자 방식 실행 지원.
- [ ] **Logging System**: (진행 예정) 상세 실행 로그 저장 및 조회 기능.

---

## 🛠️ 기술 스택 (Tech Stack)

| 구분 | 기술 / 라이브러리 |
| :--- | :--- |
| **Framework** | .NET 10.0, WPF |
| **Excel Parsing** | NPOI |
| **JSON Library** | Newtonsoft.Json |
| **Template Engine** | Scriban |
| **Icons** | MahApps.Metro.IconPacks (추천) |