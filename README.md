# BKID Workflowboost AutoCrop

<img src="BKID_AutoCrop/assets/BKID_AutoCrop_Icon.svg" width="96" alt="BKID AutoCrop 로고">

Windows Rhino의 실제 **BKID Workflowboost** 툴바에서 로고 버튼으로 투명 PNG를 저장합니다.

## 설치 / 다시 설치 — 2.2

1. [설치 프로그램 다운로드 (.exe)](https://github.com/bkidoffice/BKID_Workflow_AutoCrop/raw/refs/heads/main/installers/BKID_AutoCrop_Setup_2.2.0.exe)
2. Rhino를 모두 종료하고 실행 → **설치 / 다시 설치**
3. 이어지는 Rhino Installer에서 **현재 사용자 설치** 완료

기존 BKID 버전은 **백업 없이 삭제**합니다. 새 설치가 취소되거나 실패해도 이전 버전은 자동 복원되지 않습니다. 저장된 PNG와 Rhino 모델은 삭제하지 않습니다.

툴바를 자동 표시하는 것은 이 사용자 설정에서 첫 실행 때 한 번뿐입니다. 도킹한 뒤에는 Rhino가 저장한 위치와 표시 상태를 따르며, 매번 별도 툴바 창을 강제로 열지 않습니다. 닫은 툴바는 `BKIDAutoCrop`으로 다시 표시할 수 있습니다.

[자세한 설치·삭제 범위](installers/README_KO.md) · [개발자 빌드](plugin/README.md) · [캡처 엔진](BKID_AutoCrop/README_KO.md)

빌드·패키지·파일 삭제 검사는 통과했습니다. 실제 Windows 설치·레지스트리 처리와 Rhino 재시작 후 도킹 상태는 아직 미검증입니다. 이전 보고의 보안 프로그램 위협 탐지 원인도 미확인입니다.
