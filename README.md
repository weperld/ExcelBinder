# 🚀 ExcelBinder
> **Universal Data Extraction & Code Generation Engine**

[![.NET](https://img.shields.io/badge/.NET-10.0-512bd4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-0078d7?logo=windows)](https://www.microsoft.com/windows)

ExcelBinder는 특정 프로젝트나 라이브러리에 종속되지 않고, 사용자가 정의한 규칙에 따라 엑셀 데이터를 추출하고 코드를 생성하는 **범용 데이터 파이프라인 엔진**입니다.

---

## ✨ 핵심 아키텍처: 엔진과 정의의 분리
ExcelBinder는 앱 자체에 고정된 로직을 담지 않습니다. 모든 동작은 외부의 **기능 정의 파일(FDF)**과 **코드 템플릿(.liquid)**에 의해 결정됩니다.

- **🛠️ Engine (App)**: FDF를 로드하고, 파일을 스캔하며, 템플릿 엔진을 실행하는 코어 유닛.
- **📄 Feature Definition (FDF)**: 프로젝트별 추출 규칙, 경로, 타입 매핑 등을 정의하는 JSON 설정.
- **🎨 Templates (.liquid)**: [Scriban](https://github.com/scriban/scriban) 문법을 사용하여 생성될 코드의 모양을 자유롭게 설계.

## 🌟 주요 기능
- **📂 시트 기반 추출**: 엑셀 파일명이 아닌 **시트명(Sheet Name)** 기준 스키마 매칭. 하나의 파일에서 여러 시트 관리 가능.
- **🎯 카테고리별 특화 로직**:
  - `StaticData`: 바이너리/JSON 데이터 추출 및 모델 코드 생성.
  - `Logic`: 엑셀 수식을 분석하여 실행 가능한 C# 클래스 생성.
  - `SchemaGen`: 헤더 분석을 통한 JSON 스키마 자동 생성.
- **🏗️ Feature Builder**: 복잡한 JSON 설정을 UI에서 시각적으로 편집.
- **🔄 유연한 타입 매핑**: 엑셀 타입을 프로젝트 전용 타입(예: `int` → `System.Int32`)으로 자유롭게 매핑.
- **🚫 데이터 필터링 규칙**: `#` 접두사를 사용하여 불필요한 컬럼이나 행을 추출 대상에서 제외.
- **🤖 AI Assistant**: OpenAI/Claude를 연동하여 Scriban 템플릿을 대화형으로 생성.
- **🤖 외부 AI 활용 (Prompt Template)**: API 키 없이도 외부 AI(ChatGPT 등)를 통해 최적의 템플릿을 생성할 수 있는 프롬프트 가이드 제공.
- **🤖 CLI 지원**: CI/CD 환경 자동화를 위한 커맨드 라인 인터페이스 제공.

## 🚀 빠른 시작
1. **FDF 바인딩**: `Settings`에서 기존 FDF를 연결하거나 `Create New Feature`로 새로 만듭니다.
2. **기능 선택**: 대시보드에 나타난 기능 카드를 클릭합니다.
3. **실행**: 대상 엑셀 파일을 선택하고 `Export` 혹은 `Generate Code`를 실행합니다.

## 📖 사용 설명서 (Usage Guide)

### 1. 기능 정의 (FDF) 및 프로젝트 설정
ExcelBinder를 사용하기 위해서는 프로젝트의 규칙을 담은 **FDF(Feature Definition File)**가 필요합니다.
- **등록**: `Settings` 윈도우에서 `Bind External Feature`를 클릭하여 기존 `.json` 설정을 불러옵니다.
- **생성**: `Create New Feature`를 통해 입출력 경로, 템플릿 경로, 타입 매핑 등을 직접 설정할 수 있습니다.

### 2. 인터랙티브 스키마 에디터
데이터 추출 전, 엑셀의 헤더와 코드의 필드를 매칭하는 단계입니다.
- **자동 인지**: 동일한 이름의 컬럼이 여러 개 있을 경우 (예: `Skill`, `Skill`, `Skill`), 에디터는 이를 자동으로 `List<T>` 타입으로 그룹화합니다.
- **참조(Reference) 설정**: 필드 타입에 `ref:TargetSheet` 형식을 지정하면, 코드 생성 시 해당 시트의 데이터를 자동으로 찾아주는 참조 프로퍼티가 생성됩니다.

### 3. 데이터 추출 (Export) 및 코드 생성 (CodeGen)
- **작업 선택**: 대시보드에서 카드를 클릭하여 진입한 뒤, 왼쪽의 파일 목록에서 작업할 엑셀을 선택합니다.
- **Export**: FDF 설정에 따라 바이너리(`.bytes`) 또는 `.json` 파일로 데이터를 추출합니다.
- **Generate Code**: `.liquid` 템플릿을 기반으로 C# 모델 클래스를 생성합니다.

### 4. 🤖 외부 AI 도구 활용 (Prompt Template)
AI Assistant용 API 키가 없더라도, ChatGPT나 Claude와 같은 외부 AI 도구를 활용하여 최적의 템플릿을 얻을 수 있습니다. 아래 템플릿을 복사하여 사용하세요.

#### 📝 범용 프롬프트 템플릿
> 아래 내용을 복사하여 AI에게 전달하세요. `[ ]`로 표시된 부분에 본인의 스키마와 요구사항을 넣으면 됩니다.

```text
너는 Scriban(.liquid) 템플릿 엔진 전문가이자 숙련된 소프트웨어 엔지니어다.
내가 제공하는 [대상 스키마] JSON을 분석하여, 엑셀 데이터를 코드로 변환하기 위한 Scriban 템플릿(.liquid)을 작성하라.

### 사용 가능한 공통 변수:
- {{ namespace }}: 출력 파일의 네임스페이스
- {{ class_name }}: 클래스 이름
- {{ fields }}: 필드 리스트 (각 필드는 name, type, is_list, is_reference, read_method 속성을 가짐)

[대상 스키마 JSON]
(여기에 앱에서 생성한 스키마 JSON 내용을 붙여넣으세요)

[추가 요구사항 및 스타일 - 이 부분을 자유롭게 수정하세요]
- 예: "모든 필드는 readonly 프로퍼티로 만들어줘."
- 예: "Unity 환경에서 사용할 수 있도록 SerializeField를 붙여줘."
- 예: "필드명을 파스칼 케이스(PascalCase)로 변환해줘."

### 규칙:
1. 출력은 오직 Scriban 템플릿 코드만 포함해야 하며, 설명이나 마크다운 코드 블록(```)은 제거하라.
2. 제공된 변수명({{ fields }} 등)을 정확히 사용하라.
```

#### 💡 활용 예시
**사용자 입력:**
> "... [추가 요구사항]: 모든 필드를 public getter가 있는 private set 프로퍼티로 만들고, Unity용 [SerializeField]를 private 필드에 적용해줘."

**AI 생성 결과물 (예시):**
```liquid
namespace {{ namespace }}
{
    public class {{ class_name }}
    {
        {% for field in fields %}
        [SerializeField] private {{ field.type }} _{{ field.name | string.downcase }};
        public {{ field.type }} {{ field.name }} => _{{ field.name | string.downcase }};
        {% end %}
    }
}
```

---

## 💡 활용 예시 (Example)

### 1. 원본 엑셀 (Item.xlsx)
| Id (int) | Name (string) | Price (int) | Skill (int) | Skill (int) |
| :--- | :--- | :--- | :--- | :--- |
| 101 | 드래곤 슬레이어 | 5000 | 10 | 20 |

### 2. 스키마 정의 (Item_Schema.json)
```json
{
  "className": "ItemData",
  "key": "Id",
  "fields": {
    "Id": "int",
    "Name": "string",
    "Price": "int",
    "Skill": "List<int:ref:Skill>"
  }
}
```

### 3. 생성된 C# 코드 (ItemData.cs)
```csharp
public sealed partial class ItemData : StaticDataBase
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public IReadOnlyList<int> Skill { get; private set; }

    // 참조 프로퍼티 자동 생성
    public IEnumerable<SkillData> Skill_Ref {
        get {
            foreach (var x in Skill) yield return StaticDataManager.Instance.Skill[x];
        }
    }
}
```

---

## 🤖 CLI 활용 (Automation)
CI/CD 환경이나 배치 파일에서 다음과 같이 활용할 수 있습니다.
```powershell
# 특정 기능(FDF ID)의 모든 파일에 대해 데이터 추출 및 코드 생성 실행
./ExcelBinder.exe --feature my_project_data --all --export --codegen --both
```

## 📚 상세 문서
- [💻 설치 및 환경 설정 (ExcelBinder/SETUP.md)](./ExcelBinder/SETUP.md)
- [📝 기능 정의 및 템플릿 가이드 (ExcelBinder/README_EXT.md)](./ExcelBinder/README_EXT.md)
- [📋 전체 요구사항 명세 (ExcelBinder/Requirements.md)](./ExcelBinder/Requirements.md)
