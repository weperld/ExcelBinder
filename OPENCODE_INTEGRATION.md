# OpenCode 플러그인 연동 가이드

> ExcelBinder 프로젝트에서 OpenCode 플러그인과 커스텀 에이전트를 연동하는 방법

---

## 🎯 OpenCode 플러그인 개요

### OpenCode 플러그인이란?

OpenCode는 AI 에이전트 시스템을 구축하기 위한 플러그인입니다.

**핵심 기능:**
- `.opencode/agent/` 폴더의 마크다운 파일을 자동으로 에이전트로 등록
- `@coordinator`, `@analyst` 등의 토큰을 감지하여 해당 에이전트를 호출
- AI 모델이 마크다운 파일의 Role, Responsibilities, Workflow를 참고하여 동작

---

## 🔧 설치 및 설정

### 1. 설치

```bash
cd .opencode
npm install
```

### 2. `package.json` 확인

```json
{
  "dependencies": {
    "@opencode-ai/plugin": "1.1.51"
  }
}
```

### 3. `.opencode/agent/` 폴더 구조

```
.opencode/
├── agent/
│   ├── coordinator.md   → @coordinator 토큰
│   ├── analyst.md      → @analyst 토큰
│   ├── developer.md    → @developer 토큰
│   ├── reviewer.md     → @reviewer 토큰
│   ├── doc-manager.md  → @doc-manager 토큰
│   └── tester.md       → @tester 토큰
└── package.json
```

---

## 📋 커스텀 에이전트 등록 방법

### 방법 1: 마크다운 파일로 등록 (추천)

#### 1. 에이전트 파일 생성

`.opencode/agent/myagent.md` 파일을 생성합니다.

#### 2. 에이전트 정의

```markdown
# My Agent

## Role
내 에이전트의 역할 설명

## Responsibilities
- 책임 1
- 책임 2

## Must-Read Documents
- AGENTS.md
- PROJECT_SUMMARY.md

## Workflow
1. 단계 1
2. 단계 2

## Examples
...
```

#### 3. 자동 등록

- **파일명 기반 자동 등록**: `myagent.md` → `@myagent` 토큰
- **대소문자 무시**: `MyAgent.md` → `@myagent` 토큰
- **언더스코어 처리**: `my_agent.md` → `@my-agent` 토큰

#### 4. 사용 방법

```
사용자: "@myagent 작업 해줘"

@myagent:
✅ 작업 완료
...
```

---

### 방법 2: 복합 에이전트 (여러 역할)

#### 1. 에이전트 파일 생성

`.opencode/agent/fullstack.md` 파일을 생성합니다.

#### 2. 에이전트 정의

```markdown
# FullStack Agent

## Role
전체 스택 개발 에이전트 (분석 + 개발 + 테스트)

## Responsibilities
- 기획서 분석
- 코드 작성
- 테스트 수행

## Must-Read Documents
- AGENTS.md
- PROJECT_SUMMARY.md
- CODE_STYLE.md

## Workflow
1. 기획서 분석
2. 코드 작성
3. 테스트 수행

## Examples
...
```

#### 3. 사용 방법

```
사용자: "@fullstack 기능 A 추가해줘"

@fullstack:
✅ 기능 A 추가 완료

[수행 내용]
- 기획서 분석 완료
- 코드 작성 완료
- 테스트 수행 완료
...
```

---

## 🚀 에이전트 호출 방법

### 기본 호출

```
@coordinator [지시]
@analyst [지시]
@developer [지시]
@reviewer [지시]
@doc-manager [지시]
@tester [지시]
```

### 옵션 지정

```
@developer --mode=fast 코드 작성해줘
@analyst --deep=true 분석해줘
```

---

## 🔄 에이전트 간 통신

### 통신 방식

**WORK_IN_PROGRESS.md를 단일 진실 공급원(Single Source of Truth)으로 사용:**

1. **각 에이전트의 첫 번째 동작**: WORK_IN_PROGRESS.md 읽기
2. **각 에이전트의 마지막 동작**: WORK_IN_PROGRESS.md 업데이트
3. **다음 에이전트에게 명시적인 전달 불필요**: WORK_IN_PROGRESS.md가 자동으로 상태 전달

