# 에이전트 작업 가이드

> ExcelBinder 프로젝트의 에이전트 필수 규칙과 워크플로우 요약입니다.
> 상세 내용은 각 링크 문서를 참조하세요.

---

## 필독 순서

1. **AGENTS.md** (현재 파일) - 절대 규칙, 검증 체계
2. **PROJECT_SUMMARY.md** - 프로젝트 30초 요약
3. **WORKFLOW_PLANNING/INDEX.md** - 파이프라인, WIP 관리
4. **WORK_IN_PROGRESS.md** - 현재 진행 중 작업

> 필독 순서를 준수했음을 WORK_IN_PROGRESS.md에 기록해야 작업을 시작할 수 있습니다.

---

## 절대 규칙 (Hard Blocks)

> 모든 에이전트 공통. 위반 시: 작업 중단 → 사용자 알림 → 재시도

| 규칙 | 설명 |
|------|------|
| **타입 에러 억제 금지** | `(Type)cast` 남용 금지, `dynamic` 최소화 |
| **빈 catch 블록 금지** | `catch(e) {}` 금지 |
| **테스트 삭제 금지** | 실패한 테스트를 삭제하여 "통과"로 만드는 행위 금지 |
| **무단 커밋 금지** | 사용자의 명시적 요청 없이 커밋 금지 |
| **추측 금지** | 코드를 읽지 않고 추측해서 작업 금지 |
| **파이프라인 파괴 금지** | 개발 파이프라인 지침을 파괴하는 행위 금지 |

---

## 에이전트별 금지 행동

| 에이전트 | 핵심 금지 행동 |
|---------|--------------|
| **coordinator** | 모호한 지시 전달, WorkID 누락, WIP 업데이트 누락, 절대 규칙 위반 통과 |
| **analyst** | 추측 분석, 문서 읽기 생략, 모호한 요청 그대로 진행 |
| **architect** | 데이터 무결성 위반 설계, 비동기 패턴 위반 설계, 기술 규칙 위반 설계 |
| **developer** | 데이터 무결성 위반, 타입 에러 억제 코드 작성 |
| **tester** | 버그 은폐, 테스트 결과 왜곡 |
| **reviewer** | 절대 규칙 위반 코드 통과 |
| **doc-manager** | WIP 업데이트 누락, 버그 은폐/왜곡 문서화 |

> 상세 역할 정의: [AGENT_ROLES.md](./AGENT_ROLES.md)

---

## Self-Validation Checklist (작업 완료 전 필수)

> 수행 결과를 WORK_IN_PROGRESS.md에 기록해야 Cross-Stage Review가 수행됩니다.

```markdown
- [ ] 무조건 캐스팅 (Type)cast 남용하지 않음
- [ ] 불필요한 dynamic 사용하지 않음
- [ ] 빈 catch 블록 없음
- [ ] IsBusy 상태가 try-finally로 보장됨
- [ ] 테스트 삭제하지 않음
- [ ] 커밋 없이 파일 수정하지 않음
- [ ] 추측하지 않음
- [ ] WORK_IN_PROGRESS.md 업데이트 완료
- [ ] 크로스체크 통과 완료
```

위반 시: 즉시 작업 중단 → 사용자에게 보고 → 수정 후 재시도

---

## Cross-Stage Review Chain

각 단계 전이 시 크로스체크 에이전트가 강제 검증을 수행합니다:

| 단계 전이 | 크로스체크 담당 | Gate |
|---------|--------------|------|
| Plan → Design | architect | Gate-1 |
| Design → Code | developer | Gate-2 |
| Code → Test | tester | Gate-3 |
| Test → Docs | developer | Gate-4 |
| Docs → QA | reviewer | Gate-5 |
| QA → Review | architect | Gate-6 |

- 크로스체크 에이전트는 **절대 규칙 준수 여부**를 반드시 검증
- 위반 시 **즉시 해당 단계로 롤백** → 수정 요청
- "크로스체크 통과 완료"를 WIP에 기록하지 않으면 다음 단계 진행 불가

> 상세 Gate 통과 조건: [WORKFLOW_PLANNING/GATES.md](./WORKFLOW_PLANNING/GATES.md)

---

## 사용자 피드백 (강제)

각 단계 완료 후 사용자 피드백을 받아야 다음 단계로 진행할 수 있습니다.

- **규칙 위반함** → 즉시 작업 중단, 수정 후 재시도
- **결과 문제 있음** → 작업 중단, 수정 후 재시도
- **결과 만족** → 다음 단계 진행

> 상세 프로세스: [.guides/FEEDBACK_SYSTEM.md](./.guides/FEEDBACK_SYSTEM.md)

---

## WIP 관리 요약

- **WorkID 형식**: `WIP-YYYYMMDD-NNN` (3자리 순번)
- **템플릿**: `.wips/templates/WIP-{Stage}-YYYYMMDD-NNN.md` (읽기 전용)
- **작성 위치**: `.wips/active/{Stage}/` (복사 후 작성)
- **완료/취소 후**: `.wips/archive/`로 이동하여 보존

> 상세 가이드: [.guides/WIP_GUIDE.md](./.guides/WIP_GUIDE.md)

---

## 지시 형식 원칙

- 모호한 지시 금지 (예: "Design 진행해줘")
- 반드시 **구체적 지시 문서** 또는 **구체적 설명** 포함
- 사용자 지시 부족 시 즉시 구체적 정보 요구

> 상세 형식: [.guides/INSTRUCTION_FORMAT.md](./.guides/INSTRUCTION_FORMAT.md)

---

## 에이전트 동작 원칙

1. 모든 단계에서 **WORK_IN_PROGRESS.md 자동 업데이트**
2. 작업 완료 전 **Self-Validation Checklist** 필수 수행
3. 작업 시작은 커스텀 명령어 사용: `/project:신규`, `/project:수정`, `/project:간편`

---

## 상세 가이드 링크

| 문서 | 용도 |
|------|------|
| [WORKFLOW_PLANNING/INDEX.md](./WORKFLOW_PLANNING/INDEX.md) | 파이프라인 총괄 |
| [WORKFLOW_PLANNING/GATES.md](./WORKFLOW_PLANNING/GATES.md) | Gate 통과 조건 |
| [.guides/WORKFLOW_GUIDE.md](./.guides/WORKFLOW_GUIDE.md) | 워크플로우 절차 |
| [.guides/WIP_GUIDE.md](./.guides/WIP_GUIDE.md) | WIP 템플릿/폴더 구조 |
| [.guides/FEEDBACK_SYSTEM.md](./.guides/FEEDBACK_SYSTEM.md) | 사용자 피드백 시스템 |
| [.guides/INSTRUCTION_FORMAT.md](./.guides/INSTRUCTION_FORMAT.md) | 지시 형식 원칙 |
| [.guides/VERIFICATION_ITEMS.md](./.guides/VERIFICATION_ITEMS.md) | 검증 항목 체크리스트 |
| [.guides/CODE_STYLE.md](./.guides/CODE_STYLE.md) | 코드 스타일 |
| [.guides/TECHNICAL_RULES.md](./.guides/TECHNICAL_RULES.md) | 기술 규칙 |
| [.guides/BUILD_GUIDE.md](./.guides/BUILD_GUIDE.md) | 빌드 및 개발 |
| [.guides/TEST_GUIDE.md](./.guides/TEST_GUIDE.md) | 테스트 표준 |
| [.guides/COMMIT_RULES.md](./.guides/COMMIT_RULES.md) | 커밋 규칙 |
