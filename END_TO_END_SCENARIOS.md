# 완전한 실시 시나리오 예시

> ExcelBinder 프로젝트에서 7단계 개발 파이프라인을 통한 end-to-end 실시 예시

---

## 📋 시나리오 1: 성공적인 기능 추가

### 사용자 요청

```
사용자: "신규: CSV 데이터 추출 기능 추가해줘"
```

---

### 단계 1: Plan (계획)

#### coordinator

```
사용자: "신규: CSV 데이터 추출 기능 추가해줘"

coordinator:
✅ 작업이 시작되었습니다.

[WorkID]: WIP-20250205-001
[역할 분배]
  → analyst: 기획서 분석
  → developer: 구현
  → reviewer: 리뷰
  → tester: 테스트
  → doc-manager: 문서 관리

analyst를 호출합니다...
```

#### WORK_IN_PROGRESS.md 업데이트

```markdown
### 활성 작업 (진행 중)
| WorkID | 상태 | 유형 | 제목 | 시작일 | 진척도 |
|--------|------|------|------|--------|--------|
| WIP-20250205-001 | ⏸️ 진행 중 | 신규 | CSV 데이터 추출 | 2025-02-05 | 0% |

### WIP-20250205-001: CSV 데이터 추출

#### 📋 계획 요약
- **유형**: 신규
- **시작일**: 2025-02-05

#### ✅ 완료 단계
- [ ] 1. Plan (계획): 기획서 분석, 계획 수립, 사용자 확인
- [ ] 2. Design (설계): 아키텍처 설계, 기술적 검증
- [ ] 3. Code (코딩): 코드 구현, 빌드 확인
- [ ] 4. Test (테스트): 단위 테스트 자동 생성, 기능 테스트, 빌드 테스트
- [ ] 5. Docs (문서화): 각 단계별 문서 업데이트, API 문서 생성
- [ ] 6. QA (품질검사): 코드 품질, 스타일, 아키텍처 준수 검토
- [ ] 7. Review (최종검토): 전체 결과물 종합 검토, 최종 승인

#### 🔗 관련 파일
- (아직 없음)

#### 💬 사용자 메모
- CSV 파일을 JSON으로 변환하는 기능 추가
```

---

#### analyst

```
사용자: "analyst CSV 기능 추가 분석해줘"

analyst:
✅ 기획서 분석 완료

[유형]: 신규 (새로운 기능)
[카테고리]: StaticData (기존 카테고리 확장)

[필요 파일]:
  - Services/Processors/CSVProcessor.cs (신규)
  - ViewModels/CSVExecutionViewModel.cs (신규)
  - Views/ExecutionItems/CSVExecutionView.xaml (신규)
  - Models/FeatureDefinition.cs (수정 필요)

[위험 요소]:
  - 기존 StaticDataProcessor와 호환성: 중간
    → 대응책: 별도 프로세서로 구현
  - UI 통합: 낮음
    → 대응책: 기존 ExecutionView 패턴 준수

[계획]:
  1. CSVProcessor.cs 생성 (데이터 추출 로직)
  2. CSVExecutionViewModel.cs 생성 (MVVM 패턴)
  3. CSVExecutionView.xaml 생성 (XAML UI)
  4. FeatureBuilder에 통합
  5. 빌드 및 테스트

[다음 단계]
사용자 확인 필요:
진행하시겠습니까? (y/n 또는 수정 요청)
```

#### WORK_IN_PROGRESS.md 업데이트

```markdown
### WIP-20250205-001: CSV 데이터 추출

#### ✅ 완료 단계
- [x] 1. Plan (계획): 기획서 분석, 계획 수립, 사용자 확인
- [ ] 2. Code (코딩): 코드 구현, 빌드 확인
...

#### 🚧 진행 상황
[2025-02-05 10:00] 기획서 분석 완료
[2025-02-05 10:15] 계획 수립 완료
```

---

#### 사용자 확인

```
사용자: "y"
```

#### coordinator

