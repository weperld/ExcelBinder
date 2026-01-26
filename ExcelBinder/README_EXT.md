# 📝 기능 정의(FDF) 및 템플릿 커스터마이징

ExcelBinder는 외부 설정 파일을 통해 추출 로직과 코드 스타일을 완전히 제어할 수 있는 유연한 구조를 가지고 있습니다.

---

## 1. 기능 정의 파일 (Feature Definition File, FDF)

FDF는 특정 프로젝트의 추출 규칙을 정의하는 JSON 파일입니다. 

### 📑 주요 필드 상세

| 필드 | 설명 | 비고 |
| :--- | :--- | :--- |
| `id` / `name` | 기능의 고유 식별자와 대시보드 표시 이름 | 필수 |
| `category` | 추출 성격 지정 | `StaticData`, `Logic`, `SchemaGen`, `Enum`, `Constants` |
| `paths` | 데이터 및 스크립트의 입출력 경로 설정 | `excelPath`, `schemaPath` 등 |
| `typeMappings` | 엑셀 타입 → 프로젝트 전용 타입 변환 규칙 | 예: `"int": "int32"` |
| `templates` | 사용할 `.liquid` 템플릿 파일의 경로 | 절대 경로 권장 |
| `outputOptions` | 추출 포맷 및 확장자 설정 | `extension`, `supportsBinary` 등 |

> **💡 매칭 규칙**: ExcelBinder는 엑셀의 각 **시트 이름(Sheet Name)**과 동일한 이름의 스키마 파일(`.json`)을 자동으로 찾아 매칭합니다.

---

## 2. 코드 템플릿 (.liquid)

[Scriban](https://github.com/scriban/scriban) 엔진을 사용하여 코드를 생성합니다. 템플릿 내에서 다음 변수들을 활용할 수 있습니다.

> [!TIP]
> **템플릿 작성이 어려우신가요?** [외부 AI 도구 활용 가이드](../README.md#-외부-ai-도구-활용-prompt-template)를 통해 ChatGPT 등에서 바로 사용할 수 있는 프롬프트 템플릿을 확인해 보세요.

### 🧬 공통 변수
- `{{ namespace }}`: 작업 시 지정한 네임스페이스
- `{{ class_name }}`: 생성될 클래스 이름

### 📊 StaticData 전용 변수
- `{{ key }}` / `{{ key_type }}`: 테이블의 기본 키 정보
- `{{ fields }}`: 필드 객체 리스트
  - `field.name`, `field.type`: 필드명 및 매핑된 타입
  - `field.is_list`, `field.is_reference`: 속성 정보
  - `field.read_method`: BinaryReader 전용 읽기 메서드

### 🧮 Logic 전용 변수
- `{{ methods }}`: 엑셀 행별로 생성된 로직 메서드 리스트
  - `method.name`, `method.return_type`: 메서드 정보
  - `method.params_decl`: 매개변수 선언문
  - `method.formula`: 변환된 C# 코드 표현식

---

## 3. 전역 데이터 처리 규칙

ExcelBinder는 모든 엑셀 파일을 처리할 때 다음과 같은 공통 규칙을 적용합니다.

### 📑 행(Row) 처리 규칙
- **첫 번째 행 헤더**: 모든 시트의 첫 번째 행을 데이터의 **헤더(컬럼명)**로 사용합니다. 스키마 생성 및 데이터 매칭의 기준이 됩니다.
- **두 번째 행 이후 데이터**: 실제 데이터는 두 번째 행부터 시작됩니다.

### 🚫 데이터 필터링 기호 (`#`)
데이터 추출 및 스키마 생성 시 불필요한 정보나 임시 데이터를 제외하기 위해 `#` 접두사 규칙을 사용합니다.

#### 1. 컬럼(Header) 무시
- 헤더 이름의 가장 앞에 `#` 기호가 붙어 있는 컬럼은 **메모용**으로 간주됩니다.
- 스키마 생성 시 필드 목록에서 제외되며, 코드 생성 시에도 해당 필드는 생성되지 않습니다.
- 데이터 추출 시에도 해당 컬럼의 데이터는 무시됩니다.
- **[예시]** 헤더가 `Id | #Desc | Value` 구조일 때:
    - `#Desc` 컬럼은 스키마 및 데이터 추출에서 완전히 제외됩니다.

#### 2. 행(Row) 무시
- 헤더가 `#`으로 시작하지 않는 **첫 번째 유효 컬럼(주로 `Id` 컬럼)**의 값이 `#`으로 시작하는 경우, 해당 행 전체는 **비활성 데이터**로 간주됩니다.
- 데이터 추출(Binary, JSON) 시 대상에서 완전히 제외됩니다.
- **[예시]** 헤더가 `#Desc | Id | Value` 구조일 때:
    - `Id` 컬럼이 행 무시 여부를 결정하는 기준이 됩니다.
    - `Id` 컬럼의 값이 `#101`이면 해당 행은 데이터 추출 대상에서 제외됩니다.
    - **주의**: 주석 컬럼인 `#Desc`에 `#`을 적는 것은 행 무시 여부에 영향을 주지 않습니다. 반드시 기준이 되는 유효 컬럼(`Id`)에 표시해야 합니다.

---

## 🛠️ 적용 및 실행 방법

1. **설정 등록**: 작성한 FDF 파일을 앱의 `Settings` 창에서 **Bind** 하거나 전역 `Features` 폴더에 위치시킵니다.
2. **UI 실행**: 대시보드에서 해당 기능을 선택하여 `Export` 또는 `Generate Code`를 수행합니다.
3. **CLI 실행**:
   ```bash
   dotnet run -- --feature [FDF_ID] --export --codegen
   ```
