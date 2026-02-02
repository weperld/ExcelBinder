# Build & Development Guide

> ExcelBinder 프로젝트의 빌드, 실행, 개발 관련 명령어를 정리합니다.

---

## 🏗️ 빌드 명령어

### 기본 빌드
```bash
cd ExcelBinder
dotnet build
```
- 빌드 결과: `../Build/ExcelBinder.dll`
- 기본 구성: Debug

### Release 빌드
```bash
dotnet build -c Release
```

### 클린 빌드
```bash
dotnet clean
dotnet build
```

---

## 🚀 실행 명령어

### GUI 실행
```bash
dotnet run
```

### CLI (자동화용)
```bash
# 특정 기능에 대한 데이터 추출 및 코드 생성
dotnet run -- --feature my_project_data --export --codegen

# 모든 파일 처리
dotnet run -- --feature my_project_data --all --export --codegen

# 바이너리만 추출
dotnet run -- --feature my_project_data --export
```

### CLI 옵션 설명
| 옵션 | 설명 |
|------|------|
| `--feature [ID]` | 대상 Feature ID 지정 |
| `--export` | 데이터 추출 (Binary/JSON) |
| `--codegen` | 코드 생성 실행 |
| `--both` | export + codegen 둘 다 실행 |
| `--all` | 모든 파일 처리 (선택하지 않은 파일 포함) |

---

## 📁 출력 구조

```
D:\CS Project\ExcelBinder\
├── Build/                          # 빌드 결과물
│   └── ExcelBinder.dll
├── ExcelBinder/
│   ├── bin/                        # 중간 빌드 결과
│   ├── obj/                        # 컴파일 중간 파일
│   └── settings.json               # 앱 설정 (실행 시 생성)
└── ExternalTestData/               # 테스트용 엑셀 데이터
```

---

## ⚙️ 개발 환경

### .NET SDK
- **필요 버전**: .NET 10.0 이상
- **확인**: `dotnet --info`

### 종속성 (ExcelBinder.csproj)
```xml
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
<PackageReference Include="NPOI" Version="2.7.5" />
<PackageReference Include="Scriban" Version="6.5.2" />
```

---

## 🧪 테스트

⚠️ **현재 테스트 인프라가 없습니다.**

테스트가 필요한 경우 다음 패키지 추가 고려:
- xUnit: `dotnet add package xunit`
- NUnit: `dotnet add package NUnit`
- Moq: `dotnet add package Moq`
