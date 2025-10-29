const handled = new Set([
    "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight",
    "KeyW", "KeyA", "KeyS", "KeyD", "Space", "KeyP"
]);

let dotnet = null;
let reg = false;

function onKeyDown(e) {
    if (!dotnet) return;
    const code = e.code || "";
    if (!handled.has(code)) return;
    e.preventDefault(); // verhindert Scrollen/Space
    dotnet.invokeMethodAsync("HandleKeyAsync", code).catch(() => { });
}

export function register(reference) {
    dotnet = reference;
    if (!reg) {
        window.addEventListener("keydown", onKeyDown, { passive: false });
        reg = true;
    }
}

export function unregister() {
    if (reg) {
        window.removeEventListener("keydown", onKeyDown);
        reg = false;
    }
    dotnet = null;
}

export function getHighScore() {
    try {
        const v = window.localStorage.getItem("arcade_snake_high_score");
        if (!v) return null;
        const n = parseInt(v, 10);
        return Number.isNaN(n) ? null : n;
    } catch { return null; }
}

export function setHighScore(value) {
    try {
        if (typeof value !== "number" || Number.isNaN(value) || value < 0) return;
        window.localStorage.setItem("arcade_snake_high_score", String(value));
    } catch {/* ignore */ }
}
