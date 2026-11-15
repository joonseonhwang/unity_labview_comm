# 주수별 체중 가이드: VS Code 인계 문서

임신 주수별 권장 체중 범위를 보여주고 체중 기록을 남기는 개인용 PWA. 갤럭시 폰 홈 화면에 설치해서 쓴다.
빌드 도구 없이 바닐라 HTML/CSS/JS 한 파일로 되어 있다.

- **배포 주소**: https://joonseonhwang.github.io/unity_labview_comm/
- **현재 버전**: v1.2.0 (`index.html`, `sw.js`의 `APP_VERSION`)
- **원칙**: 기록은 폰 브라우저(IndexedDB)에만 저장하고 서버로 보내지 않음. 공개 저장소라서 **개인 데이터(체중, 예정일 등)를 코드나 커밋에 절대 넣지 않는다.**

---

## 1. 저장소·브랜치 구성

| 저장소 / 브랜치 | 내용 | 주소 |
|---|---|---|
| `joonseonhwang/unity_labview_comm` **`gh-pages`** | **이 앱 (작업 대상)** | `/unity_labview_comm/` |
| `joonseonhwang/unity_labview_comm` `main` | 예전 Unity–LabVIEW 프로젝트. **건드리지 않음** | 없음 |
| `joonseonhwang/joonseonhwang` `gh-pages` | People Book 앱(루트) + `baekdo-weight/` 옛 주소 이동 안내 | `/joonseonhwang/` |
| `joonseonhwang/joonseonhwang` `main` | GitHub 프로필 README. 건드리지 않음 | 없음 |

GitHub Pages가 `gh-pages` 브랜치 루트를 그대로 배포한다. push하고 30~60초 지나면 반영된다.

### 왜 `unity_labview_comm`에 있나
처음에는 `joonseonhwang/joonseonhwang` 아래 `/joonseonhwang/baekdo-weight/`에 배포했다. 그런데 폰에 이미 깔린 People Book의 범위(scope)가 `/joonseonhwang/`라서, 링크를 누르면 안드로이드가 **People Book 앱 안에서** 열어버렸다(설치 버튼도 안 뜸).
그래서 People Book 범위 밖의 경로로 옮겼다. 새 저장소는 Claude 쪽에서 만들 수 없어서, 안 쓰는 기존 공개 저장소에 `gh-pages` 브랜치만 추가했다.

> **규칙: 같은 도메인(`joonseonhwang.github.io`)에 앱을 추가할 때는 다른 앱 경로의 하위 폴더에 넣지 말 것.**

나중에 `baekdo` 같은 전용 저장소로 옮기려면 이 브랜치 내용을 그대로 복사해서 push하면 된다. 출처(origin)가 같은 `joonseonhwang.github.io`라서 **IndexedDB 기록은 경로를 옮겨도 그대로 따라온다.** 옛 주소에는 `baekdo-weight/` 이동 안내 페이지처럼 리다이렉트를 남길 것.

---

## 2. 파일 구성 (`gh-pages` 브랜치 루트)

```
index.html            앱 전체 (HTML + CSS + JS 한 파일, 약 1,000줄)
sw.js                 서비스워커: 오프라인 캐시
manifest.webmanifest  PWA 설정 (id/start_url/scope 모두 "./")
icons/                180·192·512·maskable-512 PNG (복숭아 = 태명 '백도')
.nojekyll             Pages가 Jekyll 처리 안 하게
tests/                갤럭시 에뮬레이션 테스트 (가짜 데이터)
HANDOFF.md            이 문서
CLAUDE.md             Claude Code용 요약 규칙
```

---

## 3. 로컬 실행

```bash
git clone -b gh-pages https://github.com/joonseonhwang/unity_labview_comm baekdo
cd baekdo
python -m http.server 8000
# 브라우저: http://localhost:8000/
```

- 서비스워커는 `localhost` 또는 https에서만 동작한다. `file://`로 열면 안 된다.
- VS Code의 Live Server 확장도 쓸 수 있다. 다만 SW 캐시 때문에 수정이 안 보이면 개발자도구 → Application → Service Workers에서 **Update on reload**를 켠다.
- 폰 화면 확인: 개발자도구 기기 모드에서 360×780 (갤럭시 S24).

