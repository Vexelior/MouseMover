setInterval(() => {
    const video = document.querySelector("video");
    if (video && !document.fullscreenElement) {
        video.requestFullscreen();
    }
}, 1000);

setInterval(() => {
    const event = new MouseEvent('mousemove', {
        view: window,
        bubbles: true,
        cancelable: true
    });
    document.dispatchEvent(event);
}, 1000);