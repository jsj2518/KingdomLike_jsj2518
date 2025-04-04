# KingdomLike_jsj2518
최종프로젝트 개인 작업물


# Purralxy: 퍼럭시

---

**🌌 장르:** Minimalist Strategy Game  
**📌 레퍼런스:** Kingdom: New Lands  
**🎯 핵심 목표:** 각 스테이지의 미션을 해결하며 보물을 찾는 여정  

---

## 📖 게임 소개
![de_gif_2](https://github.com/user-attachments/assets/57d48725-8c85-4c97-b66d-79b1a5c4aeb9)
### 게임 영상
[![Purralxy Gameplay](https://img.youtube.com/vi/V8SzJC-Xfd4/0.jpg)](https://youtu.be/V8SzJC-Xfd4?si=97A_iLTGehAqD8Wc)
> 🎥 **클릭하여 영상 시청**
### 게임 플레이
[🎮 WebGL로 플레이하기](https://snf-studio.itch.io/purralaxy)  
[🔥 Stove에서 다운로드](https://store.onstove.com/ko/games/4706)

---
## 🌌프로젝트 성과 
**⛳ 프로젝트 초반 목표 설정**
- **게임 빌드 후 Itch.io를 통한 베타 테스트 진행**

**🏆 현재 목표 달성 현황**
- **Stove SDK를 활용해 빌드 후 Stove Store에 등록 및 출시 완료**
    ![Image](https://github.com/user-attachments/assets/9871fbd9-e160-487c-8cf1-aba40e7d0f71)
- **Itch.io & Stove** 두 플랫폼을 통해 **베타 테스트 진행 중**

**📅 앞으로의 계획**
- **정식 버전 출시 준비**
- **Steam 배포 검토**
- **모바일 플랫폼 배포 검토**
---



## 🛠 주요 기술 스택

- **Finite State Machine (FSM)**  
  NPC와 튜토리얼 시스템의 상태를 관리하여 유연성과 확장성을 극대화.
- **Google Sheets Parsing**  
  데이터를 효율적으로 관리하고 ScriptableObject로 자동 변환.
- **Custom Editor Tools**  
  스테이지 테스트와 구글 시트 파싱 등 QA 작업 속도 개선.
- **UI Manager**  
  중앙 관리 시스템을 통해 UI 유지보수 및 성능 최적화.
- **최적화**  
  오브젝트 풀링과 Sprite Atlas를 활용하여 렌더링 비용 감소.

---

## 🚀 주요 기능

### 🎮 Gameplay
- **동적 씬 전환**  
  데이터를 기반으로 맵을 생성하여 다양한 플레이 경험 제공.
- **AI 시스템**  
  NPC의 명령 체계와 행동을 State Machine을 통해 구현.

### 🖌 Graphics
- **Shader와 Post Processing**  
  몰입감을 위한 Water Reflection Shader 및 Emission Shader 적용.
- **Sprite Atlas**  
  배치 최적화를 통해 성능 개선.

### 📈 관리 및 운영
- **깃 브랜치 전략**
  - `Main`: 안정화된 최신 버전  
  - `Dev`: 기능 통합 및 개발 중심  
  - `Feature`: 개별 기능 개발  
  - `Release`: 배포 준비 완료 상태  
  - `Hotfix`: 긴급 수정 대응  
- **간트 차트와 구글 시트**  
  프로젝트 관리 및 데이터 공유 효율화.

---

## 💡 주요 개선 사항

1. **게임 목표 명확화**
   - 게임의 배경과 동기를 설명하는 **인트로 컷신** 추가.
2. **튜토리얼 개선**
   - **말풍선 방식**으로 변경해 가독성 향상.
   - **스킵 기능** 추가 및 독립된 튜토리얼 씬 제공.
3. **트러블 슈팅**
   - **렌더링 순서 문제 해결:** Sorting Order 값 동적 관리.
   - **소리 문제 해결:** AudioSource의 거리 기반 설정 최적화.

---

## 🎨 주요 이미지

### Emission Shader 적용 전/후
| Before | After |
|--------|-------|
| ![Image](https://github.com/user-attachments/assets/c43a025d-aaf9-46dd-9b16-babe73e58709) | ![Image](https://github.com/user-attachments/assets/5a8a9c1c-dc48-47e6-869b-0240a32381ce) |

### Water Reflection Shader 적용 예시
![Image](https://github.com/user-attachments/assets/5799bfa6-7fe1-43d8-8f52-690f536d8126)

---

## 📅 향후 계획

- 정식 버전 출시 준비
- **Steam 및 모바일 플랫폼** 배포 검토

---

## 🧑‍💻 팀원 소개

- 프로젝트 관련 문의: [Contact Us](mailto:contact@purralxy.com)

[**목차로 돌아가기**](https://www.notion.so/Purralxy-17c87adbacba801d9b22e08d3367c192?pvs=21)
