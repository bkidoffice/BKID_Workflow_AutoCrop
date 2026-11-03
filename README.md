# BKID Work Flow

라이노 기반 제품 디자인 업무 자동화 도구를 개발하는 저장소입니다.

## BKID AutoCrop

Windows Rhino 6 / 8에서 현재 모델 뷰포트를 투명 PNG로 캡처하고, 알파 영역을 기준으로 잘라 사방에 1px 투명 여백을 추가하는 Python 도구입니다.

- [설치·사용 방법 및 검증 현황](BKID_AutoCrop/README_KO.md)
- [실행 스크립트](BKID_AutoCrop/BKID_AutoCrop.py)
- [RunAutoCrop 명령 등록](BKID_AutoCrop/Install_RunAutoCrop.py)

Rhino 명령창에서 `_RunPythonScript`로 설치 스크립트를 실행한 뒤 `RunAutoCrop` 명령을 사용합니다. IronPython 2.7용이며 별도 pip 패키지는 필요하지 않습니다.

첨부된 `BKID_AutoCrop_v1.zip`의 원본 파일을 보존했습니다. 내부 문서의 버전 표기는 1.1입니다. Windows Rhino 6 / 8의 실제 실행 검증은 아직 필요합니다.
