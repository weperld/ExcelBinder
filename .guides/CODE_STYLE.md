# Code Style Guidelines

> ExcelBinder 프로젝트의 코드 작성 표준을 정의합니다.

---

## 🏗️ 아키텍처

### MVVM 패턴
- **ViewModels**: `ViewModelBase` 상속 필수
- **Models**: 순수 데이터 클래스, JSON 직렬화 지원
- **Views**: XAML, ViewModel 바인딩

### 프로젝트 구조
```
ExcelBinder/
├── Models/           # 데이터 모델
├── ViewModels/       # MVVM ViewModels
├── Views/            # XAML Views
├── Services/         # 비즈니스 로직
│   └── Processors/   # Feature별 프로세서
└── Common/           # 공통 유틸리티
```

### 명령 패턴
- `RelayCommand` 또는 `RelayCommand<T>` 사용
- `CommonModels.cs`에 정의된 타입 활용

---

## 📝 명명 규칙

### PascalCase
- **클래스**: `FeatureService`, `ViewModelBase`
- **메서드**: `LoadSettings`, `ExportToBinary`
- **속성**: `IsBusy`, `SelectedFeature`
- **인터페이스**: `IFeatureProcessor`

### _camelCase
- **private 필드**: `_feature`, `_isBusy`, `_httpClient`

### ALL_CAPS
- **상수**: `HeaderRowIndex`, `CommentPrefix` (ProjectConstants)

### 예시
```csharp
public class FeatureBuilderViewModel : ViewModelBase
{
    private FeatureDefinition _feature;
    private bool _isSchemaPathVisible;

    public FeatureDefinition Feature { get => _feature; set => SetProperty(ref _feature, value); }
    public bool IsSchemaPathVisible { get => _isSchemaPathVisible; set => SetProperty(ref _isSchemaPathVisible, value); }
}
```

---

## ⚡ 비동기 패턴

### 필수 패턴: try-finally
모든 비동기 작업은 `try-finally` 블록 사용하여 `IsBusy` 상태 보장

```csharp
private async void ExecuteExport()
{
    if (IsBusy) return;
    try
    {
        IsBusy = true;
        var processor = FeatureProcessorFactory.GetProcessor(_feature.Category);
        await processor.ExecuteExportAsync(this);
    }
    finally
    {
        IsBusy = false;
    }
}
```

### Task.Run 사용 (UI 차단 방지)
```csharp
// CPU 집약적 작업
var data = await Task.Run(() => excelService.ReadExcel(filePath, sheetName).ToList());

// 파일 I/O
await Task.Run(() => File.WriteAllText(outputPath, json));
```

---

## ⚠️ 에러 처리

### 데이터 무결성 우선
- **절대 기본값으로 채우지 않음**
- 명시적인 예외(Exception) 발생

### 올바른 예
```csharp
if (schema == null)
    throw new FileNotFoundException($"Schema not found: {schemaFile}");

if (string.IsNullOrEmpty(apiKey))
    throw new Exception("API Key가 설정되지 않았습니다.");

if (!File.Exists(filePath))
    throw new FileNotFoundException($"Excel file not found: {filePath}");
```

### 잘못된 예 (❌ 금지)
```csharp
if (schema == null)
    schema = new SchemaDefinition(); // 데이터 오염 위험!
```

### 사용자용 에러 메시지
- `LogService.Instance.Error()` 사용
- 상세한 에러 메시지 제공
```csharp
catch (Exception ex)
{
    LogService.Instance.Error($"Error exporting {sheetName}: {ex.Message}");
}
```

---

## 📦 Import 구성

### 순서
1. **System 네임스페이스** (그룹화)
2. **타사 라이브러리** (Newtonsoft, NPOI, Scriban)
3. **프로젝트 네임스페이스** (ExcelBinder.*)

### 예시
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using ExcelBinder.Models;
using ExcelBinder.Services;
```

---

## 📖 XML 문서화

### 필수 XML 주석
- 모든 **public** 클래스, 메서드, 속성
- `summary`, `param`, `returns`, `exception` 태그 사용
- 한국어로 작성

### 예시

#### 클래스
```csharp
/// <summary>
/// CSV 파일을 처리하여 데이터를 추출하는 프로세서 클래스입니다.
/// </summary>
public class CSVProcessor : IFeatureProcessor
{
    // ...
}
```

#### 메서드
```csharp
/// <summary>
/// CSV 파일을 처리하여 데이터를 추출합니다.
/// </summary>
/// <param name="filePath">CSV 파일 경로</param>
/// <returns>추출된 데이터 목록</returns>
/// <exception cref="FileNotFoundException">파일이 존재하지 않을 때 발생</exception>
/// <exception cref="IOException">파일 읽기 오류 발생 시</exception>
public async Task<List<DataItem>> ExportAsync(string filePath)
{
    // ...
}
```

#### 속성
```csharp
/// <summary>
/// 처리 중인지 여부를 나타냅니다.
/// </summary>
public bool IsProcessing { get; private set; }

