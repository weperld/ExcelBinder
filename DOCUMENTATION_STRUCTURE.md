# 문서 구조 가이드

> ExcelBinder 프로젝트의 문서 구조 및 중복 방지 전략

---

## 🎯 문서 구조 개편 원칙

### 단일 진실 공급원(Single Source of Truth) 전략

| 콘텐츠 | 단일 진실 공급원 | 참조하는 문서 |
|--------|-----------------|---------------|
| **빌드/실행 명령** | QUICK_REFERENCE.md | AGENTS.md (단순 링크) |
| **비동기 패턴** | CODE_STYLE.md | AGENTS.md, AGENT_ROLES.md (단순 링크) |
| **에러 처리 기준** | TECHNICAL_RULES.md | AGENTS.md, AGENT_ROLES.md (단순 링크) |
| **에이전트 역할** | AGENT_ROLES.md | AGENTS.md (단순 링크) |
| **워크플로우** | WORKFLOW_PLANNING.md | AGENTS.md (단순 링크) |
| **프로젝트 컨텍스트** | PROJECT_SUMMARY.md | AGENTS.md (단순 링크) |

---

## 📚 문서 역할 정의

### 1. AGENTS.md (전체 메뉴)

**역할:** 에이전트 시스템의 빠른 개요 및 지시 템플릿

**포함 내용:**
- 📌 에이전트 필독 순서
- 🎯 토큰 절약용 지시 템플릿
- 📚 문서 인덱스 (링크만, 내용 복사 X)
- 🚀 빠른 참조 (링크만, 내용 복사 X)

**복사 금지:** 내용을 복사하지 않고, 상세 문서의 링크만 제공

---

### 2. AGENT_ROLES.md (에이전트 역할 중앙)

**역할:** 모든 에이전트의 상세 역할 및 책임

**포함 내용:**
- 7개 역할 기반 워크플로우 정의
- 각 에이전트의 Role, Responsibilities, Workflow
- 에이전트 간 협업 시나리오

**복사 금지:** AGENTS.md와 내용 중복 X

---

### 3. PROJECT_SUMMARY.md (프로젝트 30초 요약)

**역할:** 프로젝트의 핵심 컨텍스트 빠른 파악

**포함 내용:**
- 🎯 30초 요약
- 🏗️ 핵심 아키텍처
- 📁 핵심 파일 구조
- 🛠️ 기술 스택
- 🚀 빠른 시작 (링크만, 내용 복사 X)

**복사 금지:** 상세 내용은 각 가이드로 위임

---

### 4. WORKFLOW_PLANNING.md (워크플로우 상세)

**역할:** 기획서 처리 및 워크플로우의 단일 소스

**포함 내용:**
- 6단계 개발 파이프라인 (상세)
- 기획서 유형 분석 (상세)
- 에이전트 간 통신 프로토콜
- WorkID 자동 생성 로직
- 에러 상황/롤백 처리

**복사 금지:** 다른 문서와 내용 중복 X

---

### 5. QUICK_REFERENCE.md (빠른 참조)

**역할:** 복사/붙여넣기 가능한 코드 패턴 및 명령어

**포함 내용:**
- 🚀 빌드 및 실행 명령어 (상세)
- 💻 자주 쓰는 코드 패턴 (상세)
- 📝 명명 규칙 (상세)
- 🎯 단축 지시법 (상세)
- 🔍 에러 해결 체크리스트 (상세)

**복사 금지:** 다른 문서와 내용 중복 X

---

### 6. CODE_STYLE.md (코드 작성 표준)

**역할:** C# 코드 작성 표준 (상세)

**포함 내용:**
- 🏗️ 아키텍처 (상세)
- 📝 명명 규칙 (상세)
- ⚡ 비동기 패턴 (상세)
- ⚠️ 에러 처리 (상세)
- 📦 Import 구성 (상세)
- 📖 XML 문서화 (상세)

**복사 금지:** QUICK_REFERENCE.md와 내용 중복 X

---

### 7. TECHNICAL_RULES.md (기술 규칙)

**역할:** 기술적 준수 사항 (상세)

**포함 내용:**
- 데이터 무결성 원칙 (상세)
- 비동기 작업 규칙 (상세)
- 에러 처리 규칙 (상세)
- 보안 규칙 (상세)
- 성능 규칙 (상세)

**복사 금지:** CODE_STYLE.md와 내용 중복 X

---

## 🔗 문서 간 링크 구조

### 빌드/실행 명령

```
QUICK_REFERENCE.md (단일 진실 공급원)
├── 빌드: dotnet build [상세]
├── 실행: dotnet run [상세]
└── CLI 옵션 [상세]

AGENTS.md (링크만)
├── 🚀 빌드 및 실행 → QUICK_REFERENCE.md#빌드-및-실행
```

### 비동기 패턴

```
CODE_STYLE.md (단일 진실 공급원)
└── ⚡ 비동기 패턴 [상세]
    ├── 필수 패턴: try-finally
    ├── Task.Run 사용
    └── 예시

AGENTS.md (링크만)
├── ⚡ 비동기 패턴 필수 → CODE_STYLE.md#비동기-패턴

AGENT_ROLES.md (링크만)
├── - 비동기 패턴 구현 (try-finally) → CODE_STYLE.md#비동기-패턴
```

