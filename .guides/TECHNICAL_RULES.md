# Technical Compliance Rules

> ExcelBinder 프로젝트의 기술적 준수 사항을 정의합니다.

---

## 1. 비동기 예외 처리

### 필수 규칙
모든 비동기 작업은 `try-finally` 블록 사용하여 **어떤 상황에서도** 로딩 상태(`IsBusy`)가 해제되도록 보장

### 패턴
```csharp
private async void ExecuteOperation()
{
    if (IsBusy) return;
    try
    {
        IsBusy = true;
        // 비동기 작업
    }
    finally
    {
        IsBusy = false;
    }
}
```

### 위반 시 발생하는 문제
- 예외 발생 시 UI가 영구적으로 로딩 상태로 유지
- 사용자가 추가 작업 불가

---

## 2. 데이터 무결성 우선

### 핵심 원칙
엑셀 데이터와 스키마 정의가 불일치하거나 필수 데이터를 찾을 수 없는 경우:
- ❌ **임의의 기본값을 채우지 않음**
- ✅ **명시적인 예외(Exception) 발생**

### 적용 상황
- 스키마 파일이 존재하지 않음
- 필수 컬럼이 누락됨
- 데이터 타입이 불일치
- 키 필드가 null 또는 중복

### 예시
```csharp
// ✅ 올바른 예
if (schema == null)
    throw new FileNotFoundException($"Schema not found: {schemaFile}");

// ❌ 잘못된 예 (데이터 오염)
if (schema == null)
    schema = new SchemaDefinition();
```

---

## 3. 라이브러리 사용 규칙

### Excel 처리: NPOI
- `.xlsx`: `XSSFWorkbook`
- `.xls`: `HSSFWorkbook`

### JSON 처리: Newtonsoft.Json
```csharp
// 직렬화
var json = JsonConvert.SerializeObject(obj, Formatting.Indented);

// 역직렬화
var obj = JsonConvert.DeserializeObject<T>(json);
```

### 템플릿 엔진: Scriban
```csharp
var template = Template.Parse(liquidTemplate);
var result = template.Render(model);
```

### HTTP 클라이언트
- 소켓 고갈 방지: `HttpClient` 정적 멤버로 공유
```csharp
private static readonly HttpClient _httpClient = new();
```

---

## 4. 상수 관리

### ProjectConstants 클래스 사용
프로젝트 전체 상수는 `ProjectConstants.cs`에서 관리

```csharp
public class ProjectConstants
{
    public class Files
    {
        public const string AppSettings = "settings.json";
    }

    public class Excel
    {
        public const int HeaderRowIndex = 0;
        public const int DataStartRowIndex = 1;
        public const string CommentPrefix = "#";
    }

    public class Extensions
    {
        public const string Binary = ".bytes";
        public const string Json = ".json";
    }

    public class Categories
    {
        public const string StaticData = "StaticData";
        public const string Logic = "Logic";
        // ...
    }
}
```