```
coordinator:
✅ 사용자 확인 완료

[다음 단계]
developer에게 구현을 위임합니다...
```

#### WORK_IN_PROGRESS.md 업데이트

```markdown
#### ✅ 완료 단계
- [x] 1. Plan (계획): 기획서 분석, 계획 수립, 사용자 확인 ✅
- [ ] 2. Code (코딩): 코드 구현, 빌드 확인
...

#### 🚧 진행 상황
[2025-02-05 10:00] 기획서 분석 완료
[2025-02-05 10:15] 계획 수립 완료
[2025-02-05 10:20] 사용자 확인 완료 (y)
```

---

### 단계 2: Code (코딩)

#### developer

```
사용자: "developer CSV 기능 구현해줘"

developer:
✅ 구현 완료

[생성 파일]:
  - Services/Processors/CSVProcessor.cs (150줄)
    ✅ MVVM 패턴 준수
    ✅ 비동기 패턴 (try-finally)
    ✅ 데이터 무결성 우선
  - ViewModels/CSVExecutionViewModel.cs (80줄)
    ✅ ViewModelBase 상속
    ✅ RelayCommand 사용
  - Views/ExecutionItems/CSVExecutionView.xaml (120줄)
    ✅ 바인딩 구조

[수정 파일]:
  - Models/FeatureDefinition.cs (CSV 카테고리 추가)

[빌드]: 성공 ✅

reviewer에게 리뷰를 요청합니다...
```

#### WORK_IN_PROGRESS.md 업데이트

```markdown
#### ✅ 완료 단계
- [x] 1. Plan (계획): 기획서 분석, 계획 수립, 사용자 확인 ✅
- [x] 2. Code (코딩): 코드 구현, 빌드 확인 ✅
- [ ] 3. Test (테스트): 단위 테스트 자동 생성, 기능 테스트, 빌드 테스트
...

#### 🚧 진행 상황
[2025-02-05 10:00] 기획서 분석 완료
[2025-02-05 10:15] 계획 수립 완료
[2025-02-05 10:20] 사용자 확인 완료 (y)
[2025-02-05 10:30] CSVProcessor.cs 생성 완료 (150줄)
[2025-02-05 10:45] CSVExecutionViewModel.cs 생성 완료 (80줄)
[2025-02-05 11:00] CSVExecutionView.xaml 생성 완료 (120줄)
[2025-02-05 11:15] 빌드 성공 ✅
```

---

### 단계 3: QA (품질검사)

#### reviewer

```
사용자: "reviewer 코드 리뷰해줘"

reviewer:
✅ 코드 리뷰 완료

[분석 결과]:
  - CSVProcessor.cs: 데이터 무결성 준수 ✅
  - 비동기 패턴: try-finally 사용 ✅
  - 명명 규칙: PascalCase, _camelCase 준수 ✅
  - MVVM 패턴: 준수 ✅

[문제점]: 없음

[개선 사항]: 없음

[결과]: 리뷰 통과 ✅

tester에게 테스트를 요청합니다...
```

#### WORK_IN_PROGRESS.md 업데이트

```markdown
#### ✅ 완료 단계
- [x] 1. Plan (계획): 기획서 분석, 계획 수립, 사용자 확인 ✅
- [x] 2. Code (코딩): 코드 구현, 빌드 확인 ✅
- [ ] 3. Test (테스트): 단위 테스트 자동 생성, 기능 테스트, 빌드 테스트
- [ ] 4. Docs (문서화): 각 단계별 문서 업데이트, API 문서 생성
- [x] 5. QA (품질검사): 코드 품질, 스타일, 아키텍처 준수 검토 ✅
- [ ] 6. Review (최종검토): 전체 결과물 종합 검토, 최종 승인

#### 🚧 진행 상황
...
[2025-02-05 11:30] 코드 리뷰 완료 ✅
```

---

### 단계 4: Test (테스트)

#### tester

