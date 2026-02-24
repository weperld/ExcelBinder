# Feature Backlog - ExcelBinder

> 작성일: 2026-02-18
> 현재 버전: 1.2.3
> 상태: 기획 단계 (구현 가능성 분석 완료)

---

## 1. 릴리즈 버전 체크 및 새 버전 다운로드 알림

### 구현 가능성: HIGH

### 현황 분석
- 현재 버전 정보: `ExcelBinder.csproj`에 `<Version>1.2.3</Version>`으로 관리 중
- 기존 업데이트 관련 코드: 없음
- `AppSettings`에 업데이트 관련 설정 없음

### 방식
- **GitHub Releases** 기반 버전 체크
- 새 버전 발견 시 GitHub 릴리즈 페이지로 바로 연결하는 **버튼** 표시
- 인앱 다운로드/자동 설치 없음 (브라우저에서 직접 다운로드)

### 사전 결정 필요 사항
- [x] GitHub 저장소 URL 확정 → `weperld/ExcelBinder`
- [x] 업데이트 확인 시점 (앱 시작 시 / 수동 버튼 / 둘 다) → 둘 다
- [x] 프리릴리즈 버전 알림 포함 여부 → 제외 (정식 릴리즈만)

### 대략적 계획

#### Phase 1: 버전 정보 인프라
- GitHub Releases API (`https://api.github.com/repos/{owner}/{repo}/releases/latest`) 사용
- `Models/VersionInfo.cs` 모델 추가 (LatestVersion, HtmlUrl, ReleaseNotes 등)

#### Phase 2: 버전 체크 서비스
- `Services/UpdateCheckService.cs` 추가
  - HttpClient로 GitHub Releases API 호출
  - 응답에서 `tag_name`(버전), `html_url`(릴리즈 페이지 URL) 추출
  - 현재 앱 버전과 비교 (SemVer 비교)
  - 네트워크 오류 시 무시 (체크 실패가 앱 사용을 막지 않아야 함)

#### Phase 3: UI 알림
- 앱 시작 시 백그라운드 버전 체크
- 새 버전 발견 시 Dashboard에 알림 배너 + **"새 버전 다운로드" 버튼** 표시
- 버튼 클릭 시 `Process.Start()`로 GitHub 릴리즈 URL을 기본 브라우저에서 열기
- Settings에 "시작 시 업데이트 확인" 토글 추가 (`AppSettings` 확장)

### 영향 범위
| 파일 | 변경 유형 |
|------|----------|
| `Models/VersionInfo.cs` | **신규** |
| `Services/UpdateCheckService.cs` | **신규** |
| `Models/AppSettings.cs` | 수정 (업데이트 확인 설정 추가) |
| `Models/ProjectConstants.cs` | 수정 (업데이트 관련 상수 추가) |
| `Views/DashboardView.xaml` | 수정 (알림 배너 추가) |
| `ViewModels/MainViewModel.cs` | 수정 (시작 시 버전 체크 호출) |
| `ExcelBinder.csproj` | 수정 (필요 시 HttpClient 패키지 참조) |

### 리스크
- .NET 10의 `HttpClient`는 기본 제공이므로 추가 패키지 불필요
- 방화벽/프록시 환경에서 체크 실패 가능 -> 타임아웃 및 graceful 처리 필수
- GitHub API 사용 시 Rate Limit 고려 필요

---

## 2. 스키마 생성 시 헤더 체크 박스 (선택적 포함/제외)

### 구현 가능성: HIGH

### 현황 분석
- 스키마 편집기 존재: `SchemaEditorView.xaml` + `SchemaEditorViewModel.cs`
- 현재 필드 모델 `SchemaFieldViewModel`: HeaderName, SelectedType, ReferenceType, Count
- 스키마 저장 모델 `SchemaDefinition`: ClassName, Key, Fields (Dictionary<string, string>)
- Key 컬럼 개념 존재: `SchemaEditorItemViewModel.SelectedKey`
- 스키마 기반 처리: `StaticDataProcessor`, `ExportService`, `CodeGeneratorService` 등에서 `SchemaDefinition.Fields`를 순회

### 핵심 규칙
1. 체크 해제된 헤더 -> 데이터 추출 및 코드 생성에서 제외
2. Key 컬럼으로 지정된 헤더 -> 체크 여부 무관 무조건 포함 (체크박스 비활성화)
3. 기존 스키마 파일과의 하위 호환성 유지

