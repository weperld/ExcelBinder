# WIP 템플릿 및 폴더 구조 가이드

---

## 전체 폴더 구조

```
.wips/
├── templates/           # 템플릿 파일 (읽기 전용)
│   ├── WIP-Plan-YYYYMMDD-NNN.md
│   ├── WIP-Design-YYYYMMDD-NNN.md
│   ├── WIP-Code-YYYYMMDD-NNN.md
│   ├── WIP-Test-YYYYMMDD-NNN.md
│   ├── WIP-Docs-YYYYMMDD-NNN.md
│   └── WIP-QA-YYYYMMDD-NNN.md
└── active/              # 독립 WIP 작성 폴더 (쓰기 전용)
    ├── Plan/
    ├── Design/
    ├── Code/
    ├── Test/
    ├── Docs/
    └── QA/
```

---

## 에이전트별 매핑 테이블

| 에이전트 | 담당 스테이지 | 템플릿 경로 | 작성 경로 |
|---------|-------------|-----------|----------|
| **analyst** | Plan | `.wips/templates/WIP-Plan-YYYYMMDD-NNN.md` | `.wips/active/Plan/` |
| **architect** | Design | `.wips/templates/WIP-Design-YYYYMMDD-NNN.md` | `.wips/active/Design/` |
| **developer** | Code | `.wips/templates/WIP-Code-YYYYMMDD-NNN.md` | `.wips/active/Code/` |
| **tester** | Test | `.wips/templates/WIP-Test-YYYYMMDD-NNN.md` | `.wips/active/Test/` |
| **doc-manager** | Docs | `.wips/templates/WIP-Docs-YYYYMMDD-NNN.md` | `.wips/active/Docs/` |
| **reviewer** | QA | `.wips/templates/WIP-QA-YYYYMMDD-NNN.md` | `.wips/active/QA/` |
| **coordinator** | Review | (전체 관리) | (해당 없음) |

---

## WIP 절대 규칙

1. **템플릿은 읽기 전용**: `.wips/templates/` 폴더의 파일은 절대 수정하지 마세요
2. **독립 WIP는 복사 후 작성**: 템플릿 내용을 복사한 후, 실제 정보로 수정하여 작성
3. **폴더 분리**: 각 스테이지의 독립 WIP는 `.wips/active/{Stage}/` 폴더에만 작성하세요
4. **보존**: 작업 완료/취소 후 독립 WIP 파일은 `.wips/archive/{Stage}/` 폴더로 이동하여 보존하세요

---

## 파일명 형식

**템플릿**: `WIP-{Stage}-YYYYMMDD-NNN.md`
- `{Stage}`: Plan, Design, Code, Test, Docs, QA
- `YYYYMMDD`: 날짜 플레이스홀더
- `NNN`: 3자리 0패딩 순번

**실제 WIP**: `WIP-{Stage}-{YYYYMMDD}-{NNN}.md`
- 예: `WIP-Plan-20250208-001.md`

---

## 필수 변경 항목

### 1. 기본 정보

| 항목 | 템플릿 | 변경 대상 | 예시 |
|------|--------|----------|------|
| WorkID | `WIP-YYYYMMDD-NNN` | 실제 WorkID | `WIP-20250208-001` |
| 생성일 | `YYYY-MM-DD` | 실제 날짜 | `2025-02-08` |
| 상태 | `준비 / 진행 중 / 완료 / 취소` | 작업 상태 | `진행 중` |

> WorkID 생성은 [WORKFLOW_PLANNING/INDEX.md](../WORKFLOW_PLANNING/INDEX.md) 참조

### 2. 지시 정보

| 섹션 | 변경 대상 |
|------|----------|
| 작업 대상 (Target) | 실제 작업 대상 내용 |
| 작업 범위 (Scope) | 실제 작업 범위 내용 |
| 요구사항 (Requirements) | 실제 요구사항 내용 |
| 참고 자료/링크 (References) | 실제 참고 자료/링크 |
| 기대 결과 (Expected Outcome) | 실제 기대 결과 내용 |

### 3. 진척도

| 항목 | 템플릿 | 변경 대상 | 예시 |
|------|--------|----------|------|
| 진척도 | `0% / 25% / 50% / 75% / 100%` | 실제 진척도 | `50%` |

### 4. 체크박스 업데이트

| 상태 | 형식 |
|------|------|
| 미완료 | `[ ] 항목 내용` |
| 완료 | `[x] 항목 내용` |
