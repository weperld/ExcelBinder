# 에이전트 및 코파일럿 작업 가이드

> 에이전트 및 코파일럿을 위한 ExcelBinder 프로젝트 작업 가이드입니다.
> 각 항목의 상세 내용은 링크된 세부 문서를 참고하세요.

---

## 📌 에이전트 필독 순서 (중요!)

새로운 대화에서 작업을 시작할 때, **반드시 다음 순서대로 문서를 읽으세요**:

1. **AGENTS.md** (현재 파일) - 전체 메뉴 및 지시 템플릿 확인
2. **PROJECT_SUMMARY.md** - 프로젝트 30초 요약 (핵심 컨텍스트)
3. **WORKFLOW_PLANNING.md** - 기획서 처리 및 자동 업데이트 시스템
4. **WORK_IN_PROGRESS.md** - 현재 진행 중인 작업 확인
5. 필요에 따라 상세 가이드 참조

**커스텀 에이전트 사용:**
- `@coordinator`, `@analyst`, `@architect`, `@developer`, `@reviewer`, `@doc-manager`, `@tester`로 직접 호출 가능
- 자동화 모드: `@coordinator [지시]`로 전체 워크플로우 자동화
- 수동 모드: 각 에이전트를 직접 호출하여 세밀한 컨트롤 가능

**원칙:**
- 모든 단계에서 **WORK_IN_PROGRESS.md를 자동으로 업데이트**하세요
- 사용자가 별도 업데이트 지시할 필요가 없습니다
- 작업 완료/취소 시에도 자동으로 업데이트하세요

---

## 🎯 토큰 절약용 지시 템플릿

### 커스텀 에이전트 직접 호출
```
@coordinator [지시]
@analyst [지시]
@architect [지시]
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
@architect 아키텍처 설계해줘
@developer 코드 작성해줘
@reviewer 코드 리뷰해줘
@doc-manager 문서 업데이트해줘
@tester 테스트해줘
```

---

### 프로젝트 이해 필요
```
요약: PROJ_SUMMARY 읽고 3줄로 설명
```
→ 에이전트가 PROJECT_SUMMARY.md 읽고 핵심 3줄로 요약

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

### 작업 완료/취소
```
완료: WIP-YYYYMMDD-NN
취소: WIP-YYYYMMDD-NN [사유]
상태: WIP-YYYYMMDD-NN 또는 상태: 전체
```

---

### 내보내기
```
내보내기: json
또는
내보내기: markdown
```
→ WORK_IN_PROGRESS.md의 완료/취소 작업을 WORK_HISTORY.json 또는 마크다운으로 내보내기

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

## 📚 문서 인덱스

### 핵심 문서 (반드시 읽어야 함)

