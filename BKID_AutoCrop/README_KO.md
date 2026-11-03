# BKID AutoCrop 1.1 — Rhino 6 / 8 공통판

Windows Rhino용 자동 투명 PNG 캡처 실행 스크립트입니다. 컴파일된 .rhp 플러그인이 아닌 버튼 등록형 Python 도구입니다.

## 간편 설치 — 설치 도구 1.2.1

1. 저장소 첫 화면의 `Install_BKID_AutoCrop.py`를 **Raw 파일로 다운로드**합니다. 이 파일 하나에 캡처 스크립트와 로고 툴바가 포함되어 있습니다.
2. Windows Rhino 명령창에서 `_RunPythonScript`를 입력하고 다운로드한 파일을 선택합니다.
3. 설치 완료 후 **BKID AutoCrop 로고 버튼**을 클릭합니다. `RunAutoCrop` 명령으로도 실행할 수 있습니다.

설치 시 `%APPDATA%\BKID\AutoCrop\Rhino6` 또는 `Rhino8`에 파일을 복사하고 명령 별칭과 전용 툴바를 등록합니다. 관리자 권한이나 별도의 pip 설치는 필요하지 않습니다. 설치 후 다운로드한 설치 파일은 삭제하거나 이동해도 됩니다. Rhino 6과 8은 각각 한 번 설치합니다.

소스 ZIP을 받은 경우 압축을 푼 뒤 `BKID_AutoCrop/Install_RunAutoCrop.py`를 같은 방식으로 실행해도 됩니다. 이 경우 함께 제공된 `.rui`, 캡처 스크립트, `assets` 폴더가 필요합니다.

같은 설치 파일을 다시 실행하면 기존 설치와 툴바를 갱신합니다. 기존 `RunAutoCrop` 별칭은 새 설치 경로로 바뀝니다. 설치 오류 시 이 설치기가 변경한 파일과 별칭을 복원하며, 복구에도 실패하면 F2 명령 기록에 백업 위치를 남깁니다. 캡처 스크립트를 직접 수정한 경우 업데이트 전에 별도로 보관하세요.

### 로고와 툴바

첨부된 SVG 원본을 `assets/BKID_AutoCrop_Icon.svg`에 보관하고, 투명 PNG 16·24·32px을 생성해 `BKID_AutoCrop.rui`에 내장했습니다. 아이콘을 따로 지정할 필요가 없습니다. 툴바는 `! RunAutoCrop` 매크로를 실행합니다.

설치 후 툴바를 숨긴 경우 Rhino의 툴바 설정에서 `BKID AutoCrop`을 표시하거나 설치 파일을 다시 실행합니다. Rhino 재시작 후 툴바 유지와 Rhino 8의 UI 변환 동작은 실제 Windows Rhino에서 확인이 필요합니다.

기본 캡처 위치는 바탕화면 `BKID_Captures`입니다. 실행 엔진은 두 버전 모두 **IronPython 2.7**이며 Rhino 8 CPython 3은 지원하지 않습니다. 컴파일된 `.rhp` 플러그인이나 패키지 관리자용 `.yak` 파일은 아닙니다.

## 동작 규칙

- 현재 모델 뷰포트에 보이는 전체 장면을 한 장으로 저장합니다. 숨긴 오브젝트는 포함되지 않습니다.
- 카메라, 줌, 오브젝트 형상과 재질을 변경하지 않습니다.
- 그리드와 좌표축은 캡처에서 제외합니다. 일반 오브젝트 및 하위 요소의 선택 강조는 임시 해제 후 finally에서 복원합니다.
- 실제 알파가 0보다 큰 모든 픽셀로 외곽을 계산합니다. RGB 배경색이나 3D 바운딩 박스는 사용하지 않습니다.
- 안티앨리어싱의 희미한 픽셀, 반투명 픽셀, 떨어진 작은 부품을 포함합니다. 완전 투명한 픽셀은 제외됩니다.
- 외곽을 자른 후 사방에 정확히 1px의 투명 여백을 만듭니다. 직사각형 이미지의 네 변을 기준으로 하며, 곡선 외곽을 따라 잘라내는 비정형 파일은 아닙니다.
- 캡처 해상도는 현재 뷰포트 크기 × CAPTURE_SCALE입니다. 최종 PNG 크기는 크롭 결과에 따라 달라집니다. 리사이즈하지 않습니다.
- PNG를 다시 열어 최종 크기와 알파 외곽을 검사하고 검증에 성공한 파일만 최종 이름으로 저장합니다.
- 날짜·시간과 임의 ID를 조합해 파일명 충돌과 기존 파일 덮어쓰기를 피합니다.

