# 💻 ExcelBinder 설치 및 설정 가이드

ExcelBinder는 독립적인 외부 툴로서 프로젝트 데이터 파이프라인을 구축하는 데 사용됩니다.

---

## 🏗️ 1. 환경 요구사항

- **OS**: Windows 10/11
- **Runtime**: [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) 이상
- **IDE**: Visual Studio 2022 또는 JetBrains Rider (권장)

---

## 📁 2. 프로젝트 구조 가이드

ExcelBinder는 고정된 구조를 강요하지 않지만, 다음과 같은 **독립적인 워크스페이스** 구성을 권장합니다.

```text
ProjectRoot/
├── GameClient/ (또는 Server)
└── ExternalData/ (ExcelBinder 워크스페이스)
    ├── Excel/      # 원본 데이터 (.xlsx)
    ├── Schemas/    # 데이터 규격 (.json)
    ├── Features/   # 기능 정의 파일 (.json)
    ├── Templates/  # 코드 템플릿 (.liquid)
    └── Output/     # 최종 결과물 (Binary, JSON, Scripts)
```

---

## 🚀 3. 핵심 워크플로우

1. **FDF 등록**: 
   - `Settings` 창 진입 → `Bind External Feature` 클릭 → 준비된 FDF 파일 선택.
2. **대시보드 확인**: 
   - 메인 화면에 생성된 기능 카드를 확인합니다.
3. **작업 수행**: 
   - 카드를 클릭하여 파일 목록으로 진입합니다.
   - 대상 엑셀을 선택한 후 `Export` 또는 `Generate Code`를 실행합니다.
4. **결과 확인**: 
   - FDF에 정의된 `exportPath` 및 `scriptsPath`에서 결과를 확인합니다.

---

## ⚠️ 주의사항 및 운영 규칙

- **인코딩**: 모든 출력물은 `UTF-8 (BOM 없음)`으로 생성됩니다.
- **경로 설정**: FDF 파일 내에서 경로를 지정할 때, 팀 협업 환경이라면 **상대 경로**보다는 환경 변수나 절대 경로를 활용하는 것이 안정적일 수 있습니다. (앱 내에서는 상대 경로도 현재 작업 디렉토리 기준으로 지원합니다.)
- **자동화**: 반복적인 작업은 CLI 명령을 배치 파일(`.bat`)로 만들어 관리하세요.