```
사용자: "tester 테스트해줘"

tester:
✅ 테스트 완료

[결과]:
  - 빌드: 성공 ✅
  - 기능 테스트: 통과 ✅
  - 단위 테스트: 테스트 인프라 없음

[테스트 시나리오]:
  1. CSV 기능 선택
  2. Export 버튼 클릭
  3. 파일 경로 선택
  4. 데이터 추출 확인
  5. JSON 출력 확인

[버그]:
  - 발견된 버그: 없음

doc-manager에게 문서화를 요청합니다...
```

#### WORK_IN_PROGRESS.md 업데이트

```markdown
#### ✅ 완료 단계
- [x] 1. Plan (계획): 기획서 분석, 계획 수립, 사용자 확인 ✅
- [x] 2. Code (코딩): 코드 구현, 빌드 확인 ✅
- [x] 3. Test (테스트): 단위 테스트 자동 생성, 기능 테스트, 빌드 테스트 ✅
- [ ] 4. Docs (문서화): 각 단계별 문서 업데이트, API 문서 생성
- [x] 5. QA (품질검사): 코드 품질, 스타일, 아키텍처 준수 검토 ✅
- [ ] 6. Review (최종검토): 전체 결과물 종합 검토, 최종 승인

#### 🚧 진행 상황
...
[2025-02-05 11:45] 기능 테스트 완료 ✅
```

---

### 단계 5: Docs (문서화)

#### doc-manager

```
사용자: "doc-manager 문서 업데이트해줘"

doc-manager:
✅ 문서 업데이트 완료

[업데이트 내용]:
  - WORK_IN_PROGRESS.md: 각 단계 체크박스 체크
  - 진행 상황: 모든 단계 완료
  - 진척도: 0% → 100%

coordinator에게 보고합니다...
```

#### WORK_IN_PROGRESS.md 업데이트

```markdown
### 활성 작업 (진행 중)
| WorkID | 상태 | 유형 | 제목 | 시작일 | 진척도 |
|--------|------|------|------|--------|--------|
| *(WIP-20250205-001 제거됨)* | - | - | - | - | - |

### 완료 작업 (히스토리)
| WorkID | 완료일 | 유형 | 제목 | 소요시간 |
|--------|--------|------|------|----------|
| WIP-20250205-001 | 2025-02-05 | 신규 | CSV 데이터 추출 | 2.5h |

### 완료된 작업 상세

#### WIP-20250205-001: CSV 데이터 추출

**완료일:** 2025-02-05 14:00
**소요 시간:** 2.5시간

#### ✅ 완료 단계
- [x] 1. Plan (계획): 기획서 분석, 계획 수립, 사용자 확인 ✅
- [x] 2. Code (코딩): 코드 구현, 빌드 확인 ✅
- [x] 3. Test (테스트): 단위 테스트 자동 생성, 기능 테스트, 빌드 테스트 ✅
- [x] 4. Docs (문서화): 각 단계별 문서 업데이트, API 문서 생성 ✅
- [x] 5. QA (품질검사): 코드 품질, 스타일, 아키텍처 준수 검토 ✅
- [x] 6. Review (최종검토): 전체 결과물 종합 검토, 최종 승인 ✅

#### 🔗 관련 파일
- Services/Processors/CSVProcessor.cs
- ViewModels/CSVExecutionViewModel.cs
- Views/ExecutionItems/CSVExecutionView.xaml
- Models/FeatureDefinition.cs

#### 💬 사용자 메모
- CSV 파일을 JSON으로 변환하는 기능 추가

#### 🚧 진행 상황
[2025-02-05 10:00] 기획서 분석 완료
[2025-02-05 10:15] 계획 수립 완료
[2025-02-05 10:20] 사용자 확인 완료 (y)
[2025-02-05 10:30] CSVProcessor.cs 생성 완료 (150줄)
[2025-02-05 10:45] CSVExecutionViewModel.cs 생성 완료 (80줄)
[2025-02-05 11:00] CSVExecutionView.xaml 생성 완료 (120줄)
[2025-02-05 11:15] 빌드 성공 ✅
[2025-02-05 11:30] 코드 리뷰 완료 ✅
[2025-02-05 11:45] 기능 테스트 완료 ✅
[2025-02-05 12:00] 문서 업데이트 완료 ✅
[2025-02-05 14:00] 전체 완료 ✅
```