/// <summary>
/// 현재 진척도를 0~100 사이의 값으로 나타냅니다.
/// </summary>
public int Progress { get; private set; }
```

#### 이벤트
```csharp
/// <summary>
/// 처리 완료 시 발생하는 이벤트입니다.
/// </summary>
public event EventHandler<ProcessCompleteEventArgs>? ProcessComplete;
```

#### 인터페이스
```csharp
/// <summary>
/// 기능 프로세서를 위한 인터페이스입니다.
/// </summary>
public interface IFeatureProcessor
{
    /// <summary>
    /// 기능을 실행합니다.
    /// </summary>
    /// <param name="viewModel">실행 뷰모델</param>
    Task ExecuteAsync(IExecutionViewModel viewModel);
}
```

### XML 주석 태그

| 태그 | 용도 | 예시 |
|-----|------|------|
| `summary` | 요약 | `/// <summary>`데이터를 추출합니다.`</summary>` |
| `param` | 파라미터 | `/// <param name="filePath">파일 경로</param>` |
| `returns` | 반환값 | `/// <returns>`추출된 데이터 목록</returns>` |
| `exception` | 예외 | `/// <exception cref="FileNotFoundException">`파일이 존재하지 않을 때 발생`</exception>` |
| `remarks` | 비고 | `/// <remarks>`이 메서드는 비동기로 실행됩니다.`</remarks>` |
| `example` | 예시 | `/// <example>`이 메서드를 사용하는 방법...`</example>` |
| `see` | 참조 | `/// <see cref="ExportAsync"/>` |

### XML 문서 생성

#### 프로젝트 파일 설정 (ExcelBinder.csproj)
```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

#### API 문서 생성
```bash
cd ExcelBinder
dotnet build
```

빌드 시 `ExcelBinder.xml` 파일이 생성됩니다. 이 파일을 사용하여 API 문서를 자동으로 생성할 수 있습니다.

### 예외 처리 문서화
```csharp
/// <summary>
/// 엑셀 데이터를 로드합니다.
/// </summary>
/// <param name="filePath">엑셀 파일 경로</param>
/// <returns>로드된 데이터 목록</returns>
/// <exception cref="FileNotFoundException">
/// 지정된 파일이 존재하지 않을 때 발생
/// </exception>
/// <exception cref="IOException">
/// 파일 읽기 오류 발생 시 발생
/// </exception>
/// <exception cref="NotSupportedException">
/// 지원하지 않는 파일 형식일 때 발생
/// </exception>
public async Task<List<string[]>> LoadExcelDataAsync(string filePath)
{
    if (!File.Exists(filePath))
        throw new FileNotFoundException($"Excel file not found: {filePath}");

    if (!filePath.EndsWith(".xlsx") && !filePath.EndsWith(".xls"))
        throw new NotSupportedException("Only .xlsx and .xls files are supported");

    // ...
}
```

### 사용자 정의 태그
```csharp
/// <summary>
/// CSV 파일을 처리합니다.
/// </summary>
/// <param name="filePath">CSV 파일 경로</param>
/// <feature category="StaticData" />
/// <author>Junie</author>
/// <version>1.0.0</version>
public async Task<List<DataItem>> ExportAsync(string filePath)
{
    // ...
}
```

### XML 주석 예시 (전체)
```csharp
/// <summary>
/// CSV 파일을 처리하여 데이터를 추출하는 프로세서 클래스입니다.
/// </summary>
/// <remarks>
/// 이 클래스는 CSV 파일을 읽고, 데이터를 추출하여 JSON 형식으로 변환합니다.
/// 지원하는 CSV 형식은 다음과 같습니다:
/// - UTF-8 인코딩
/// - 쉼표(,) 구분자
/// - 따옴표(") 필드 구분자
/// </remarks>
/// <example>
/// 다음은 CSVProcessor를 사용하는 방법입니다:
/// <code>
/// var processor = new CSVProcessor();
/// var data = await processor.ExportAsync("data.csv");
/// foreach (var item in data)
/// {
///     Console.WriteLine($"{item.Id}: {item.Name}");
/// }
/// </code>
/// </example>
public class CSVProcessor : IFeatureProcessor
{
    /// <summary>
    /// CSV 파일을 처리하여 데이터를 추출합니다.
    /// </summary>
    /// <param name="filePath">CSV 파일 경로</param>
    /// <returns>추출된 데이터 목록</returns>
    /// <exception cref="FileNotFoundException">파일이 존재하지 않을 때 발생</exception>
    /// <exception cref="IOException">파일 읽기 오류 발생 시</exception>
    /// <exception cref="CSVFormatException">CSV 형식이 올바르지 않을 때 발생</exception>
    public async Task<List<DataItem>> ExportAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"CSV file not found: {filePath}");

        // ...

        return data;
    }

    /// <summary>
    /// CSV 데이터를 파싱합니다.
    /// </summary>
    /// <param name="csvContent">CSV 파일 내용</param>
    /// <param name="separator">구분자 (기본값: 쉼표)</param>
    /// <returns>파싱된 데이터 목록</returns>
    private List<string[]> ParseCSV(string csvContent, char separator = ',')
    {
        // ...
    }
}
```

---

## 🎯 Nullable Reference Types

### 사용 규칙
- 프로젝트는 nullable reference types 활성화
- null 가능 여부 명시적으로 표현

### 예시
```csharp
public FeatureDefinition? LoadFeatureFromFile(string filePath)
{
    if (!File.Exists(filePath)) return null;
    // ...
}

public void ProcessFeature(FeatureDefinition feature) // null 허용 안 함
{
    // ...
}
```

### 경고 수정
- CS8618: non-nullable 필드 초기화
- CS8602: null 가능 참조 역참조
- CS8600: null 값 비-nullable로 변환

---

## 🖼️ WPF/XAML 패턴

### 리소스 사용
- `App.xaml`에서 스타일 정의
- `{StaticResource ResourceKey}`로 참조

### 커맨드 바인딩
```xaml
<Button Content="Save" Command="{Binding SaveCommand}"/>
```

```csharp
public ICommand SaveCommand { get; }
```

### 데이터 바인딩
```xaml
<TextBlock Text="{Binding Feature.Name}"/>
```

```csharp
public FeatureDefinition Feature { get => _feature; set => SetProperty(ref _feature, value); }
```
