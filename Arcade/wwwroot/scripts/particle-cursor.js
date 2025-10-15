(() => {
    const root = document.documentElement;
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');

    if (prefersReducedMotion.matches) {
        root.style.setProperty('--cursor-x', '50vw');
        root.style.setProperty('--cursor-y', '50vh');
        return;
    }

    let targetX = window.innerWidth / 2;
    let targetY = window.innerHeight / 2;
    let currentX = targetX;
    let currentY = targetY;
    const parallaxStrength = 28;

    function updatePointerPosition(clientX, clientY) {
        targetX = clientX;
        targetY = clientY;
    }

    window.addEventListener('pointermove', (event) => {
        updatePointerPosition(event.clientX, event.clientY);
    });

    window.addEventListener('pointerleave', () => {
        updatePointerPosition(window.innerWidth / 2, window.innerHeight / 2);
    });

    window.addEventListener('resize', () => {
        targetX = Math.min(targetX, window.innerWidth);
        targetY = Math.min(targetY, window.innerHeight);
    });

    function animate() {
        currentX += (targetX - currentX) * 0.08;
        currentY += (targetY - currentY) * 0.08;

        const parallaxX = ((currentX / window.innerWidth) - 0.5) * parallaxStrength;
        const parallaxY = ((currentY / window.innerHeight) - 0.5) * parallaxStrength;

        root.style.setProperty('--cursor-x', `${currentX}px`);
        root.style.setProperty('--cursor-y', `${currentY}px`);
        root.style.setProperty('--cursor-parallax-x', `${parallaxX}px`);
        root.style.setProperty('--cursor-parallax-y', `${parallaxY}px`);

        requestAnimationFrame(animate);
    }

    animate();
})();
