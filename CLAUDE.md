# ExcelBinder

## 프로젝트 개요

엑셀 데이터를 추출하여 JSON/Binary로 변환하고, C# 코드를 자동 생성하는 WPF 데스크톱 애플리케이션

- **기술 스택**: C#, WPF, .NET 10.0, MVVM 패턴
- **라이브러리**: NPOI 2.7.5 (Excel 처리), Newtonsoft.Json 13.0.4 (JSON 직렬화), Scriban 6.5.2 (템플릿 엔진)
- **출력 포맷**: Binary (.bytes), JSON (.json)
- **상세 정보**: PROJECT_SUMMARY.md 참조

### 프로젝트 구조

ExcelBinder/
├── Models/          # 데이터 모델
├── ViewModels/      # MVVM ViewModel
├── Views/           # XAML UI
├── Services/        # 비즈니스 로직
│   └── Processors/  # 데이터 처리기
└── ...

### 엑셀 필터링 규칙
- **열 필터링 접두어**: `#` (해당 열 무시)
- **행 필터링 접두어**: `#` (해당 행 무시)
- **헤더 행**: 1행
- **데이터 시작 행**: 2행

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
