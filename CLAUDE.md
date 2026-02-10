# ExcelBinder - Claude Code 프로젝트 설정

## 프로젝트 개요

ExcelBinder는 엑셀 데이터를 추출하여 JSON/Binary로 변환하고, C# 코드를 자동 생성하는 WPF 데스크톱 애플리케이션입니다.

- **기술 스택**: C#, WPF, .NET 10.0, MVVM 패턴
- **라이브러리**: NPOI 2.7.5 (Excel 처리), Newtonsoft.Json 13.0.4 (JSON 직렬화), Scriban 6.5.2 (템플릿 엔진)
- **출력 포맷**: Binary (.bytes), JSON (.json)
- **기능 카테고리**: StaticData, Logic, SchemaGen, Enum, Constants
- **상세 정보**: PROJECT_SUMMARY.md 참조

### 프로젝트 구조

```
ExcelBinder/
├── Models/          # 데이터 모델
├── ViewModels/      # MVVM ViewModel
├── Views/           # XAML UI
├── Services/        # 비즈니스 로직
│   └── Processors/  # 데이터 처리기
└── ...
```

### 엑셀 필터링 규칙

- **열 필터링 접두어**: `#` (해당 열 무시)
- **행 필터링 접두어**: `#` (해당 행 무시)
- **헤더 행**: 1행
- **데이터 시작 행**: 2행

---

## 필수 참조 문서

작업 전 반드시 해당 문서를 확인하세요:

| 문서 | 경로 | 용도 |
|------|------|------|
| **프로젝트 요약** | `PROJECT_SUMMARY.md` | 30초 프로젝트 이해 |
| **에이전트 규칙** | `AGENTS.md` | 절대 규칙, Self-Validation, Cross-Stage Review |
| **에이전트 역할** | `AGENT_ROLES.md` | 각 에이전트 역할 정의 |
| **워크플로우** | `WORKFLOW_PLANNING.md` | 자동 업데이트 시스템, WIP 관리 |
| **작업 현황** | `WORK_IN_PROGRESS.md` | 현재 진행 중인 작업 |
| **빠른 참조** | `QUICK_REFERENCE.md` | 자주 사용하는 명령어/패턴 |

### 개발 가이드 (.guides/)

| 문서 | 용도 |
|------|------|
| `.guides/BUILD_GUIDE.md` | 빌드 및 개발 절차 |
| `.guides/CODE_STYLE.md` | C# 코드 스타일 (PascalCase, _camelCase, MVVM) |
| `.guides/TECHNICAL_RULES.md` | 기술 요구사항 및 표준 |
| `.guides/WORKFLOW_GUIDE.md` | 워크플로우 절차 |
| `.guides/TEST_GUIDE.md` | 테스트 표준 |
| `.guides/COMMIT_RULES.md` | Git 커밋 규칙 |
| `.guides/PLANNING_TEMPLATE.md` | 기획 문서 템플릿 |

---

## 절대 규칙 (Hard Blocks)

> AGENTS.md의 절대 규칙 섹션을 반드시 준수하세요.

핵심 규칙 요약:
- **데이터 무결성 우선**: 기본값 대신 명시적 예외(exception) 발생
- **비동기 패턴**: try-finally로 IsBusy 상태 보장
- **빈 catch 블록 금지**: `catch(e) {}` 사용 금지
- **MVVM 패턴 준수**: ViewModel에서 View 직접 참조 금지
- **추측 금지**: 모호한 요청은 반드시 사용자에게 확인

---

## 커스텀 명령어

`.claude/commands/` 디렉토리에 13개의 명령어가 정의되어 있습니다.
`/project:명령어`로 전체 목록을 확인하세요.

주요 명령어:
- `/project:신규 [기능 설명]` - 새로운 기능 추가
- `/project:수정 [문제 설명]` - 버그 수정 또는 기능 개선
- `/project:커밋` - 변경 사항 커밋 (메시지 자동 생성)
- `/project:전송` - 스테이징 → 커밋 → 푸시 한번에
- `/project:상태 전체` - 전체 작업 상태 확인

---

## 워크플로우 파이프라인

```
Plan → Design → Code → Test → Docs → QA → Review
```

각 단계마다 Gate 검증이 수행되며, 3번 실패 시 이전 단계로 롤백됩니다.
상세 프로세스는 `WORKFLOW_PLANNING.md`를 참조하세요.

---

## WIP 추적 시스템

- **WorkID 형식**: `WIP-YYYYMMDD-NN`
- **활성 WIP**: `.wips/active/{Stage}/WIP-{Stage}-YYYYMMDD-NN.md`
- **완료 WIP**: `.wips/archive/{Stage}/WIP-{Stage}-YYYYMMDD-NN.md`
- **전체 현황**: `WORK_IN_PROGRESS.md`

---

## 작업 중 문서화 규칙 (필수)

다른 PC 또는 다른 사용자가 작업을 이어받을 수 있도록, 모든 개발 작업 시 다음을 준수합니다:

1. **작업 시작** → `WORK_IN_PROGRESS.md`에 WorkID 및 계획 기록
2. **각 단계 완료** → 체크박스 업데이트 + 진행 상황 타임스탬프
3. **중단 시** → 현재 상태, 다음 할 일, 미해결 이슈를 명시적으로 기록
4. **재개 시** → `/project:작업이어하기 WIP-YYYYMMDD-NN`으로 이전 작업 확인 후 이어서 진행

---

## 빌드 및 실행

```bash
# 빌드
dotnet build ExcelBinder/

# 테스트
dotnet test ExcelBinder.Tests/

# GUI 실행
dotnet run --project ExcelBinder/

# CLI 실행
dotnet run --project ExcelBinder/ -- --feature [FeatureID] [옵션]
# 옵션: --export, --codegen, --both, --all
```

## 명명 규칙

- **클래스/메서드/속성**: PascalCase
- **private 필드**: _camelCase
- **인터페이스**: IPascalCase
- **파일명**: 클래스명과 동일
