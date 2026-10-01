// Página do card: compartilha a imagem pelo menu do próprio celular (Stories, WhatsApp...) ou baixa o arquivo.
(function () {
  var body = document.body;
  var shareButton = document.getElementById('share');
  var downloadLink = document.getElementById('download');
  var token = new URLSearchParams(location.search).get('t') || '';
  var cardFile = null;

  // Eventos anônimos ligados ao token do QR. Sem endereço configurado (data-events-url), nada é enviado.
  function track(type) {
    var url = body.dataset.eventsUrl;
    if (!url) return;
    var payload = JSON.stringify({ type: type, token: token, artist: body.dataset.artist, song: body.dataset.song });
    if (navigator.sendBeacon) navigator.sendBeacon(url, new Blob([payload], { type: 'application/json' }));
    else fetch(url, { method: 'POST', body: payload, headers: { 'Content-Type': 'application/json' }, keepalive: true });
  }

  function canShareFile() {
    return cardFile !== null && typeof navigator.canShare === 'function' && navigator.canShare({ files: [cardFile] });
  }

  // O arquivo é baixado antes do toque: no iPhone, o compartilhamento só abre se for chamado direto no toque.
  fetch(downloadLink.getAttribute('href'))
    .then(function (response) { return response.blob(); })
    .then(function (blob) {
      cardFile = new File([blob], body.dataset.fileName, { type: blob.type || 'image/jpeg' });
      if (canShareFile()) {
        shareButton.hidden = false;
        downloadLink.classList.remove('primary');
        downloadLink.classList.add('secondary');
      }
    })
    .catch(function () { /* sem a imagem em memória, fica só o botão de baixar */ });

  shareButton.addEventListener('click', function () {
    if (!canShareFile()) {
      downloadLink.click();
      return;
    }
    // Só o arquivo, sem texto: com texto junto, alguns apps (o Instagram entre eles) não recebem a imagem.
    navigator.share({ files: [cardFile] })
      .then(function () { track('share'); })
      .catch(function (error) {
        if (error && error.name !== 'AbortError') downloadLink.click();
      });
  });

  downloadLink.addEventListener('click', function () { track('download'); });
  document.querySelectorAll('[data-track]').forEach(function (link) {
    link.addEventListener('click', function () { track(link.dataset.track); });
  });

  track('scan');
})();
