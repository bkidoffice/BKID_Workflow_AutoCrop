# BKID Workflow AutoCrop

<img src="BKID_AutoCrop/assets/BKID_AutoCrop_Icon.svg" width="96" alt="BKID AutoCrop 로고">

Windows Rhino의 **실제 툴바**에서 로고 버튼을 눌러 투명 PNG를 저장하는 도구입니다. 2.1에서는 이전 별도 버튼 창을 제거했습니다.

## 설치 / 다시 설치

1. [BKID AutoCrop 2.1 설치 프로그램 다운로드 (.exe)](https://github.com/bkidoffice/BKID_Workflow_AutoCrop/raw/refs/heads/main/installers/BKID_AutoCrop_Setup_2.1.0.exe)
2. Rhino를 모두 종료하고 설치 프로그램 실행 → **설치 / 다시 설치**
3. 이어서 열리는 Rhino Installer에서 **현재 사용자 설치** 진행
4. Rhino 실행 → **BKID AutoCrop 툴바**의 로고 버튼 사용

설치 프로그램은 기존 BKID 설치를 백업·제거한 뒤 새 버전을 설치합니다. 취소·실패하면 이전 파일과 등록 정보 복원을 시도하며 백업은 보관합니다. 저장해 둔 PNG 파일은 건드리지 않습니다.

툴바를 닫았으면 `BKIDAutoCrop`으로 다시 표시합니다. 버튼은 `BKIDAutoCropCapture` 명령을 실행합니다. Rhino의 툴바이므로 원하는 위치로 옮기거나 도킹해서 사용할 수 있습니다.

[자세한 설치·정리 범위·검증 상태](installers/README_KO.md) · [개발자 빌드](plugin/README.md) · [캡처 엔진 설명](BKID_AutoCrop/README_KO.md)

빌드·패키지·파일 백업/복구 테스트를 통과했습니다. Windows Rhino에서 설치 프로그램 실행, 레지스트리 교체, 실제 툴바 표시와 캡처는 아직 미검증입니다.

기존 Python 설치 파일은 이력 보존용입니다. 새 설치에는 위의 EXE를 사용하세요.