---

## 4. 코드 구조 (`index.html` 내 `<script>`)

IIFE 하나 안에 다 들어 있다. 줄 번호는 v1.2.0 기준이라 대략적인 위치다.

### 상태와 저장
| 이름 | 역할 |
|---|---|
| `state` | `{ base, height, due, std, logs:[{w,k}], updatedAt }`. base는 임신 전 체중(문자열), w는 소수 주수(15주 4일 = 15.571), k는 kg |
| `idb()` / `idbGet` / `idbPut` | IndexedDB `baekdo-weight` DB, `meta` 스토어(keyPath `id`). 키는 `state`, `lastBackup` |
| `saveLocal()` | IndexedDB와 localStorage(`preg-weight-v1`, 보조) 둘 다에 저장 |
| `save()` | `updatedAt` 갱신 후 저장. **상태를 바꿨으면 반드시 `save(); render();`** |
| 시작 시 로드 | localStorage로 먼저 그리고, IndexedDB가 더 최신이면 덮어쓰고 다시 그림 (`idbGet('state')` 부분) |

### 백업 / 복원
- `exportBackup()`: JSON 파일. 공유 시트(`navigator.share`)가 되면 공유, 안 되면 다운로드. 파일명은 `체중기록_백업_YYYYMMDD.json`
- `importBackup(file)` → 하단 시트에서 **합치기 / 전체 바꾸기** 선택 → `importData(d, mode)`
  - 합치기: 같은 날(주수×7 반올림이 같은) 기록은 `updatedAt`이 더 최신인 쪽을 우선
- `markBackup()`: 마지막 백업 시각 저장. 14일이 지나면 `#bkBanner` 표시
- 백업 형식:
```json
{ "app":"baekdo-weight", "version":1, "exportedAt":"ISO", "updatedAt":0,
  "base":"55", "height":"162", "due":"YYYY-MM-DD", "std":"auto",
  "logs":[{ "w":12.4286, "k":55.6 }] }
```

### 권장 범위 계산
- `STDS`: 기준별 BMI 구간(`cuts`), 이름, 총 증가량(`totals`), 1분기 증가량(`first`)
  - `jp2021` 일본산부인과학회 2021 · `cn2022` 중국 국가표준 2022 · `iom2009` 미국 IOM 2009
- `state.std === 'auto'`이면 `autoKey()`가 **출산 무렵 상한이 가장 낮은 기준**을 고른다
- `range(w, c)`: 0~13주는 0→`first`까지 선형, 13~40주는 `first`→`totals`까지 선형. 반환값은 임신 전 대비 증가량 `[하한, 상한]`
- `statusOf(k, w, base, c)`: 권장보다 적음 / 권장 범위 / 권장보다 많음 (±0.05kg 여유)

### 무게 구성
- `FETAL`: 주수별 태아 체중(kg), Hadlock 50백분위
- `HY`: 0/10/20/30/40주의 태반+양수, 자궁+유방+혈액+체액 (Hytten)
- `comp(w)`, `pregKg(w)`: 임신으로 늘어난 무게. **순수 몸무게 = 기록 체중 − pregKg(w)**

### 날짜
- `gaDays()`: 출산 예정일(`state.due`) 기준 오늘 임신 일수 (280 − 남은 날)
- 임신 시작일(LMP) 입력 = 예정일 − 280일. 서로 자동 계산
- 기록 입력은 `주.일` 표기 (`15.4` = 15주 4일, 일은 0~6)

### 화면
| 함수 | 그리는 것 |
|---|---|
| `render()` | 전체 갱신 진입점 |
| `drawStatic()` | SVG 그래프 배경·범위 밴드·기록 점. `staticKey`가 같으면 건너뜀 → **데이터 구조를 바꾸면 `staticKey = ''`로 강제 갱신** |
| `moveCursor` / `drawCursor` | 선택 주수 커서 (requestAnimationFrame) |
| `drawComp()` | 무게 구성 막대 |
| `drawList()` | 체중 기록 목록 (`listKey`로 캐시) |
| `drawTable()` | 주요 주수별 표 |
| `summaryText()` / `makeSummaryImage()` | 카톡용 텍스트 요약 / 1080×1350 PNG |
| `sheet(title, msg, buttons)` | confirm 대신 쓰는 하단 시트. Promise로 선택값 반환 |

