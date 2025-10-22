(function () {
    const handledCodes = new Set([
        "ArrowUp",
        "ArrowDown",
        "ArrowLeft",
        "ArrowRight",
        "KeyW",
        "KeyA",
        "KeyS",
        "KeyD",
        "Space",
        "KeyP"
    ]);

    let dotNetRef = null;

    function onKeyDown(event) {
        if (!dotNetRef || !handledCodes.has(event.code)) {
            return;
        }

        event.preventDefault();
        dotNetRef.invokeMethodAsync("HandleKeyAsync", event.code);
    }

    window.ArcadeSnake = {
        register(reference) {
            dotNetRef = reference;
            window.addEventListener("keydown", onKeyDown);
        },
        unregister() {
            window.removeEventListener("keydown", onKeyDown);
            dotNetRef = null;
        },
        getHighScore() {
            const stored = window.localStorage.getItem("arcade_snake_high_score");
            if (!stored) {
                return null;
            }

            const value = parseInt(stored, 10);
            return Number.isNaN(value) ? null : value;
        },
        setHighScore(value) {
            if (typeof value !== "number" || Number.isNaN(value) || value < 0) {
                return;
            }

            window.localStorage.setItem("arcade_snake_high_score", value.toString());
        }
    };
})();