---

### 단계 6: Review (최종검토)

#### coordinator

```
사용자: "coordinator 최종 검토해줘"

coordinator:
✅ 최종 검토 완료

[전체 결과물 종합]
- WorkID: WIP-20250205-001
- 유형: 신규 (CSV 데이터 추출)
- 소요 시간: 2.5시간

[수정/생성 파일]:
  - Services/Processors/CSVProcessor.cs (신규, 150줄)
  - ViewModels/CSVExecutionViewModel.cs (신규, 80줄)
  - Views/ExecutionItems/CSVExecutionView.xaml (신규, 120줄)
  - Models/FeatureDefinition.cs (수정, CSV 카테고리 추가)

[테스트 결과]:
  - 빌드: 성공 ✅
  - 기능 테스트: 통과 ✅

[품질 검토]:
  - 코드 스타일: 준수 ✅
  - 아키텍처: 준수 ✅
  - 데이터 무결성: 준수 ✅

[최종 승인]: ✅ 승인 완료

[다음 단계]
- 커밋: [feat] CSV 데이터 추출 기능 추가
- WORK_HISTORY.json 업데이트
- 보고서 생성 (선택 사항)
```

---

### 사용자 완료

```
사용자: "완료: WIP-20250205-001"
```

#### coordinator

```
coordinator:
✅ WIP-20250205-001 작업이 완료되었습니다.

[완료 요약]
- 완료일: 2025-02-05 14:00
- 소요 시간: 2.5시간
- 커밋: [feat] CSV 데이터 추출 기능 추가

[다음 단계]
- WORK_HISTORY.json 업데이트 완료 ✅
- 보고서 생성 (필요 시)
```

---

## 📋 시나리오 2: 버그 발견 후 수정

### 사용자 요청

```
사용자: "🚨 ExportService.cs:45 NullReferenceException 발생"
```

---

### 단계 1: Plan (긴급 대응)

#### coordinator

```
사용자: "🚨 ExportService.cs:45 NullReferenceException 발생"

coordinator:
✅ 즉시 대응 모드 시작

[WorkID]: WIP-20250205-999 (긴급)
[긴급 처리]
  → analyst: 오류 분석
  → developer: 즉시 수정
  → tester: 수정 검증
  → reviewer: 수정 검토
  → doc-manager: 문서화

analyst를 호출합니다...
```

---

### 단계 2: Error Analysis (오류 분석)

#### analyst

```
analyst:
✅ 오류 분석 완료

[에러]:
- NullReferenceException: Object reference not set to an instance of an object.
- 위치: ExportService.cs:45

[원인]:
- ExportService.cs:45에서 schema 객체가 null인 상태로 접근

[위험 요소]:
- 데이터 무결성: 높음 (null 데이터 처리 가능성)
- 호환성: 낮음 (단순 null 체크 추가)

[대응책]:
- ExportService.cs:45에 null 체크 추가
- null 경우 명시적인 예외 발생

[다음 단계]
developer에게 수정을 요청합니다...
```

---

### 단계 3: Code Fix (코드 수정)

#### developer

```
사용자: "developer 버그 수정해줘"

developer:
✅ 버그 수정 완료

[수정 파일]:
  - Services/ExportService.cs
    - 45번 라인: null 체크 추가
    - ✅ 데이터 무결성 준수
    - ✅ 예외 처리

[수정 내용]:
```csharp
// 수정 전:
var data = await LoadData(schema);  // schema가 null일 수 있음

// 수정 후:
if (schema == null)
    throw new FileNotFoundException($"Schema not found: {schemaFile}");
var data = await LoadData(schema);
```

[빌드]: 성공 ✅

reviewer에게 수정 리뷰를 요청합니다...
```

---

### 단계 4: QA (수정 리뷰)

#### reviewer