### 사전 결정 필요 사항
- [ ] 체크 해제된 필드의 스키마 JSON 저장 방식 (별도 excludedFields 배열 vs 필드에 메타 플래그)
- [ ] 기존 스키마 파일 로드 시 기본값 (전부 체크됨으로 처리 -> 하위 호환)
- [ ] Key 컬럼 체크박스의 시각적 표현 (비활성화 + 체크 / 비활성화 + 잠금 아이콘 등)

### 대략적 계획

#### Phase 1: 모델 확장
- `SchemaFieldViewModel`에 `IsIncluded` (bool, 기본값 true) 속성 추가
- `SchemaFieldViewModel`에 `IsKeyColumn` (bool, 읽기 전용) 속성 추가
- `SchemaDefinition`에 `ExcludedFields` (List<string>) 추가, 또는 Fields 값에 메타 정보 포함
- 기존 스키마 로드 시 ExcludedFields 없으면 전부 포함으로 처리 (하위 호환)

#### Phase 2: 스키마 편집기 UI 수정
- `SchemaEditorView.xaml`의 필드 목록에 체크박스 열 추가
- Key 컬럼과 동일한 HeaderName의 필드 -> IsIncluded=true 고정, 체크박스 비활성화
- SelectedKey 변경 시 이전 Key의 체크박스 활성화, 새 Key의 체크박스 비활성화
- "전체 선택/해제" 토글 추가 고려

#### Phase 3: 처리 파이프라인 반영
- `SchemaEditorItemViewModel.Save()`에서 ExcludedFields 저장 로직 추가
- `StaticDataProcessor`에서 스키마 로드 후 ExcludedFields 필터링
- `CodeGeneratorService`에서 코드 생성 시 제외 필드 스킵
- `ExportService`에서 데이터 내보내기 시 제외 필드 스킵

### 영향 범위
| 파일 | 변경 유형 |
|------|----------|
| `ViewModels/SchemaEditorViewModel.cs` | 수정 (IsIncluded, IsKeyColumn 추가) |
| `Models/SchemaDefinition.cs` | 수정 (ExcludedFields 추가) |
| `Views/SchemaEditorView.xaml` | 수정 (체크박스 열 추가) |
| `Services/Processors/StaticDataProcessor.cs` | 수정 (제외 필드 필터링) |
| `Services/CodeGeneratorService.cs` | 수정 (제외 필드 스킵) |
| `Services/ExportService.cs` | 수정 (제외 필드 스킵) |
| `Services/SchemaGeneratorService.cs` | 수정 (자동 생성 시 전부 포함 기본값) |

### 기능 충돌 가능성
- **TypeMappings**: 제외된 필드가 TypeMappings에 존재할 경우 -> 무시하면 됨 (영향 없음)
- **Reference 타입**: 제외된 필드가 다른 필드의 Reference로 참조될 경우 -> 경고 표시 필요
- **List 타입 (동일 헤더명 복수)**: Count > 1인 필드 제외 시 동일 이름 전체 제외 -> 명확한 UX 필요
- **Binary Export**: 바이너리 포맷의 필드 순서가 변경될 수 있음 -> 기존 바이너리와 호환성 깨질 수 있음 (주의 필요)

---

## 3. 프로젝트 별 Feature 그룹화

### 구현 가능성: HIGH

### 현황 분석
- 현재 대시보드: `DashboardView.xaml`에서 `ListBox` + `WrapPanel`로 모든 Feature를 flat하게 표시
- `FeatureDefinition`에 `Category` 속성 존재 (StaticData, Logic 등 기능 카테고리)
- 프로젝트 그룹 개념 없음
- `MainViewModel.Features`는 `ObservableCollection<FeatureDefinition>`으로 flat 관리
- WPF의 `CollectionViewSource` + `GroupDescriptions`로 그룹화 가능

### 사전 결정 필요 사항
- [ ] 그룹 단위: "프로젝트"라는 새 개념 도입 vs FeatureDefinition에 Group 속성 추가
- [ ] 그룹 관리 방식: 별도 프로젝트 설정 파일 vs FeatureDefinition 내 속성으로 관리
- [ ] 그룹 미지정 Feature 처리: "미분류" 그룹에 표시 vs 그룹 바깥에 표시
- [ ] 그룹 UI 형태: Expander(접기/펼치기) vs TabControl vs TreeView 사이드바
- [ ] 그룹 간 Feature 이동/복사 허용 여부

