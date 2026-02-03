# Quick Reference

> 자주 사용하는 명령어, 코드 패턴, 지시법을 요약한 빠른 참조용 문서입니다.

---

## 🚀 빌드 및 실행

### 빌드
```bash
cd ExcelBinder
dotnet build                    # Debug 빌드 (../Build 폴더)
dotnet build -c Release         # Release 빌드
dotnet clean                    # Clean
```

### 실행
```bash
dotnet run                                          # GUI 실행
dotnet run -- --feature [ID] --export --codegen    # CLI 실행
```

### CLI 옵션
| 옵션 | 설명 |
|------|------|
| `--feature [ID]` | 대상 Feature ID 지정 |
| `--export` | 데이터 추출 (Binary/JSON) |
| `--codegen` | 코드 생성 실행 |
| `--both` | export + codegen 둘 다 실행 |
| `--all` | 모든 파일 처리 |

---

## 💻 자주 쓰는 코드 패턴

### 비동기 메서드 (필수)
```csharp
private async void ExecuteExport()
{
    if (IsBusy) return;
    try
    {
        IsBusy = true;
        await processor.ExecuteExportAsync(this);
    }
    finally
    {
        IsBusy = false;
    }
}
```

### 에러 처리 (데이터 무결성 우선)
```csharp
if (schema == null)
    throw new FileNotFoundException("Schema not found");

if (string.IsNullOrEmpty(apiKey))
    throw new Exception("API Key가 설정되지 않았습니다.");
```

### 속성 (ViewModelBase 상속)
```csharp
private string _name;
public string Name { get => _name; set => SetProperty(ref _name, value); }

private bool _isBusy;
public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
```

### 명령 (RelayCommand)
```csharp
public ICommand SaveCommand { get; }

생성자에서:
SaveCommand = new RelayCommand(ExecuteSave);
```

### 컬렉션 속성
```csharp
public ObservableCollection<FeatureDefinition> Features { get; } = new();
```

---

## 📝 명명 규칙

| 타입 | 규칙 | 예시 |
|------|------|------|
| 클래스 | PascalCase | FeatureService, ViewModelBase |
| 메서드 | PascalCase | LoadSettings, ExportToBinary |
| 속성 | PascalCase | IsBusy, SelectedFeature |
| private 필드 | _camelCase | _feature, _isBusy, _httpClient |
| 인터페이스 | IPascalCase | IFeatureProcessor |
| 상수 (클래스 내) | PascalCase | HeaderRowIndex, CommentPrefix |
| 상수 (전역) | ALL_CAPS | MAX_RETRY_COUNT |

---

## 🎯 단축 지시법

### 커스텀 에이전트 직접 호출
```
@coordinator [지시]
@analyst [지시]
@developer [지시]
@reviewer [지시]
@doc-manager [지시]
@tester [지시]
```
→ 해당 에이전트를 직접 호출하여 작업 수행

**예시:**
```
@coordinator CSV 기능 추가해줘
@analyst 기획서 분석해줘
@developer 코드 작성해줘
@reviewer 코드 리뷰해줘
@doc-manager 문서 업데이트해줘
@tester 테스트해줘
```

### 테스트 관련 지시어
```
@tester 단위 테스트 자동 생성해줘
@tester 단위 테스트 실행해줘
@tester 기능 테스트해줘
```
→ 단위 테스트 자동 생성, 기능 테스트 수행

**예시:**
```
@tester 단위 테스트 자동 생성해줘
@tester 단위 테스트 실행해줘
@tester CSVProcessor 테스트해줘
```

---

### 프로젝트 이해 필요
```
요약: PROJ_SUMMARY 읽고 3줄로 설명
```
→ PROJECT_SUMMARY.md 읽고 핵심 3줄로 요약

---

### 기획서 처리
```
기획: [파일경로]
또는
기획: "기획서 내용"
```
→ WORKFLOW_PLANNING.md 참고 → 분석 → 계획 → 확인

**예시:**
```
기획: ./docs/planning/feature_001.md
기획: "엑셀 데이터 CSV로도 추출 가능하게 해줘"
```

---

### 기능 수정
```
수정: [파일:라인] [문제]
또는
수정: [문제 설명]
```
→ 유형A 분석 → 계획 → 확인 → 구현

**예시:**
```
수정: ExportService.cs:20 null 체크 추가
수정: ExportService에서 null 참조 버그 수정
```

---

### 새로운 기능
```
신규: [기능 설명]
```
→ 유형B 분석 → 계획 → 확인 → 구현

**예시:**
```
신규: CSV 데이터 추출 기능 추가
신규: LogicProcessor 기능 추가
```

---

### 작업 재개
```
재개: WIP-YYYYMMDD-NN
또는
CONTINUE: WIP-YYYYMMDD-NN
```
→ WORK_IN_PROGRESS.md에서 상태 확인 → 재개

**예시:**
```
재개: WIP-20250202-001
CONTINUE: WIP-20250202-001
```

---

### 작업 완료
```
완료: WIP-YYYYMMDD-NN
```
→ 완료 단계 모두 체크 → 완료 작업으로 이동

---

### 작업 취소
```
취소: WIP-YYYYMMDD-NN [사유]
```
→ 활성 작업에서 제거 → 취소 작업으로 이동

**예시:**
```
취소: WIP-20250202-001 우선순위 조정으로 인해
```

---

### 상태 확인
```
상태: WIP-YYYYMMDD-NN
또는
상태: 전체
```

---

### 내보내기
```
내보내기: json
또는
내보내기: markdown
```
→ WORK_IN_PROGRESS.md의 완료/취소 작업을 WORK_HISTORY.json 또는 마크다운으로 내보내기

