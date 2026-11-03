# BKID AutoCrop 1.1 — Rhino 6 / 8 공통판

Windows Rhino용 자동 투명 PNG 캡처 실행 스크립트입니다. 컴파일된 .rhp 플러그인이 아닌 버튼 등록형 Python 도구입니다.

## 실행 명령 등록 — Rhino 6 / 8 공통

1. ZIP 압축을 풀고 폴더를 고정 위치에 보관합니다.
2. Rhino 명령창에 `_RunPythonScript`를 입력합니다.
3. `Install_RunAutoCrop.py`를 선택해 한 번 실행합니다.
4. 이후에는 `RunAutoCrop` 입력 후 Enter를 누르면 캡처됩니다.

Rhino 6과 8에서는 각각 한 번 등록합니다. 같은 이름의 기존 별칭이 있으면 이 파일 경로로 갱신합니다. 폴더를 옮긴 경우 설치 스크립트를 다시 실행하십시오.

실행 명령은 공백 없는 `RunAutoCrop`입니다. Rhino 명령 별칭에는 공백을 사용할 수 없습니다. 버튼 표시 이름은 **Run AutoCrop**으로 지정할 수 있습니다. 등록은 사용자 PC에서 설치 스크립트를 실행해야 적용됩니다.

기본 저장 위치는 바탕화면의 `BKID_Captures`입니다. 두 버전 모두 IronPython 2.7의 RunPythonScript 경로를 사용하며, 별도 pip 설치가 필요 없습니다.

## 툴바 버튼 설정

버튼 표시 이름: **Run AutoCrop**

위 설치 후 버튼 매크로:

```
! RunAutoCrop
```

이 패키지는 명령 별칭을 등록합니다. 툴바 버튼 자체를 자동 생성하지는 않습니다.

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
