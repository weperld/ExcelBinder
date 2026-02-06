# 에이전트 구현 가이드

> ExcelBinder 프로젝트에서 새로운 커스텀 에이전트를 추가하는 방법

---

## 🎯 개요

이 가이드는 ExcelBinder 프로젝트에서 새로운 커스텀 에이전트를 추가하는 방법을 설명합니다.

**전제 조건:**
- OpenCode 플러그인 설치 완료
- `.opencode/agent/` 폴더 구조 이해

---

## 📋 에이전트 추가 절차

### 1단계: 에이전트 정의 파일 생성

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

### 3단계: 에이전트 자동 등록

- **파일명 기반 자동 등록**: `myagent.md` → `@myagent` 토큰
- **대소문자 무시**: `MyAgent.md` → `@myagent` 토큰
- **언더스코어 처리**: `my_agent.md` → `@my-agent` 토큰

### 4단계: 테스트

```
사용자: "@myagent 테스트"

@myagent:
✅ 테스트 완료
...
```

### 5단계: 통합 (선택 사항)

에이전트 간 통신이 필요한 경우, WORK_IN_PROGRESS.md를 통해 상태를 공유합니다.

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
5. WORK_IN_PROGRESS.md 업데이트 (완료)
6. 최종 보고
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

## 🎯 에이전트 예시

### 예시 1: 단순 에이전트

```markdown
# Simple Agent

## Role
간단한 작업을 수행하는 단순 에이전트

## Responsibilities
- 간단한 텍스트 편집
- 파일 읽기/쓰기

## Must-Read Documents
- AGENTS.md

## Workflow
1. 사용자 지시 수신
2. 작업 수행
3. 결과 보고

## Examples
### 간단한 작업
```
사용자: "@simple 파일 읽어줘"

@simple:
✅ 파일 읽기 완료

[파일 내용]
... (파일 내용)
```
```

### 예시 2: 복잡한 에이전트

```markdown
# Complex Agent

## Role
복잡한 작업을 수행하는 복합 에이전트 (분석 + 개발 + 테스트)

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
### 전체 작업
```
사용자: "@complex 기능 A 추가해줘"

@complex:
✅ 기능 A 추가 완료

[수행 내용]
- 기획서 분석 완료
- 코드 작성 완료
- 테스트 수행 완료
...
```
```

---

## 🔄 에이전트 간 통신

### WORK_IN_PROGRESS.md를 통한 간접 통신

**통신 방식:**
1. 각 에이전트의 첫 번째 동작: WORK_IN_PROGRESS.md 읽기
2. 각 에이전트의 마지막 동작: WORK_IN_PROGRESS.md 업데이트
3. 다음 에이전트에게 명시적인 전달 불필요

**통신 흐름 예시:**

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
@coordinator (WORK_IN_PROGRESS.md 읽기)
  ↓ (WORK_IN_PROGRESS.md 업데이트: Review 단계 완료)
```

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

- [OPENCODE_INTEGRATION.md](./OPENCODE_INTEGRATION.md)
- [AGENTS.md](./AGENTS.md)
- [AGENT_ROLES.md](./AGENT_ROLES.md)
- [WORKFLOW_PLANNING.md](./WORKFLOW_PLANNING.md)
