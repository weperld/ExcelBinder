# Test Guidelines

> ExcelBinder 프로젝트의 테스트 작성 가이드입니다.

---

## 🧪 단위 테스트 작성 규칙

### AAA 패턴 (Arrange-Act-Assert)
모든 단위 테스트는 AAA 패턴을 따라야 합니다.

```csharp
[Fact]
public void Test_ExportAsync_ValidFile_Success()
{
    // Arrange (준비)
    var processor = new CSVProcessor();
    var filePath = "test_data.csv";

    // Act (실행)
    var result = await processor.ExportAsync(filePath);

    // Assert (검증)
    Assert.NotNull(result);
    Assert.Equal(3, result.Count);
}
```

---

## 📝 테스트 네이밍 규칙

### 메서드 네이밍
```
Test_{MethodName}_{Condition}_{ExpectedResult}
```

**예시:**
- `Test_ExportAsync_ValidFile_Success`
- `Test_ExportAsync_NullFile_ThrowsException`
- `Test_LoadData_EmptyFile_ReturnsEmptyList`
- `Test_ExportAsync_InvalidData_ThrowsException`

---

## 🎯 테스트 유형

### 1. 단위 테스트 (Unit Test)
단일 메서드 또는 클래스를 테스트

```csharp
public class CSVProcessorTests
{
    [Fact]
    public async Task ExportAsync_ValidFile_ReturnsData()
    {
        // Arrange
        var processor = new CSVProcessor();
        var filePath = "valid_data.csv";

        // Act
        var result = await processor.ExportAsync(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}
```

### 2. 통합 테스트 (Integration Test)
여러 컴포넌트의 통합을 테스트

```csharp
public class CSVIntegrationTests
{
    [Fact]
    public async Task CSVProcessor_WithViewModel_DataFlowsCorrectly()
    {
        // Arrange
        var viewModel = new CSVExecutionViewModel();
        var processor = new CSVProcessor();
        var filePath = "test_data.csv";

        // Act
        await viewModel.ExecuteExport(filePath);
        var result = await processor.ExportAsync(filePath);

        // Assert
        Assert.True(viewModel.IsSuccess);
        Assert.NotNull(result);
    }
}
```

---

## 🎭 Mock 사용법 (Moq)

### Mock 생성
```csharp
[Fact]
public void ExportAsync_WithMockedService_Succeeds()
{
    // Arrange
    var mockService = new Mock<IExcelService>();
    mockService
        .Setup(x => x.ReadExcel(It.IsAny<string>()))
        .Returns(new List<DataItem>());

    var processor = new CSVProcessor(mockService.Object);

    // Act
    var result = processor.ExportAsync("test.csv");

    // Assert
    mockService.Verify(x => x.ReadExcel("test.csv"), Times.Once);
}
```

### Mock 설정 (Setup)
```csharp
mockService
    .Setup(x => x.ReadExcel(It.Is<string>(s => s.EndsWith(".csv"))))
    .Returns(new List<DataItem> { new DataItem { Id = 1 } });

mockService
    .Setup(x => x.ReadExcel(It.Is<string>(s => s.EndsWith(".xlsx"))))
    .Throws(new NotSupportedException("Only CSV files are supported"));
```

### Mock 검증 (Verify)
```csharp
mockService.Verify(x => x.ReadExcel("test.csv"), Times.Once);
mockService.Verify(x => x.SaveData(It.IsAny<string>(), It.IsAny<List<DataItem>>()), Times.AtLeastOnce);
mockService.VerifyNoOtherCalls();
```

---

## 🚫 테스트 작성 시 피해야 할 것

### ❌ 잘못된 예시
```csharp
// 테스트 대상이 불분명
[Fact]
public void Test1()
{
    var processor = new CSVProcessor();
    var result = processor.ExportAsync("test.csv");
    Assert.NotNull(result);
}

// 테스트가 너무 많은 것을 테스트함
[Fact]
public void ExportAsync_ValidFile_AllChecks()
{
    var processor = new CSVProcessor();
    var result = processor.ExportAsync("test.csv");
    Assert.NotNull(result);
    Assert.Equal(3, result.Count);
    Assert.Equal("test", result[0].Name);
    Assert.True(result[0].Value > 0);
    // ... 더 많은 검증
}
```