### 에러 처리

```
TECHNICAL_RULES.md (단일 진실 공급원)
└── 데이터 무결성 우선 [상세]
    ├── 올바른 예시
    ├── 잘못된 예시
    └── 사용자용 에러 메시지

CODE_STYLE.md (링크만)
├── ⚠️ 에러 처리 → TECHNICAL_RULES.md#데이터-무결성-우선

AGENTS.md (링크만)
├── ### 에러 처리 → TECHNICAL_RULES.md#에러-처리

AGENT_ROLES.md (링크만)
├── - 에러 처리 (데이터 무결성 우선) → TECHNICAL_RULES.md#에러-처리

QUICK_REFERENCE.md (링크만)
├── ### 에러 해결 체크리스트 → TECHNICAL_RULES.md#에러-처리
```

---

## 🔄 문서 업데이트 절차

### 1단계: 단일 진실 공급원 선택

| 콘텐츠 | 단일 진실 공급원 |
|--------|-----------------|
| 빌드/실행 명령 | QUICK_REFERENCE.md |
| 비동기 패턴 | CODE_STYLE.md |
| 에러 처리 | TECHNICAL_RULES.md |
| 에이전트 역할 | AGENT_ROLES.md |
| 워크플로우 | WORKFLOW_PLANNING.md |
| 프로젝트 컨텍스트 | PROJECT_SUMMARY.md |

### 2단계: 단일 진실 공급원 업데이트

```
단일 진실 공급원에서 내용을 추가/수정
```

### 3단계: 참조 문서 링크 업데이트

```
참조 문서에서 링크만 업데이트
(내용 복사 X)
```

### 4단계: 중복 제거

```
참조 문서에서 중복 내용 제거
```

---

## 📝 예시: 에러 처리 업데이트

### 1단계: 단일 진실 공급원 선택

TECHNICAL_RULES.md를 에러 처리의 단일 진실 공급원으로 선택

### 2단계: TECHNICAL_RULES.md 업데이트

```markdown
## 에러 처리

### 데이터 무결성 우선

**올바른 예시:**
```csharp
if (schema == null)
    throw new FileNotFoundException("Schema not found");
```

**잘못된 예시:**
```csharp
if (schema == null)
    schema = new SchemaDefinition(); // 데이터 오염 위험!
```
```

### 3단계: 참조 문서 링크 업데이트

**AGENTS.md:**
```markdown
### 에러 처리
→ [TECHNICAL_RULES.md](./.guides/TECHNICAL_RULES.md) 참고
```

**AGENT_ROLES.md:**
```markdown
## Responsibilities
- 에러 처리 → [TECHNICAL_RULES.md](./.guides/TECHNICAL_RULES.md) 참고
```

### 4단계: 중복 제거

AGENTS.md와 AGENT_ROLES.md에서 에러 처리 예시 제거

---

## 🚨 중복 방지 체크리스트

### 문서 작성 전 체크

- [ ] 이 콘텐츠의 단일 진실 공급원을 선택했는가?
- [ ] 단일 진실 공급원에 이미 내용이 있는가?
- [ ] 단일 진실 공급원을 먼저 업데이트했는가?
- [ ] 참조 문서에서 링크만 제공했는가?
- [ ] 중복 내용을 제거했는가?

### 문서 작성 후 체크

- [ ] 다른 문서와 내용 중복이 없는가?
- [ ] 단일 진실 공급원과 내용이 일치하는가?
- [ ] 링크가 올바르게 작동하는가?

---

## 📚 상세 문서 목차

### 핵심 문서 (단일 진실 공급원 포함)

| 문서 | 역할 | 단일 진실 공급원 콘텐츠 |
|------|------|---------------------|
| AGENTS.md | 전체 메뉴 | - |
| AGENT_ROLES.md | 에이전트 역할 중앙 | 에이전트 역할 |
| PROJECT_SUMMARY.md | 프로젝트 30초 요약 | 프로젝트 컨텍스트 |
| WORKFLOW_PLANNING.md | 워크플로우 상세 | 워크플로우 |
| QUICK_REFERENCE.md | 빠른 참조 | 빌드/실행 명령 |
| CODE_STYLE.md | 코드 작성 표준 | 비동기 패턴 |
| TECHNICAL_RULES.md | 기술 규칙 | 에러 처리 |

### 보조 문서

| 문서 | 역할 |
|------|------|
| OPENCODE_INTEGRATION.md | OpenCode 플러그인 연동 |
| .guides/BUILD_GUIDE.md | 빌드 및 개발 가이드 |
| .guides/TEST_GUIDE.md | 테스트 가이드 |

---

## 📖 참고 문서

- [AGENTS.md](./AGENTS.md)
- [AGENT_ROLES.md](./AGENT_ROLES.md)
- [PROJECT_SUMMARY.md](./PROJECT_SUMMARY.md)
- [WORKFLOW_PLANNING.md](./WORKFLOW_PLANNING.md)
- [QUICK_REFERENCE.md](./QUICK_REFERENCE.md)
- [CODE_STYLE.md](./.guides/CODE_STYLE.md)
- [TECHNICAL_RULES.md](./.guides/TECHNICAL_RULES.md)
