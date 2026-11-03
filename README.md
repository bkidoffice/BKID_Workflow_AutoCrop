# BKID Workflow AutoCrop

<img src="BKID_AutoCrop/assets/BKID_AutoCrop_Icon.svg" width="96" alt="BKID AutoCrop 로고">

Windows Rhino 6 / 8의 현재 뷰포트를 투명 PNG로 캡처하고, 알파 영역을 기준으로 잘라 사방에 1px 투명 여백을 추가하는 도구입니다.

## 설치

1. [단일 설치 파일](Install_BKID_AutoCrop.py)을 열고 **Download raw file**로 저장합니다.
2. Rhino에서 `_RunPythonScript`를 입력하고 받은 파일을 선택합니다.
3. **BKID AutoCrop 로고 버튼**을 클릭해 사용합니다. `RunAutoCrop` 명령도 사용할 수 있습니다.

설치기가 AppData에 필요한 파일을 복사하고 명령과 로고 툴바를 등록합니다. 설치 후 다운로드 파일을 이동하거나 삭제해도 됩니다. Rhino 6과 8에서는 각각 한 번 설치합니다. IronPython 2.7용이며 별도 pip 패키지는 필요하지 않습니다.

[자세한 사용 방법·빌드·검증 현황](BKID_AutoCrop/README_KO.md)

설치 도구 1.2.2 / 캡처 엔진 1.1. 자동 테스트 12개를 통과했으며 Windows Rhino의 실제 툴바 로딩·표시·캡처·재시작 유지 확인은 아직 필요합니다.

기본 툴바 표시가 지원되지 않으면 별도 로고 버튼 창을 엽니다. 닫았거나 Rhino를 재시작했다면 `ShowAutoCrop`으로 다시 열 수 있습니다. 이 창은 Rhino의 도킹 툴바와 다릅니다.