### ✅ 올바른 예시
```csharp
// 테스트 대상이 명확
[Fact]
public async Task ExportAsync_ValidFile_ReturnsData()
{
    var processor = new CSVProcessor();
    var result = await processor.ExportAsync("test.csv");
    Assert.NotNull(result);
}

// 단일 책임 원칙 준수
[Fact]
public async Task ExportAsync_ValidFile_ReturnsThreeItems()
{
    var processor = new CSVProcessor();
    var result = await processor.ExportAsync("test.csv");
    Assert.Equal(3, result.Count);
}

[Fact]
public async Task ExportAsync_ValidFile_FirstItemNameIsTest()
{
    var processor = new CSVProcessor();
    var result = await processor.ExportAsync("test.csv");
    Assert.Equal("test", result[0].Name);
}
```

---

## 🤖 테스트 자동 생성 규칙

### 자동 생성 규칙

새로운 기능이 추가될 때, @tester는 다음 규칙에 따라 단위 테스트를 자동으로 생성해야 합니다:

1. **Processor 클래스** → `{ProcessorName}Tests.cs` 생성
2. **ViewModel 클래스** → `{ViewModelName}Tests.cs` 생성
3. **Model 클래스** → `{ModelName}Tests.cs` 생성 (필요 시)

### 자동 생성 포맷

**Processor 테스트:**
```csharp
using Xunit;
using Moq;
using ExcelBinder.Services.Processors;
using ExcelBinder.Services;

namespace ExcelBinder.Tests.Processors;

public class CSVProcessorTests
{
    [Fact]
    public async Task ExportAsync_ValidFile_ReturnsData()
    {
        // Arrange
        var mockService = new Mock<IExcelService>();
        var processor = new CSVProcessor(mockService.Object);
        var filePath = "test_data.csv";

        // Act
        var result = await processor.ExportAsync(filePath);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExportAsync_NullFile_ThrowsException()
    {
        // Arrange
        var processor = new CSVProcessor();
        string? filePath = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => processor.ExportAsync(filePath!));
    }

    [Fact]
    public async Task ExportAsync_EmptyFile_ReturnsEmptyList()
    {
        // Arrange
        var processor = new CSVProcessor();
        var filePath = "empty_data.csv";

        // Act
        var result = await processor.ExportAsync(filePath);

        // Assert
        Assert.Empty(result);
    }
}
```

**ViewModel 테스트:**
```csharp
using Xunit;
using ExcelBinder.ViewModels;

namespace ExcelBinder.Tests.ViewModels;

public class CSVExecutionViewModelTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Act
        var viewModel = new CSVExecutionViewModel();

        // Assert
        Assert.False(viewModel.IsBusy);
        Assert.NotNull(viewModel.ExecuteExport);
    }

    [Fact]
    public void ExecuteExport_WhenCalled_UpdatesIsBusy()
    {
        // Arrange
        var viewModel = new CSVExecutionViewModel();

        // Act
        viewModel.ExecuteExport.Execute(null);

        // Assert
        Assert.True(viewModel.IsBusy);
    }
}
```

---

## 📊 테스트 커버리지

### 커버리지 확인
```bash
cd ExcelBinder.Tests
dotnet test --collect:"XPlat Code Coverage"
```

### 커버리지 기준
- **최소 커버리지**: 80%
- **권장 커버리지**: 90% 이상
- **핵심 모듈**: 95% 이상

---

## 📚 관련 문서

- [코드 스타일 가이드](../.guides/CODE_STYLE.md)
- [빌드 가이드](../.guides/BUILD_GUIDE.md)
- [기술 규칙](../.guides/TECHNICAL_RULES.md)
