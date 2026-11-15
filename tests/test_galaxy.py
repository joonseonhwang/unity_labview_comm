"""갤럭시 S24 에뮬레이션 테스트. 저장소 루트에서: python tests/test_galaxy.py"""
import asyncio, json, os, sys, threading, functools, http.server, socketserver
from playwright.async_api import async_playwright

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
FIX = os.path.join(ROOT, 'tests', 'fixture_backup.json')
PORT = 8799
URL = f'http://localhost:{PORT}/'

def serve():
    class Quiet(http.server.SimpleHTTPRequestHandler):
        def log_message(self, *a): pass
    h = functools.partial(Quiet, directory=ROOT)
    socketserver.TCPServer.allow_reuse_address = True
    srv = socketserver.TCPServer(('localhost', PORT), h)
    threading.Thread(target=srv.serve_forever, daemon=True).start()
    return srv

R = []
def ck(name, ok, extra=''):
    R.append(ok); print('PASS' if ok else 'FAIL', name, extra)

async def main():
    srv = serve()
    async with async_playwright() as p:
        b = await p.chromium.launch()
        ctx = await b.new_context(viewport={'width': 360, 'height': 780}, device_scale_factor=3,
            is_mobile=True, has_touch=True, locale='ko-KR', timezone_id='Asia/Seoul', accept_downloads=True,
            user_agent='Mozilla/5.0 (Linux; Android 14; SM-S921N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Mobile Safari/537.36')
        pg = await ctx.new_page(); errs = []
        pg.on('pageerror', lambda e: errs.append(str(e)))
        pg.on('console', lambda m: m.type == 'error' and errs.append(m.text))
        await pg.goto(URL); await pg.wait_for_timeout(1200)
        ck('빈 화면 안내', await pg.is_visible('#empty'))
        await pg.set_input_files('#bkIn', FIX); await pg.wait_for_timeout(400)
        await pg.click('.sheet button.primary'); await pg.wait_for_timeout(500)
        ck('백업 불러오기', await pg.locator('#list li').count() == 3)
        await pg.fill('#lw', '16.2'); await pg.fill('#lk', '57.1'); await pg.click('#add'); await pg.wait_for_timeout(300)
        ck('기록 추가', await pg.locator('#list li').count() == 4)
        await pg.reload(); await pg.wait_for_timeout(1200)
        ck('새로고침 후 유지', await pg.locator('#list li').count() == 4)
        await pg.locator('#list li .del').last.click(); await pg.wait_for_timeout(300)
        ck('기록 삭제', await pg.locator('#list li').count() == 3)
        await pg.fill('#lw', '16.2'); await pg.fill('#lk', '57.1'); await pg.click('#add'); await pg.wait_for_timeout(300)
        await pg.set_input_files('#bkIn', FIX); await pg.wait_for_timeout(400)
        await pg.click('.sheet button.primary'); await pg.wait_for_timeout(500)
        ck('합치기 중복 없음', await pg.locator('#list li').count() == 4)
        async with pg.expect_download() as dl:
            await pg.click('#bkOut')
        d = await dl.value; path = await d.path(); data = json.load(open(path, encoding='utf-8'))
        ck('백업 다운로드 내용', len(data['logs']) == 4, d.suggested_filename)
        await pg.evaluate("indexedDB.deleteDatabase('baekdo-weight'); localStorage.clear()")
        await pg.reload(); await pg.wait_for_timeout(1200)
        ck('전체 삭제 후 빈 화면', await pg.is_visible('#empty'))
        await pg.set_input_files('#bkIn', path); await pg.wait_for_timeout(400)
        await pg.click('.sheet button.primary'); await pg.wait_for_timeout(500)
        ck('백업으로 복원', await pg.locator('#list li').count() == 4)
        scope = await pg.evaluate('navigator.serviceWorker.ready.then(r => r.scope)')
        ck('서비스워커 활성', scope == URL, scope)
        await ctx.set_offline(True); await pg.reload(); await pg.wait_for_timeout(1200)
        ck('오프라인 실행', await pg.locator('#list li').count() == 4)
        await ctx.set_offline(False)
        ov = await pg.evaluate('document.documentElement.scrollWidth - innerWidth')
        ck('가로 넘침 없음', ov <= 0, str(ov))
        v1 = await pg.evaluate('X.APP_VERSION')
        sw = open(os.path.join(ROOT, 'sw.js'), encoding='utf-8').read()
        ck('index/sw 버전 일치', f"APP_VERSION = '{v1}'" in sw, v1)
        await pg.screenshot(path=os.path.join(ROOT, 'tests', 'last_run.png'))
        bad = [e for e in errs if 'ERR_INTERNET_DISCONNECTED' not in e and 'Failed to load resource' not in e]
        ck('콘솔 오류 없음', not bad, str(bad[:3]))
        await b.close()
    srv.shutdown()
    print(f'\n결과: {sum(R)} / {len(R)}')
    sys.exit(0 if all(R) else 1)

asyncio.run(main())
