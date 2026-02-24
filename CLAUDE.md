# ExcelBinder

## 검색 규칙 (필수 준수)
- 코드 검색 시 반드시 AI-grep을 먼저 사용할 것
- 전체 파일을 읽지 말고 `--lines N-M` 옵션으로 필요한 부분만 읽을 것
- 직접 파일 탐색은 AI-grep으로 찾을 수 없을 때만 최후의 수단으로 사용
- Windows 환경: 반드시 `PYTHONIOENCODING=utf-8 python .search/ai-grep` 형식으로 실행

## 검색 도구 사용법
```bash
PYTHONIOENCODING=utf-8 python .search/ai-grep stats                        # 코드베이스 개요
PYTHONIOENCODING=utf-8 python .search/ai-grep relevant "query" --top 5     # 관련 파일 랭킹
PYTHONIOENCODING=utf-8 python .search/ai-grep search "keyword"             # 전문 검색 + 스니펫
PYTHONIOENCODING=utf-8 python .search/ai-grep refs "ClassName"             # 심볼 참조 검색
PYTHONIOENCODING=utf-8 python .search/ai-grep get "file.cs" --lines 10-50  # 특정 줄만 읽기
PYTHONIOENCODING=utf-8 python .search/ai-grep outline "file.cs"            # 파일 구조 확인
```

## 프로젝트 개요

엑셀 데이터를 추출하여 JSON/Binary로 변환하고, C# 코드를 자동 생성하는 WPF 데스크톱 애플리케이션

- **기술 스택**: C#, WPF, .NET 10.0, MVVM 패턴
- **라이브러리**: NPOI 2.7.5 (Excel 처리), Newtonsoft.Json 13.0.4 (JSON 직렬화), Scriban 6.5.2 (템플릿 엔진)
- **출력 포맷**: Binary (.bytes), JSON (.json)
- **상세 정보**: PROJECT_SUMMARY.md 참조

### 아키텍처
- **Models/**: 데이터 모델 (BinderData, ColumnInfo, RowData)
- **ViewModels/**: MVVM ViewModel (MainViewModel)
- **Views/**: XAML UI (MainWindow)
- **Services/**: 비즈니스 로직 (ExcelLoaderService, ExportService, CodeGenService)
- **Services/Processors/**: Feature별 데이터 처리기

### 핵심 흐름
Excel Load → ExcelLoaderService.Parse() → ColumnFilter/RowFilter → Processor.Process() → ExportService.Export() (JSON/Binary/Code)

### 엑셀 필터링 규칙 (`#` 접두어)
- **열 무시**: 헤더 행에서 `#`이 붙은 열은 스키마 생성에서 제외 (바이너리/JSON 출력도 제외)
- **행 무시**: 데이터 행에서 첫 번째 유효 열의 값이 `#`으로 시작하면 해당 행 추출 제외
- **시트 무시**: 시트명이 `#`으로 시작하면 해당 시트 무시
- **헤더 행**: 1행, **데이터 시작 행**: 2행

### 도메인 용어
- **Feature**: 기능 카테고리 - StaticData, Logic, SchemaGen, Enum, Constants (`Models/FeatureType.cs`)
- **Processor**: Feature별 데이터 처리기 (`Services/Processors/`)
- **BinderData**: 엑셀에서 추출된 데이터 구조 (`Models/BinderData.cs`)
- **CodeGen**: Scriban 템플릿 기반 C# 코드 자동 생성 (`Services/CodeGenService.cs`)

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

## 작업 방식

모든 코드 변경은 커스텀 명령어로 시작한다.
- 대규모 (새 기능, 아키텍처 변경): `/project:신규` 또는 `/project:수정`
- 중소규모 (버그 수정, 리팩토링): `/project:간편`
- 명령어 없이 직접 수정 요청 시, 규모를 판단하여 적절한 명령어 사용을 안내한다.
- 명령어 목록: `/project:명령어`
