const handledCodes = new Set([
    "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight",
    "KeyW", "KeyA", "KeyS", "KeyD",
    "Space", "KeyP"
]);

let dotNetRef = null;
let isRegistered = false;

function onKeyDown(event) {
    if (!dotNetRef) return;

    const code = event.code || "";
    if (!handledCodes.has(code)) return;

    event.preventDefault();

    dotNetRef.invokeMethodAsync("HandleKeyAsync", code).catch(() => { });
}

export function register(reference) {
    dotNetRef = reference;
    if (!isRegistered) {
        window.addEventListener("keydown", onKeyDown, { passive: false });
        isRegistered = true;
    }
}

export function unregister() {
    if (isRegistered) {
        window.removeEventListener("keydown", onKeyDown);
        isRegistered = false;
    }
    dotNetRef = null;
}

export function getHighScore() {
    try {
        const stored = window.localStorage.getItem("arcade_snake_high_score");
        if (!stored) return null;
        const value = parseInt(stored, 10);
        return Number.isNaN(value) ? null : value;
    } catch {
        return null;
    }
}

export function setHighScore(value) {
    try {
        if (typeof value !== "number" || Number.isNaN(value) || value < 0) return;
        window.localStorage.setItem("arcade_snake_high_score", String(value));
    } catch {
    }
}