### 대략적 계획

#### Phase 1: 모델 확장
- `FeatureDefinition`에 `Group` (string, 기본값 "") 속성 추가
- 기존 Feature JSON 로드 시 Group 없으면 빈 문자열 -> 하위 호환
- 또는 별도 `FeatureGroup` 모델 도입 (GroupName, Description, Color 등)

#### Phase 2: 그룹 관리 기능
- Feature Builder에서 그룹 지정 UI 추가 (ComboBox/TextBox)
- 그룹 생성/수정/삭제 관리 (Settings 또는 별도 관리 화면)
- 그룹 목록을 AppSettings 또는 별도 파일로 저장

#### Phase 3: 대시보드 UI 변경
- `DashboardView.xaml`에 `CollectionViewSource` 적용
  - `GroupDescriptions`로 Group 속성 기준 그룹화
- 그룹별 `Expander` 또는 섹션 헤더로 시각적 구분
- 그룹 필터링/검색 기능 추가 고려
- "모든 Feature 보기" vs "프로젝트별 보기" 토글

### 영향 범위
| 파일 | 변경 유형 |
|------|----------|
| `Models/FeatureDefinition.cs` | 수정 (Group 속성 추가) |
| `Views/DashboardView.xaml` | 수정 (그룹화 UI) |
| `ViewModels/MainViewModel.cs` | 수정 (그룹화 로직, 필터링) |
| `Views/FeatureBuilderView.xaml` | 수정 (그룹 지정 UI) |
| `ViewModels/FeatureBuilderViewModel.cs` | 수정 (그룹 선택 로직) |
| `Models/AppSettings.cs` | 수정 (그룹 목록 또는 마지막 선택 그룹 저장) |

### 리스크
- 기존 Feature JSON 파일에 Group 속성이 없으므로 마이그레이션 고려 필요
- 그룹명 변경 시 해당 그룹의 모든 Feature를 업데이트해야 함
- WPF `CollectionViewSource`의 그룹화는 대량 아이템에서 성능 이슈 가능 (보통은 문제 없음)

---

## 4. 업데이트 알림에 릴리즈 노트 표시

### 구현 가능성: HIGH

### 현황 분석
- `VersionInfo.Body`에 GitHub 릴리즈 노트가 이미 저장됨
- 현재 토스트/팝업에는 버전 번호만 표시
- GitHub API `/releases` 엔드포인트로 전체 릴리즈 목록 조회 가능 (페이지당 30건)

### 사전 결정 필요 사항
- [x] 표시 범위: 현재 버전~최신 버전 사이 모든 릴리즈 노트 표시 (B 방식)
- [ ] 릴리즈 노트 포맷: Markdown 원본 그대로 vs 간소화 텍스트

### 대략적 계획

#### Phase 1: 서비스 확장
- `UpdateCheckService`에 `/releases` API 호출 메서드 추가
- 현재 버전 < release 버전 <= 최신 버전 범위 필터링
- `List<VersionInfo>` 반환

#### Phase 2: UI 반영
- 토스트 배너에 릴리즈 노트 요약 영역 추가 (스크롤 가능)
- Settings 업데이트 확인 팝업에도 릴리즈 노트 표시
- 버전별 섹션 구분 (v2.3.0, v2.2.0 등)

### 영향 범위
| 파일 | 변경 유형 |
|------|----------|
| `Services/UpdateCheckService.cs` | 수정 (전체 릴리즈 조회 메서드 추가) |
| `Models/ProjectConstants.cs` | 수정 (API URL 상수 추가) |
| `ViewModels/MainViewModel.cs` | 수정 (릴리즈 노트 목록 바인딩) |
| `MainWindow.xaml` | 수정 (토스트에 릴리즈 노트 영역) |
| `Views/SettingsView.xaml` | 수정 (팝업에 릴리즈 노트 영역) |

### 난이도: 소~중

---

## 5. 인앱 업데이트 다운로드 (고려 대상)

### 구현 가능성: HIGH

### 현황
- 현재: 새 버전 발견 시 GitHub 릴리즈 페이지를 브라우저로 열어 수동 다운로드

