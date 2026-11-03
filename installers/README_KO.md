# BKID AutoCrop 2.0 — 버튼 설치 및 자동 시작 창

## 설치

1. Windows Rhino를 모두 종료합니다.
2. 이 폴더의 `BKID_AutoCrop_2.0.0.rhi`를 다운로드하고 **더블클릭**합니다.
3. Rhino Installer 화면에서 대상 Rhino와 현재 사용자 설치 옵션을 확인하고 **설치**를 누릅니다.
4. Rhino를 다시 실행합니다. 플러그인이 활성화되어 있으면 첫 모델 문서가 준비된 후 **BKID AutoCrop** 창을 자동으로 표시하도록 구현되어 있습니다.
5. 창의 **PNG 저장** 버튼으로 현재 모델 뷰포트를 캡처합니다.

이 버전은 실제 컴파일된 `.rhp`를 포함하는 Rhino 표준 Windows 설치 패키지입니다. `_RunPythonScript`를 실행해 등록할 필요가 없습니다. 확장자를 `.zip`으로 바꾸거나 압축을 풀어서 설치하지 마세요. 저장소 전체 ZIP을 받은 경우에만 압축을 풀고 그 안의 `installers/BKID_AutoCrop_2.0.0.rhi`를 실행하세요.

`.rhi` 연결 프로그램이 없으면 Windows의 연결 프로그램에서 해당 Rhino 버전의 Rhino Installer를 선택해야 합니다. 파일이 `.rhi.zip`으로 저장되었다면 실제 확장자를 확인하세요.

## 창과 명령

- **PNG 저장**: 기존 캡처 엔진으로 투명 PNG를 저장합니다. 기본 위치는 바탕화면 `BKID_Captures`입니다.
- **Rhino 시작 시 창 열기**: 기본값은 켜짐입니다. 끄면 이후 Rhino 시작 시 자동으로 창을 열지 않습니다. 설정은 Rhino 플러그인 설정에 저장됩니다.
- **BKIDAutoCrop**: 닫은 창을 다시 엽니다. 이미 열려 있으면 앞으로 가져옵니다.
- **BKIDAutoCropCapture**: 창 없이 캡처 명령을 실행합니다.

별도 창은 Rhino가 소유하는 모델리스 창입니다. Rhino에서 모델링을 계속하면서 사용할 수 있습니다. Grasshopper의 기능이나 노드 편집기를 포함하는 것은 아닙니다.

이전 1.x 설치의 `RunAutoCrop`, `ShowAutoCrop`, 툴바는 삭제하거나 덮어쓰지 않습니다. 새 창은 별도 명령 `BKIDAutoCropCapture`를 사용하므로 이전 별칭과 충돌하지 않습니다. 이전 툴바가 남아 있으면 Rhino의 툴바 설정에서 숨길 수 있습니다. 새 버전은 AppData의 1.x 스크립트를 사용하지 않습니다.

플러그인을 더 이상 자동 로드하지 않으려면 Rhino의 플러그인 설정에서 **BKID AutoCrop**을 비활성화합니다. 창만 자동으로 열리지 않게 하려면 창의 체크박스를 끄면 됩니다.

## 검증 범위

- RhinoCommon 6.35 SDK와 .NET Framework 4.5를 대상으로 실제 Release 빌드 성공: 오류 0, 경고 0.
- 설치기가 식별하는 어셈블리 GUID·버전과 RhinoCommon/Eto 참조 확인.
- 플러그인에 내장된 캡처 스크립트와 로고가 저장소 원본과 일치하는지 확인.
- RHI ZIP 무결성과 내부 `.rhp` 바이트 일치 확인. RhinoCommon/Eto DLL은 재배포하지 않고 Rhino 제공 어셈블리를 사용합니다.
- **Windows Rhino 6/8 실기 설치, 시작 시 자동 창, 설정 유지, 버튼 캡처는 미실행**입니다. Linux 빌드는 실제 Rhino 호스트 테스트를 대신하지 않습니다.

실기 확인 순서: 설치 → Rhino 실행 → 로고 창 표시 → PNG 저장 → 창 닫기 → `BKIDAutoCrop`으로 재열기 → 자동 시작 체크 해제 후 Rhino 재시작 → 자동 시작 체크 재설정 후 재시작.

오류가 있으면 Rhino 버전과 F2 명령 기록을 확인하세요. 자동 창 오류는 `BKID AutoCrop startup:`으로 출력됩니다. 플러그인 자체가 목록에 없거나 로드되지 않으면 Rhino 플러그인 설정의 로드 오류를 확인해야 합니다.