| 순서 | 문서 | 용도 | 링크 |
|------|------|------|------|
| 1️⃣ | **AGENTS.md** | 전체 메뉴 및 에이전트 동작 원칙 | [현재 파일](#) |
| 2️⃣ | **PROJECT_SUMMARY.md** | 프로젝트 30초 요약, 빠른 컨텍스트 | [읽기 →](./PROJECT_SUMMARY.md) |
| 3️⃣ | **WORKFLOW_PLANNING.md** | 기획서 처리 및 자동 업데이트 시스템 | [읽기 →](./WORKFLOW_PLANNING.md) |
| 4️⃣ | **WORK_IN_PROGRESS.md** | 현재 진행 중 작업 상태 확인 | [읽기 →](./WORK_IN_PROGRESS.md) |
| 5️⃣ | **QUICK_REFERENCE.md** | 자주 쓰는 명령어, 패턴, 단축어 | [읽기 →](./QUICK_REFERENCE.md) |

### 상세 가이드 (필요 시 참조)

| 섹션 | 설명 | 문서 |
|------|------|------|
| 🆕 | **프로젝트 요약** | [PROJECT_SUMMARY.md](./PROJECT_SUMMARY.md) |
| 🆕 | **에이전트 역할 정의** | [AGENT_ROLES.md](./AGENT_ROLES.md) |
| 🆕 | **기획서 워크플로우** | [WORKFLOW_PLANNING.md](./WORKFLOW_PLANNING.md) |
| 🆕 | **작업 추적** | [WORK_IN_PROGRESS.md](./WORK_IN_PROGRESS.md) |
| 🆕 | **빠른 참조** | [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) |
| 🆕 | **구조화된 컨텍스트** | [CONTEXT.json](./CONTEXT.json) |
| 🆕 | **아키텍처 설계** | [architect.md](./.opencode/agent/architect.md) |
| 🆕 | **커스텀 에이전트** | [.opencode/agent/](./.opencode/agent/) |
| 1️⃣ | **빌드 및 개발** | [BUILD_GUIDE.md](./.guides/BUILD_GUIDE.md) |
| 2️⃣ | **작업 워크플로우** | [WORKFLOW_GUIDE.md](./.guides/WORKFLOW_GUIDE.md) |
| 3️⃣ | **코드 스타일** | [CODE_STYLE.md](./.guides/CODE_STYLE.md) |
| 4️⃣ | **기술적 준수 사항** | [TECHNICAL_RULES.md](./.guides/TECHNICAL_RULES.md) |
| 5️⃣ | **테스트 가이드** | [TEST_GUIDE.md](./.guides/TEST_GUIDE.md) |

---

## 🚀 빠른 참조

### 빌드
```bash
cd ExcelBinder
dotnet build                    # Debug 빌드 (../Build 폴더)
dotnet build -c Release         # Release 빌드
```

### 실행
```bash
dotnet run -- --feature [FeatureID] [options]
# 옵션:
#   --export      데이터 추출 실행
#   --codegen     코드 생성 실행
#   --both        둘 다 실행
#   --all         모든 파일 처리
```

### 기술 원칙 (📌 즉시 확인 필요)
- **데이터 무결성 우선**: 기본값 대신 예외 발생
- **비동기 작업**: try-finally로 IsBusy 상태 보장
- **커밋 메시지**: `[태그] 요약` 형식 (한글)

---

## 🔍 자주 찾는 내용

### 명명 규칙
- 클래스/메서드/속성: `PascalCase`
- private 필드: `_camelCase`
- 인터페이스: `IPascalCase`

### 비동기 패턴 필수
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

### 에러 처리
```csharp
if (schema == null)
    throw new FileNotFoundException("Schema not found");
```

### WorkID 형식
```
WIP-YYYYMMDD-NN
예: WIP-20250202-001
```

---

## 📌 에이전트 가이드 요약

### 시스템 이해
```
📋 문서 구조
├── AGENTS.md (핵심)
├── PROJECT_SUMMARY.md (컨텍스트)
├── WORKFLOW_PLANNING.md (프로세스)
├── WORK_IN_PROGRESS.md (상태 추적)
└── QUICK_REFERENCE.md (빠른 참조)

🔄 자동화 시스템
├── WorkID 생성 (스크립트 또는 자동)
├── WORK_IN_PROGRESS.md 자동 업데이트
├── 완료/취소 자동 처리
└── 보고서 자동 생성
```

### 에이전트 동작 원칙
1. **모든 단계에서 WORK_IN_PROGRESS.md 자동 업데이트**
2. **사용자가 별도 업데이트 지시 불필요**
3. **작업 완료/취소 시 자동 처리**

### 필독 순서
```
새 대화 시작 → AGENTS.md 읽기 → PROJECT_SUMMARY.md 읽기 
→ 필요한 가이드 읽기 → 작업 시작
```

---

## 📖 상세 가이드 확인

- 작업 절차: [WORKFLOW_PLANNING.md](./WORKFLOW_PLANNING.md)
- 작업 추적: [WORK_IN_PROGRESS.md](./WORK_IN_PROGRESS.md)
- 코드 작성법: [CODE_STYLE.md](./.guides/CODE_STYLE.md)
- 기술 규칙: [TECHNICAL_RULES.md](./.guides/TECHNICAL_RULES.md)
