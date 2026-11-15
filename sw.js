/* 주수별 체중 가이드 서비스워커 — 앱 화면 파일만 캐시. 체중 기록은 IndexedDB에만 있음 */
const APP_VERSION = '1.2.0';
const CACHE = 'baekdo-weight-v' + APP_VERSION;
const ASSETS = ['./', './index.html', './manifest.webmanifest',
  './icons/icon-180.png', './icons/icon-192.png', './icons/icon-512.png', './icons/icon-maskable-512.png'];

self.addEventListener('install', e => {
  e.waitUntil(caches.open(CACHE).then(c => c.addAll(ASSETS)).then(() => self.skipWaiting()));
});
self.addEventListener('activate', e => {
  e.waitUntil(caches.keys().then(ks => Promise.all(ks.filter(k => k.startsWith('baekdo-weight-') && k !== CACHE).map(k => caches.delete(k))))
    .then(() => self.clients.claim()));
});
self.addEventListener('fetch', e => {
  const req = e.request;
  if (req.method !== 'GET' || new URL(req.url).origin !== location.origin) return;
  if (req.mode === 'navigate') {
    // 화면: 네트워크 우선(업데이트 반영), 실패하면 캐시 → 오프라인 동작
    e.respondWith(fetch(req).then(res => {
      if (res.ok) { const copy = res.clone(); caches.open(CACHE).then(c => c.put('./index.html', copy)); }
      return res;
    }).catch(() => caches.match('./index.html', { cacheName: CACHE })));
    return;
  }
  // 정적 파일: 캐시 우선, 뒤에서 갱신
  e.respondWith(caches.match(req, { cacheName: CACHE }).then(hit => {
    const net = fetch(req).then(res => {
      if (res.ok) { const copy = res.clone(); caches.open(CACHE).then(c => c.put(req, copy)); }
      return res;
    }).catch(() => hit);
    return hit || net;
  }));
});