## 설정

`CAPTURE_SCALE = 1.0`: 크롭 전 캡처 배율. 예를 들어 2.0은 가로·세로 2배입니다. 최종 여백은 여전히 1px입니다.

`OUTPUT_FOLDER = None`: 기본 저장 폴더. 원하는 경우 `OUTPUT_FOLDER = r"D:\Captures"`처럼 수정합니다.

## 적용 범위와 한계

- 먼저 Shaded 모드로 확인하십시오. Rendered/사용자 지정 모드는 Rhino가 반환하는 알파를 그대로 사용합니다.
- 바닥면·그림자·치수·문자·Picture 오브젝트·Grasshopper 미리보기 등 실제 캡처에 그려진 요소는 알파가 존재하면 포함됩니다. 모델만 필요하면 해당 요소를 끄고 실행하십시오. 그림자를 별도로 분리하는 마스크 패스는 이 버전에 포함되지 않습니다.
- 픽셀이 화면 가장자리에 닿으면 저장을 중단합니다. 줌을 조금 축소하거나 불투명 배경/바닥면을 확인하십시오. 화면 밖에 완전히 나간 오브젝트나 다른 형상 뒤에 가려진 부분은 복원하거나 감지하지 않습니다.
- Layout은 지원하지 않습니다. Raytraced 모드의 수렴 대기, HDR/특수 디스플레이 플러그인, 컨트롤 포인트/그립 선택, 사용자 정의 conduit는 실기 검증 대상입니다.
- 해상도가 높으면 픽셀 검사 중 Rhino UI가 잠시 대기할 수 있습니다.

## 검증 현황

일반 Python 환경에서 문법 검사 및 100개 무작위 알파 마스크의 외곽 비교와 PNG 저장·재읽기 테스트를 통과했습니다. 빈 이미지, 분리된 요소, 반투명 픽셀, 화면 가장자리 픽셀을 포함합니다.

이 환경에는 Windows Rhino가 없어 Rhino 캡처, System.Drawing 처리 및 실제 표시 모드별 결과는 실행 검증하지 않았습니다. 위 테스트는 픽셀 알고리즘 검증이며 Rhino 호환성 테스트를 대신하지 않습니다.

현장 확인: 흰색/검은색 제품, 반투명 재질, 1px 선, 떨어진 작은 부품, 선택된 면, 빈 뷰, 화면 끝 형상, 한글 저장 경로에서 실행 후 확인하십시오. 오류는 명령 기록(F2)에 출력됩니다.

## 조사한 API 및 공개 예제

- 공식 ViewCapture.TransparentBackground API (Rhino 6.0부터): https://developer.rhino3d.com/api/rhinocommon/rhino.display.viewcapture/transparentbackground
- 공식 ViewCapture API: https://developer.rhino3d.com/api/RhinoCommon/html/T_Rhino_Display_ViewCapture.htm
- McNeel 직원의 투명 캡처 Python 예제: https://discourse.mcneel.com/t/capturetobitmap-settings-transparentbackground/145280
- McNeel GitHub Python 예제 경로: https://github.com/mcneel/rhino-developer-samples/blob/6/rhinopython/SampleViewCaptureToFile.py
- GitHub 예제의 공개 코드 발췌 확인: https://discourse.mcneel.com/t/viewcapture-to-file-using-python-grasshopper/95098
- McNeel C# 예제 링크와 발췌 확인: https://discourse.mcneel.com/t/q-csharp-example-for-viewcapturesettings-and-capturetobitmap/204332

GitHub 본문 직접 열기는 검색 도구에서 실패하여, McNeel 포럼에 공개된 예제 발췌와 공식 API 문서로 캡처 경로를 확인했습니다. 저장소 전체를 복제하거나 분석한 것은 아닙니다. 크롭·검증·저장 처리는 이 도구를 위해 작성했습니다.


## 1.1 호환성 보강

- System.Drawing 어셈블리를 명시적으로 로드합니다.
- Marshal.Copy에 넘기는 배열을 명시적인 .NET Byte[]로 생성합니다.
- 실행 시 Rhino 버전과 IronPython 버전을 출력합니다.
- 캡처 전에 실제 System.Drawing으로 빈 알파, 알파 1의 흰색 픽셀, 불투명 검정 픽셀, PNG 인코딩/디코딩 및 LockBits/Marshal.Copy 경로를 자체 검사합니다. 이 검사는 사용자 문서를 변경하지 않습니다.
- 이 사전 검사는 Rhino 뷰포트 렌더링의 정상 동작을 보증하지 않습니다. 캡처 후 기존의 PNG 재읽기/1px 여백 검증도 수행합니다.

