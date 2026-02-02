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

### 한국어 주석 필수
public API에 `///` 주석 추가

```csharp
/// <summary>
/// 특정 디렉토리 내의 모든 특징 정의 파일(.json)을 로드합니다.
/// </summary>
public IEnumerable<FeatureDefinition> LoadFeatures(string directoryPath)
{
    // ...
}

/// <summary>
/// 엑셀 데이터를 바이너리 형식으로 변환하여 저장합니다.
/// </summary>
public void ExportToBinary(SchemaDefinition schema, IEnumerable<string[]> excelData, string outputPath)
{
    // ...
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
