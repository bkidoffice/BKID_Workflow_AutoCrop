# BKID Workflow AutoCrop

<img src="BKID_AutoCrop/assets/BKID_AutoCrop_Icon.svg" width="96" alt="BKID AutoCrop 로고">

Windows Rhino용 투명 PNG 자동 크롭 도구입니다. **2.0은 설치 버튼으로 등록하는 정식 플러그인**이며 Rhino 시작 시 전용 창을 열도록 구성했습니다.

## 간편 설치

1. [BKID AutoCrop 2.0 설치 파일 다운로드](https://github.com/bkidoffice/BKID_Workflow_AutoCrop/raw/refs/heads/main/installers/BKID_AutoCrop_2.0.0.rhi)
2. Rhino를 종료하고 `.rhi` 파일을 더블클릭 → Rhino Installer에서 **설치**
3. Rhino 재실행 → **BKID AutoCrop** 창의 **PNG 저장** 버튼 사용

저장소 ZIP을 받았다면 압축을 풀고 `installers/BKID_AutoCrop_2.0.0.rhi`를 실행합니다. Python 설치 스크립트를 선택할 필요가 없습니다.

로고와 캡처 스크립트는 플러그인 안에 포함됩니다. 창을 닫았으면 `BKIDAutoCrop`으로 다시 열고, **Rhino 시작 시 창 열기** 체크박스로 자동 표시를 설정합니다.

[설치·사용·검증 상태](installers/README_KO.md) · [개발자 빌드](plugin/README.md) · [기존 캡처 엔진 설명](BKID_AutoCrop/README_KO.md)

실제 `.rhp` 빌드와 `.rhi` 패키지 검증은 통과했습니다. Windows Rhino 6/8에서 설치·자동 시작·실제 캡처는 아직 미검증입니다.

이전 1.x Python 설치 파일은 기존 사용자를 위해 보존했으며 새 설치 경로는 위의 `.rhi`입니다.