**예시:**
```
내보내기: json
내보내기: markdown
```

---

### 긴급 버그
```
🚨 [파일:라인] [오류 메시지]
```
→ 즉시 LOG 확인 → 문제 분석 → 수정

**예시:**
```
🚨 ExportService.cs:45 NullReferenceException 발생
🚨 MainViewModel에서 빌드 오류
```

---

## 🔍 에러 해결 체크리스트

### 빌드 오류
- [ ] CS8602: null 가능 참조 역참조 → nullable 확인
- [ ] CS8618: non-nullable 필드 초기화 → required 또는 nullable
- [ ] CS8600: null 값 비-nullable로 변환 → 형식 확인

### 런타임 오류
- [ ] NullReferenceException → null 체크 추가
- [ ] FileNotFoundException → 파일 존재 확인
- [ ] DirectoryNotFoundException → 폴더 존재 확인

---

## 📌 WorkID 형식

### 추천 형식: **WIP-YYYYMMDD-NN**

```
WIP-20250202-001  # 2025년 2월 2일 첫 번째 작업
WIP-20250202-002  # 같은 날 두 번째 작업
WIP-20250203-001  # 다음 날 첫 번째 작업
```

---

## 🔗 빠른 링크

### 메인 문서
- [전체 가이드 목차](AGENTS.md)
- [프로젝트 요약](PROJECT_SUMMARY.md)
- [빠른 참조 (현재 문서)](QUICK_REFERENCE.md)

### 워크플로우
- [기획서 워크플로우](WORKFLOW_PLANNING.md)
- [작업 추적](WORK_IN_PROGRESS.md)
- [작업 히스토리](WORK_HISTORY.json)

### 상세 가이드
- [빌드 및 실행](.guides/BUILD_GUIDE.md)
- [작업 워크플로우](.guides/WORKFLOW_GUIDE.md)
- [코드 스타일](.guides/CODE_STYLE.md)
- [기술 규칙](.guides/TECHNICAL_RULES.md)

---

## 🚨 긴급 상황 대응

### 빌드 오류 발생 시
```
1. 에러 메시지 복사
2. "🚨 [파일:라인] [에러]" 지시
3. 에이전트가 자동으로 분석 및 수정 제안
```

### 작업 중단 시
```
1. "상태: 전체"로 현재 작업 확인
2. 다음 대화에서 "재개: WIP-XXX"로 작업 재개
```

---

## 📊 파일 구조 요약

```
ExcelBinder/
├── Models/          # 데이터 모델
├── ViewModels/      # ViewModelBase 상속
├── Views/           # XAML Views
├── Services/        # 비즈니스 로직
├── .guides/        # 에이전트 가이드
└── [가이드 문서들] # AGENTS.md, WORKFLOW_*, QUICK_REFERENCE 등
```

---

## 💡 기술 원칙 (즉시 확인 필요)

1. **데이터 무결성 우선**: 기본값 대신 예외 발생
2. **비동기 작업**: try-finally로 IsBusy 상태 보장
3. **커밋 메시지**: `[태그] 요약` 형식 (한글)
4. **명명 규칙**: PascalCase / _camelCase

---

## 🎯 새 대화에서의 빠른 시작

### 필독 순서 (중요!)
```
1. AGENTS.md 읽기 (메뉴 확인)
2. PROJECT_SUMMARY.md 읽기 (프로젝트 이해)
3. WORKFLOW_PLANNING.md 읽기 (기획서 처리 방법)
4. WORK_IN_PROGRESS.md 읽기 (현재 진행 중 작업 확인)
5. 필요에 따라 상세 가이드 참조 (CODE_STYLE.md, TECHNICAL_RULES.md 등)
```

### 작업 시작
```
1. AGENTS.md에서 지시 템플릿 확인
2. PROJECT_SUMMARY.md에서 프로젝트 컨텍스트 파악
3. "요약: PROJ_SUMMARY"로 빠른 컨텍스트 전달
4. "기획: ..." 또는 "수정: ..."로 작업 시작
5. 모든 단계에서 WORK_IN_PROGRESS.md 자동 업데이트
```

---

## 📖 상세 참조

**더 상세한 내용이 필요하면:**
- [코드 작성법](.guides/CODE_STYLE.md)
- [기술 규칙](.guides/TECHNICAL_RULES.md)
- [기획서 처리](WORKFLOW_PLANNING.md)
- [작업 추적](WORK_IN_PROGRESS.md)

---

## 📊 보고서 생성

### 보고서 명령어
```
보고서: WIP-YYYYMMDD-NN
```
→ WORK_IN_PROGRESS.md의 완료 작업에서 보고서 생성

### 보고서 형식

#### JSON (WORK_HISTORY.json)
```json
{
  "workId": "WIP-20250202-001",
  "type": "수정",
  "title": "ExportService null 체크 추가",
  "startDate": "2025-02-02T10:00:00",
  "endDate": "2025-02-02T16:30:00",
  "duration": "6.5h",
  "files": ["Services/ExportService.cs"],
  "commit": "abc123"
}
```

#### 마크다운 (reports/WORK_REPORT_WIP-YYYYMMDD-NN.md)
```markdown
# 작업 보고서

## 작업 정보
- **WorkID**: WIP-20250202-001
- **유형**: 기능 수정
- **제목**: ExportService null 체크 추가

## 기간
- **시작**: 2025-02-02 10:00
- **완료**: 2025-02-02 16:30
- **소요 시간**: 6.5시간

## 구현 내용
- Services/ExportService.cs
  - 20번 라인: null 체크 추가

## 테스트 결과
- [x] 빌드 성공
- [x] 기능 테스트 통과
```