| 필수 실기 확인 | Rhino 6 Windows | Rhino 8 Windows |
| --- | --- | --- |
| RunPythonScript 실행 및 .NET 자체 검사 | 미실행 | 미실행 |
| Shaded 뷰 투명 PNG + 1px 여백 | 미실행 | 미실행 |
| 흰색/검정/반투명 모델과 얇은 선 | 미실행 | 미실행 |
| 선택 강조 해제 및 복원 | 미실행 | 미실행 |
| 한글 경로 저장 및 반복 실행 | 미실행 | 미실행 |

두 버전의 실기 확인을 모두 통과해야 정상 작동 검증 완료로 판정합니다.

Rhino 8의 IronPython 지원 공식 설명: https://developer.rhino3d.com/guides/rhinopython/python-3-in-rhino/

## 설치 도구 1.2.1 검증

- 클라우드 모의 Rhino 호스트에서 9개 자동 테스트 통과: 단일 파일 설치·재설치·이동, 업데이트 실패 복원, 첫 설치 실패 정리, 누락 패키지 보호, Rhino 6/8 설치 경로 분리, RUI 매크로·아이콘 참조와 PNG 크기·투명도.
- RUI XML 구조 검사와 Python 문법 검사를 수행했습니다. 이 검사는 Rhino 자체의 RUI 로딩 검증을 대신하지 않습니다.
- Windows Rhino 6/8에서 설치 실행, 로고 표시, 버튼 클릭 캡처, 재시작 후 유지, 한글 사용자 경로는 **미실행**입니다. 캡처 엔진의 실기 검증 상태도 위 표와 같습니다.

### 개발자 빌드

Python 3에서 `python -m pip install CairoSVG==2.8.2 Pillow` 후 저장소 루트에서 `python tools/build_release.py`를 실행합니다. SVG에서 PNG/RUI, 루트 단일 설치 파일, `dist`의 설치 파일과 소스 ZIP을 재생성합니다. CairoSVG는 빌드 머신에 Cairo 라이브러리가 필요합니다. 최종 사용자 Rhino에는 필요하지 않습니다.

테스트: `python -m unittest discover -s tests -v`

### 구현 근거

- McNeel 공식 [rhinoscriptsyntax toolbar.py](https://github.com/mcneel/rhinoscriptsyntax/blob/master/Scripts/rhinoscript/toolbar.py)의 `OpenToolbarCollection`, `CloseToolbarCollection`, `ShowToolbar`, `IsToolbarVisible` 구현을 확인했습니다. 이 함수들은 RhinoApp.ToolbarFiles와 그룹 Visible을 사용합니다.
- McNeel 공식 [SampleShowToolbarGroup.py](https://github.com/mcneel/rhino-developer-samples/blob/master/rhinopython/SampleShowToolbarGroup.py)의 컬렉션 로드 → 그룹 조회 → 표시 흐름을 확인했습니다(MIT).
- [COMPAS v0.19.3의 rui.py](https://github.com/compas-dev/compas/blob/v0.19.3/src/compas_rhino/ui/rui.py)에서 RhinoUI 2.0의 매크로·툴바·그룹과 PNG 비트맵 슬롯 구조를 확인했습니다. 생성기는 이 파일의 코드를 복제하지 않고 프로젝트에 맞게 작성했습니다. COMPAS의 MIT 라이선스는 저장소 LICENSE에서 확인했습니다.

### 1.2.1 설치 오류 수정

기존 툴바를 닫지 못했다는 오류를 줄이기 위해, 설치할 RUI와 기존 파일이 같으면 열린 툴바를 재사용합니다. RUI 교체 시에는 이름 기반 `CloseToolbarCollection` 대신 공식 `RhinoApp.ToolbarFiles.FindByPath`로 찾은 파일을 닫고, 반환값 대신 경로 재조회로 닫힘 여부를 확인합니다. 실제로 파일이 계속 열려 있으면 기존 파일을 덮어쓰지 않고 재시작 안내를 표시합니다.

추가 모의 테스트: 닫기가 거부되는 환경의 동일 툴바 재설치, 반환값 없는 닫기 후 교체, 실제 닫기 거부 시 기존 파일 보존. Windows Rhino 8에서 사용자가 보고한 오류에 대응한 수정이며 실제 Rhino 재실행 검증은 아직 필요합니다.
