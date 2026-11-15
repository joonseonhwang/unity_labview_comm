# 주수별 체중 가이드 (baekdo-weight PWA)

자세한 내용은 HANDOFF.md를 볼 것. 핵심 규칙:

- 이 브랜치(`gh-pages`)가 곧 배포본. push하면 https://joonseonhwang.github.io/unity_labview_comm/ 에 30~60초 뒤 반영
- `main` 브랜치는 무관한 옛 프로젝트라 건드리지 않음
- 앱은 `index.html` 한 파일 (바닐라 JS, 외부 CDN·폰트 금지). 기록은 IndexedDB `baekdo-weight`에만 저장, 서버 전송 없음
- 공개 저장소: 실제 체중·예정일 같은 개인 데이터는 코드·테스트·커밋에 넣지 않음 (테스트는 tests/fixture_backup.json 가짜 데이터)
- 수정하면 `index.html`의 `APP_VERSION`, 하단 버전 문구, `sw.js`의 `APP_VERSION`을 같이 올릴 것
- 배포 전 `python tests/test_galaxy.py` 전부 PASS 확인
- 같은 도메인의 다른 앱(People Book: /joonseonhwang/) 경로 아래에 이 앱을 두지 말 것 (설치가 People Book으로 빨려 들어감)
- confirm/alert 대신 `sheet()` 사용, 입력창 font-size 16px 이상
- 답변은 한국어, 캐주얼·간결하게