### 통신 흐름 예시

```
@coordinator
  ↓ (WORK_IN_PROGRESS.md 업데이트: WorkID 생성)
  ↓
@analyst (WORK_IN_PROGRESS.md 읽기)
  ↓ (WORK_IN_PROGRESS.md 업데이트: Plan 단계 완료)
  ↓
@developer (WORK_IN_PROGRESS.md 읽기)
  ↓ (WORK_IN_PROGRESS.md 업데이트: Code 단계 완료)
  ↓
@reviewer (WORK_IN_PROGRESS.md 읽기)
  ↓ (WORK_IN_PROGRESS.md 업데이트: QA 단계 완료)
```

---

## 📝 에이전트 파일 구조

### 필수 섹션

#### 1. Role

```markdown
## Role
에이전트의 역할 설명
```

**예시:**
```markdown
## Role
ExcelBinder 프로젝트의 전체 작업을 조율하고 적절한 전문가에게 위임하는 코디네이터
```

#### 2. Responsibilities

```markdown
## Responsibilities
- 책임 1
- 책임 2
```

**예시:**
```markdown
## Responsibilities
- 사용자 지시 파악 및 적절한 전문가에게 위임
- 전체 워크플로우 자동화
- 작업 진행 상황 모니터링
```

#### 3. Must-Read Documents (선택 사항)

```markdown
## Must-Read Documents
- 문서1
- 문서2
```

**예시:**
```markdown
## Must-Read Documents
- AGENTS.md
- PROJECT_SUMMARY.md
- WORKFLOW_PLANNING.md
```

#### 4. Workflow (선택 사항)

```markdown
## Workflow
1. 단계 1
2. 단계 2
```

**예시:**
```markdown
## Workflow
1. 사용자 지시 수신
2. WorkID 생성
3. WORK_IN_PROGRESS.md에 작업 등록
4. @analyst에게 기획서 분석 위임
5. @developer에게 구현 위임
6. @reviewer에게 리뷰 위임
7. @tester에게 테스트 위임
8. WORK_IN_PROGRESS.md 업데이트 (완료)
9. 최종 보고
```

#### 5. Examples (선택 사항)

```markdown
## Examples
### 예시 1
...

### 예시 2
...
```

---

## 🎯 에이전트 추가 가이드

### 1단계: 에이전트 파일 생성

```bash
# .opencode/agent/myagent.md 파일 생성
```

### 2단계: 에이전트 정의

```markdown
# My Agent

## Role
내 에이전트의 역할 설명

## Responsibilities
- 책임 1
- 책임 2

## Must-Read Documents
- AGENTS.md
- PROJECT_SUMMARY.md

## Workflow
1. 단계 1
2. 단계 2

## Examples
### 예시 1
```
사용자: "@myagent 작업 해줘"

@myagent:
✅ 작업 완료
...
```
```

### 3단계: 테스트

```
사용자: "@myagent 테스트"
```

### 4단계: 통합

에이전트 간 통신이 필요한 경우, WORK_IN_PROGRESS.md를 통해 상태를 공유합니다.

---

## 🚨 문제 해결

### 에이전트 토큰이 인식되지 않을 때

#### 해결 1: 파일명 확인

- 파일명이 `.md`로 끝나는지 확인
- 파일명이 올바른지 확인 (공백 없음)

#### 해결 2: 파일 위치 확인

- 파일이 `.opencode/agent/` 폴더에 있는지 확인
- `.opencode` 폴더가 프로젝트 루트에 있는지 확인

#### 해결 3: OpenCode 플러그인 재설치

```bash
cd .opencode
rm -rf node_modules
npm install
```

---

## 📚 참고 문서

- [AGENTS.md](../AGENTS.md)
- [AGENT_ROLES.md](../AGENT_ROLES.md)
- [WORKFLOW_PLANNING.md](../WORKFLOW_PLANNING.md)
- [WORK_IN_PROGRESS.md](../WORK_IN_PROGRESS.md)
