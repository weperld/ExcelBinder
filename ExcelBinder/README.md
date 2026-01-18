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

## 🤖 외부 AI 도구 활용 (Prompt Template)
AI Assistant용 API 키가 없더라도, ChatGPT나 Claude와 같은 외부 AI 도구를 활용하여 최적의 템플릿을 얻을 수 있습니다. 상세 가이드는 [루트 README.md](../README.md#4--외부-ai-도구-활용-prompt-template)를 참고하세요.

## 📚 상세 문서
- [💻 설치 및 환경 설정 (SETUP.md)](./SETUP.md)
- [📝 기능 정의 및 템플릿 가이드 (README_EXT.md)](./README_EXT.md)
- [📋 전체 요구사항 명세 (Requirements.md)](./Requirements.md)