### 사전 결정 필요 사항
- [ ] 아래 3가지 방식 중 선택

### 방식 비교

#### 방식 A: 단순 다운로드 (GitHub asset → 로컬 저장) — 소규모
- GitHub Releases API에서 asset URL 가져오기 + HttpClient로 다운로드 + 진행률 표시
- 사용자가 수동으로 압축 풀고 교체
- 약 2~3개 파일 추가/수정

#### 방식 B: 자동 업데이트 (다운로드 → 교체 → 재시작) — 중~대규모
- 실행 중인 exe는 자체 교체가 불가능 → 별도 업데이터 프로세스 또는 배치 스크립트 필요
- 앱 종료 → 파일 교체 → 재시작 흐름 구현
- 파일 잠금, 권한(UAC), 실패 시 롤백 처리
- 테스트 난이도 높음

#### 방식 C: 프레임워크 활용 (Velopack 등) — 중규모
- .NET용 자동 업데이트 라이브러리 (Velopack, Squirrel 등)
- 설치/업데이트/롤백을 프레임워크가 처리
- 대신 배포 방식 자체가 변경됨 (설치형으로 전환 필요)

---

## 우선순위 제안

| 순위 | 기능 | 난이도 | 영향 범위 | 이유 |
|------|------|--------|----------|------|
| 1 | 헤더 체크 박스 | 중 | 중 | 핵심 워크플로우 개선, 기존 구조에 자연스럽게 확장 가능 |
| 2 | Feature 그룹화 | 중 | 중 | Feature 수 증가 시 필수, UX 개선 효과 큼 |
| 3 | 버전 체크/알림 | 중 | 소 | 배포 인프라 결정 후 진행 가능, 독립적 기능 |

> 우선순위는 제안입니다. 실제 순서는 프로젝트 상황에 따라 조정해 주세요.

---

## 할 일 목록 (TODO)

### 기능 1: 릴리즈 버전 체크 ✅ 구현 완료 (2026-02-24)
- [x] GitHub 저장소 URL 확정 → `weperld/ExcelBinder`
- [x] 업데이트 확인 시점 결정 → 둘 다 (시작 시 자동 + Settings 수동 버튼)
- [x] `VersionInfo` 모델 설계 → `Models/VersionInfo.cs`
- [x] `UpdateCheckService` 구현 → `Services/UpdateCheckService.cs`
- [x] `AppSettings` 확장 → `CheckForUpdatesOnStartup` 속성 추가
- [x] Dashboard 알림 배너 + GitHub 릴리즈 링크 버튼 UI 추가
- [x] MainViewModel 시작 시 체크 로직 연동
- [x] 네트워크 오류 처리 (타임아웃 10초, graceful 실패)

### 기능 2: 스키마 헤더 체크 박스
- [ ] 제외 필드 저장 방식 결정 (ExcludedFields vs 필드 메타 플래그)
- [ ] Key 컬럼 체크박스 비활성화 방식 결정
- [ ] `SchemaDefinition` 모델 확장
- [ ] `SchemaFieldViewModel` 확장 (IsIncluded, IsKeyColumn)
- [ ] `SchemaEditorView.xaml` 체크박스 열 추가
- [ ] Key 컬럼 변경 시 체크박스 상태 연동 로직
- [ ] `SchemaEditorItemViewModel.Save()` 수정
- [ ] `StaticDataProcessor` 필터링 반영
- [ ] `CodeGeneratorService` 제외 필드 스킵
- [ ] `ExportService` 제외 필드 스킵
- [ ] Reference 충돌 경고 로직 추가
- [ ] 기존 스키마 하위 호환성 테스트

### 기능 3: 프로젝트 별 Feature 그룹화
- [ ] 그룹 모델 방식 결정 (속성 vs 별도 모델)
- [ ] 그룹 UI 형태 결정 (Expander / Tab / TreeView)
- [ ] `FeatureDefinition` Group 속성 추가
- [ ] Feature Builder 그룹 지정 UI 추가
- [ ] 그룹 관리 기능 구현
- [ ] DashboardView 그룹화 UI 적용
- [ ] MainViewModel 그룹 필터링 로직
- [ ] 기존 Feature JSON 하위 호환성 확인
- [ ] 그룹 미지정 Feature 처리 확인
