mergeInto(LibraryManager.library, {
  // Initializes MediaPipe Hands + camera and streams landmarks to Unity.
  // Self-loads the MediaPipe CDN scripts if they aren't already on the page,
  // so it works under ANY index.html — including unityroom's own wrapper
  // (which ignores our custom WebGL template).
  MP_Init: function () {
    if (window.__mpInited) return;
    window.__mpInited = true;

    function ensureVideo() {
      var v = document.getElementById('cam-video');
      if (!v) {
        v = document.createElement('video');
        v.id = 'cam-video';
        v.setAttribute('playsinline', '');
        v.muted = true;
        v.style.display = 'none';
        document.body.appendChild(v);
      }
      return v;
    }

    function loadScript(src) {
      return new Promise(function (resolve, reject) {
        var s = document.createElement('script');
        s.src = src;
        s.onload = function () { resolve(); };
        s.onerror = function () { reject(new Error('failed to load ' + src)); };
        document.head.appendChild(s);
      });
    }

    function startMediaPipe() {
      var video = ensureVideo();
      if (typeof Hands === 'undefined' || typeof Camera === 'undefined') {
        if (typeof SendMessage === 'function')
          SendMessage('HandManager', 'OnCameraError', 'MediaPipe scripts unavailable');
        return;
      }
      var hands = new Hands({
        locateFile: function (f) { return 'https://cdn.jsdelivr.net/npm/@mediapipe/hands/' + f; }
      });
      hands.setOptions({
        maxNumHands: 1,
        modelComplexity: 1,
        minDetectionConfidence: 0.45,
        minTrackingConfidence: 0.4,
        selfieMode: true,
      });
      hands.onResults(function (r) {
        if (typeof SendMessage !== 'function') return;
        if (!r.multiHandLandmarks || !r.multiHandLandmarks.length) {
          SendMessage('HandManager', 'OnHandLost', '');
          return;
        }
        var lm = r.multiHandLandmarks[0];
        var s = '';
        for (var i = 0; i < lm.length; i++) {
          if (i) s += ',';
          s += lm[i].x.toFixed(4) + ',' + lm[i].y.toFixed(4) + ',' + lm[i].z.toFixed(4);
        }
        SendMessage('HandManager', 'OnHandResult', s);
      });
      var cam = new Camera(video, {
        onFrame: function () { return hands.send({ image: video }); },
        width: 640,
        height: 480,
      });
      cam.start().catch(function (e) {
        console.error('Camera start failed:', e);
        if (typeof SendMessage === 'function')
          SendMessage('HandManager', 'OnCameraError', (e && e.message) || 'camera failed');
      });
    }

    // Already present (our custom template / GitHub Pages / itch.io)? Start now.
    if (typeof Hands !== 'undefined' && typeof Camera !== 'undefined') {
      startMediaPipe();
      return;
    }
    // Otherwise inject the CDN scripts ourselves (unityroom case).
    loadScript('https://cdn.jsdelivr.net/npm/@mediapipe/camera_utils/camera_utils.js')
      .then(function () { return loadScript('https://cdn.jsdelivr.net/npm/@mediapipe/hands/hands.js'); })
      .then(function () { startMediaPipe(); })
      .catch(function (e) {
        console.error(e);
        if (typeof SendMessage === 'function')
          SendMessage('HandManager', 'OnCameraError', e.message || 'mediapipe load error');
      });
  }
});
