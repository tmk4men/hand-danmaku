mergeInto(LibraryManager.library, {
  MP_Init: function () {
    if (window.__mpInited) return;
    window.__mpInited = true;

    var video = document.getElementById('cam-video');
    if (!video) {
      video = document.createElement('video');
      video.id = 'cam-video';
      video.playsInline = true;
      video.muted = true;
      video.style.display = 'none';
      document.body.appendChild(video);
    }

    if (typeof Hands === 'undefined') {
      console.error('MediaPipe Hands script not loaded. Check WebGL template.');
      return;
    }

    var hands = new Hands({
      locateFile: function (f) {
        return 'https://cdn.jsdelivr.net/npm/@mediapipe/hands/' + f;
      }
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
      // Pack 21 landmarks as a flat float CSV — cheaper to parse on Unity side
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
      SendMessage('HandManager', 'OnCameraError', e.message || 'unknown');
    });
  }
});
