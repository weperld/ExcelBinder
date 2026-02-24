# ExcelBinder Project Summary

> 프로젝트를 빠르게 이해하기 위한 핵심 요약입니다.

---

## 🎯 30초 요약

**ExcelBinder**는 엑셀 데이터를 추출하여 JSON/Binary로 변환하고, C# 코드를 자동 생성하는 **WPF 데스크톱 애플리케이션**입니다.

- **입력**: 엑셀 파일 (.xlsx, .xls)
- **출력**: JSON 데이터, Binary 데이터, C# 소스 코드
- **특징**: Feature Definition File (FDF) 기반 범용 엔진

---

## 🏗️ 핵심 아키텍처

### MVVM 패턴
- **Models**: 데이터 모델 (JSON 직렬화 지원)
- **ViewModels**: `ViewModelBase` 상속, `IsBusy` 상태 관리
- **Views**: XAML UI, ViewModel 바인딩

### Feature 기반 시스템
- **FDF (Feature Definition File)**: JSON 설정으로 기능 정의
- **Processors**: 각 카테고리별 전용 프로세서
- **Templates**: Scriban (.liquid)로 코드 생성 100% 제어

### 카테고리
| 카테고리 | 역할 |
|----------|------|
| `StaticData` | 바이너리/JSON 데이터 추출 + 모델 코드 |
| `Logic` | 엑셀 수식 → C# 메서드 변환 |
| `SchemaGen` | 엑셀 구조 → JSON 스키마 자동 생성 |
| `Enum` | Definition 시트 기반 Enum 생성 |
| `Constants` | 시트별 상수 클래스 생성 |

---

## 📁 핵심 파일 구조

```
ExcelBinder/
├── Models/                  # 데이터 모델
│   ├── FeatureDefinition.cs  # FDF 정의
│   ├── SchemaDefinition.cs   # 스키마 정의
│   └── AppSettings.cs       # 앱 설정
│
├── ViewModels/              # MVVM ViewModels
│   ├── ViewModelBase.cs      # 기본 ViewModel (IsBusy)
│   ├── MainViewModel.cs      # 메인 ViewModel
│   └── *ExecutionViewModel.cs # 실행 ViewModels
│
├── Views/                   # XAML Views
│   ├── DashboardView.xaml    # 대시보드
│   ├── FeatureBuilderView.xaml  # 기능 빌더
│   └── ExecutionView.xaml   # 실행 화면
│
├── Services/                # 비즈니스 로직
│   ├── ExcelService.cs       # 엑셀 파싱 (NPOI)
│   ├── ExportService.cs      # 데이터 추출
│   ├── CodeGeneratorService.cs # 코드 생성 (Scriban)
│   ├── UpdateCheckService.cs  # GitHub 릴리즈 버전 체크
│   └── Processors/         # Feature별 프로세서
│       ├── StaticDataProcessor.cs
│       ├── LogicProcessor.cs
│       ├── SchemaGenProcessor.cs
│       ├── EnumProcessor.cs
│       └── ConstantsProcessor.cs
│
└── .guides/               # 📌 에이전트 가이드
    ├── BUILD_GUIDE.md
    ├── WORKFLOW_GUIDE.md
    ├── CODE_STYLE.md
    └── TECHNICAL_RULES.md
```

---

## 🛠️ 기술 스택

| 항목 | 기술 | 버전 |
|------|------|------|
| **언어** | C# | - |
| **프레임워크** | WPF | - |
| **.NET** | .NET 10.0 | 10.0 |
| **엑셀 처리** | NPOI | 2.7.5 |
| **JSON** | Newtonsoft.Json | 13.0.4 |
| **템플릿 엔진** | Scriban | 6.5.2 |

---

## 🚀 빠른 시작

### 빌드
```bash
cd ExcelBinder
dotnet build                    # Debug 빌드 (../Build 폴더)
dotnet build -c Release         # Release 빌드
```

### GUI 실행
```bash
dotnet run
```

### CLI (자동화용)
```bash
# 특정 기능에 대한 데이터 추출 및 코드 생성
dotnet run -- --feature my_project_data --export --codegen

# 모든 파일 처리
dotnet run -- --feature my_project_data --all --export --codegen
```

---

## 🎯 주요 기능

### 1. 시트 기반 추출
- 엑셀 파일명이 아닌 **시트명(Sheet Name)** 기준 스키마 매칭
- 하나의 파일에서 여러 시트 관리 가능

### 2. 데이터 필터링 (`#` 접두사)
```excel
| Id | #Desc | Value |
|----|-------|-------|
| 101 | 설명 | 100    |  → 추출됨
| #102| 설명 | 200    |  → 제외됨 (행)
```
- 컬럼 헤더가 `#`으로 시작하면 컬럼 제외
- 첫 번째 유효 컬럼의 값이 `#`으로 시작하면 행 제외

### 3. 유연한 타입 매핑
- 엑셀 타입 → 프로젝트 전용 타입 변환
- 예: `"int"` → `"int32"`, `"string"` → `"string"`

### 4. AI Assistant
- OpenAI/Claude API 연동
- 대화형 템플릿 생성 및 가이드

---

## 📌 에이전트 필독 순서

새로운 대화에서 작업을 시작할 때:

1. **AGENTS.md** 읽기 (메뉴 및 지시 템플릿)
2. **PROJECT_SUMMARY.md** 읽기 (현재 파일)
3. **QUICK_REFERENCE.md** 참조 (자주 쓰는 패턴)
4. 필요한 가이드 읽기 (CODE_STYLE.md, WORKFLOW_PLANNING/INDEX.md 등)
5. **WORK_IN_PROGRESS.md** 확인 (진행 중인 작업)

---

## 🔄 개발 프로세스

```
기획서 전달 → 유형 분석 → 계획 수립 → 사용자 확인 → 구현 → 보고
```

**상세 절차:** [WORKFLOW_PLANNING/INDEX.md](./WORKFLOW_PLANNING/INDEX.md)

---

## 🚨 긴급 상황

빌드 오류나 런타임 오류 발생 시:
```
🚨 [파일:라인] [오류 메시지]
```

예시:
```
🚨 ExportService.cs:45 NullReferenceException 발생
```

---

## 📖 상세 문서

| 항목 | 문서 |
|------|------|
| 전체 가이드 목차 | [AGENTS.md](./AGENTS.md) |
| 빌드 및 실행 | [.guides/BUILD_GUIDE.md](./.guides/BUILD_GUIDE.md) |
| 기획서 처리 | [WORKFLOW_PLANNING/INDEX.md](./WORKFLOW_PLANNING/INDEX.md) |
| 작업 추적 | [WORK_IN_PROGRESS.md](./WORK_IN_PROGRESS.md) |
| 빠른 참조 | [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) |
| 코드 스타일 | [.guides/CODE_STYLE.md](./.guides/CODE_STYLE.md) |
| 기술 규칙 | [.guides/TECHNICAL_RULES.md](./.guides/TECHNICAL_RULES.md) |