```
reviewer:
✅ 수정 리뷰 완료

[분석 결과]:
  - ExportService.cs:45: null 체크 추가 ✅
  - 데이터 무결성 준수 ✅
  - 예외 처리 적절 ✅

[문제점]: 없음

[개선 사항]: 없음

[결과]: 리뷰 통과 ✅

tester에게 수정 검증을 요청합니다...
```

---

### 단계 5: Test (수정 검증)

#### tester

```
tester:
✅ 수정 검증 완료

[결과]:
  - 빌드: 성공 ✅
  - 기능 테스트: 통과 ✅

[버그 재현 단계]:
  1. 빈 CSV 파일 로드
  2. Export 버튼 클릭
  3. FileNotFoundException 발생 (예상 동작) ✅

[버그]:
  - 버그 수정 확인: 완료 ✅
  - 회귀 버그: 없음 ✅

doc-manager에게 문서화를 요청합니다...
```

---

### 단계 6: Docs (문서화)

#### doc-manager

```
doc-manager:
✅ 긴급 작업 완료

[업데이트 내용]:
  - WORK_IN_PROGRESS.md: 긴급 작업 완료
  - 완료 작업으로 이동 ✅

coordinator에게 보고합니다...
```

---

### 단계 7: Review (최종 검토)

#### coordinator

```
coordinator:
✅ 긴급 작업 완료

[전체 결과물 종합]
- WorkID: WIP-20250205-999
- 유형: 긴급 버그 수정
- 소요 시간: 1.5시간

[수정 파일]:
  - Services/ExportService.cs (수정: null 체크 추가)

[테스트 결과]:
  - 빌드: 성공 ✅
  - 기능 테스트: 통과 ✅
  - 버그 수정: 완료 ✅
  - 회귀 버그: 없음 ✅

[최종 승인]: ✅ 승인 완료

[다음 단계]
- 커밋: [fix] ExportService null 체크 추가
- WORK_HISTORY.json 업데이트
```

---

## 📋 시나리오 3: 작업 중단 후 재개

### 사용자 요청 (새 대화)

```
사용자: "재개: WIP-20250202-001"
```

---

### coordinator

```
coordinator:
✅ 작업이 재개되었습니다.

[WorkID]: WIP-20250202-001
[유형]: 신규
[제목]: LogicProcessor 기능 추가

[중단 시점]: 구현 단계2 완료
[다음 작업]: 빌드 테스트

[완료 단계]
- [x] 1. Plan (계획): 기획서 분석, 계획 수립, 사용자 확인
- [x] 2. Code (코딩): 코드 구현, 빌드 확인
- [ ] 3. Test (테스트): 단위 테스트 자동 생성, 기능 테스트, 빌드 테스트
- [ ] 4. Docs (문서화): 각 단계별 문서 업데이트, API 문서 생성
- [ ] 5. QA (품질검사): 코드 품질, 스타일, 아키텍처 준수 검토
- [ ] 6. Review (최종검토): 전체 결과물 종합 검토, 최종 승인

[진척도]: 40%

developer에게 구현 계속을 요청합니다...
```

---

## 📋 시나리오 4: 작업 취소

### 사용자 요청

```
사용자: "취소: WIP-20250203-001 우선순위 조정으로 인해"
```

---

### coordinator

```
coordinator:
⚠️ WIP-20250203-001 작업이 취소되었습니다.

[취소 정보]
- 사유: 우선순위 조정으로 인해
- 진척도: 구현 단계2 완료
- 소요 시간: 3.0시간

[정보 보존]
- 상세 정보는 취소 작업 히스토리에 보존됨
```

---

## 📚 관련 문서

- [AGENTS.md](./AGENTS.md)
- [AGENT_ROLES.md](./AGENT_ROLES.md)
- [WORKFLOW_PLANNING.md](./WORKFLOW_PLANNING.md)
- [WORK_IN_PROGRESS.md](./WORK_IN_PROGRESS.md)
- [DOCUMENTATION_STRUCTURE.md](./DOCUMENTATION_STRUCTURE.md)