### 테스트용 전역
`window.X = { state, importData, snapshot, APP_VERSION }`. Playwright나 개발자도구 콘솔에서 쓴다.

---

## 5. 수정 → 배포 순서

1. `index.html` 수정
2. **버전 올리기 (꼭)**: `index.html`의 `APP_VERSION`, 하단 `v1.x.x ·` 문구, `sw.js`의 `APP_VERSION`을 모두 같은 값으로
   - `sw.js` 내용이 바뀌어야 폰이 새 서비스워커를 받는다. 안 올리면 옛 화면 파일이 캐시에 남는다
3. 로컬 확인 + `tests/` 테스트 실행
4. 커밋하고 `git push origin gh-pages`
5. 30~60초 뒤 배포 주소에서 하단 버전 문구로 반영 확인
6. 폰: 앱을 완전히 닫았다 다시 열면 새 버전. 화면은 네트워크 우선이라 보통 바로 반영된다

### 커밋 작성자
Claude Code로 커밋할 때는 커미터가 `Claude <noreply@anthropic.com>`이어야 GitHub에서 Verified로 보인다(세션 훅이 검사함). 직접 커밋할 때는 본인 계정으로 하면 된다.

---

## 6. 테스트

```bash
pip install playwright && python -m playwright install chromium
python tests/test_galaxy.py          # 저장소 루트에서 실행. 자체적으로 localhost:8799 서버를 띄움
```
갤럭시 S24 에뮬레이션(360×780, DPR 3, 터치, 안드로이드 UA, ko-KR, Asia/Seoul)으로 다음을 확인한다:
빈 화면 → 백업 불러오기 → 기록 추가/삭제 → 새로고침 후 유지(IndexedDB) → 합치기 중복 없음 → 백업 다운로드 내용 → 전체 삭제 후 복원 → 서비스워커 범위 → 오프라인 실행 → 가로 넘침 없음 → 콘솔 오류 없음

`tests/fixture_backup.json`은 **가짜 데이터**다. 실제 기록 파일을 저장소에 넣지 말 것.

---

## 7. 주의할 점

- **외부 CDN·폰트 금지**: 오프라인 동작을 보장해야 한다. 폰트는 시스템 폰트 사용
- **입력창 font-size 16px 이상**: 갤럭시/iOS에서 포커스할 때 확대되는 문제 방지
- `alert`/`confirm`/`prompt` 쓰지 말고 `sheet()` 사용
- 사용자 입력을 `innerHTML`로 넣을 때는 숫자만 쓰거나 escape할 것 (지금은 숫자만 출력)
- 갤럭시에서 설치하는 방법: Claude·카톡 미니 브라우저에는 설치 메뉴가 없다 → 메뉴(⋮) **Chrome에서 열기** → **홈 화면에 추가** → **설치**
- 기록은 폰에만 있다. 앱 데이터를 지우면 사라지니 백업 파일을 카톡 '나에게 보내기'로 보관

---

## 8. 관련 버전

- **Claude 아티팩트판**: https://claude.ai/artifact/LBdYEw85ejgwngZAXhfyKB (claude.ai 로그인 기반 클라우드 동기화, 이 PWA와는 데이터가 따로임)
- **People Book**: https://joonseonhwang.github.io/joonseonhwang/ (같은 방식의 인맥 관리 PWA. `sw.js`가 하위 폴더 요청은 가로채지 않도록 v1.0.2에서 수정함)

## 9. 다음에 해볼 만한 것 (미정)

- 아내 폰과 기록 동기화: Firebase/Supabase 붙이기 (Google 계정 연결 필요)
- 전용 저장소(`baekdo`)로 이전해서 주소 정리
- 주간 체중 측정 알림 (PWA 알림은 서버 푸시가 필요해서, 하려면 네이티브 앱이 더 적합)
