window.survival = {
    canvas: null,
    ctx: null,
    dotNetRef: null,
    isInitialized: false,
    autoInitAttempts: 0,

    init() {
        console.log("🎮 [SURVIVAL] Starting initialization...");

        let attempts = 0;
        const maxAttempts = 10;

        const tryInit = () => {
            attempts++;
            console.log(`🔍 [SURVIVAL] Attempt ${attempts}/${maxAttempts}`);

            this.canvas = document.getElementById("survivalCanvas");

            if (!this.canvas) {
                console.warn(
                    `⚠️ [SURVIVAL] Canvas not found yet (attempt ${attempts})`
                );

                if (attempts < maxAttempts) {
                    setTimeout(tryInit, 500);
                } else {
                    console.error(
                        "❌ [SURVIVAL] Canvas not found after all attempts!"
                    );
                }
                return;
            }

            console.log("✅ [SURVIVAL] Canvas found!", this.canvas);

            try {
                this.ctx = this.canvas.getContext("2d");
                console.log("✅ [SURVIVAL] Context created");

                this.resize();
                console.log("✅ [SURVIVAL] Canvas resized");

                window.addEventListener("resize", () => this.resize());

                this.canvas.addEventListener("click", (e) => {
                    const rect = this.canvas.getBoundingClientRect();
                    const x =
                        (e.clientX - rect.left) *
                        (this.canvas.width / rect.width);
                    const y =
                        (e.clientY - rect.top) *
                        (this.canvas.height / rect.height);

                    console.log(`🖱️ [SURVIVAL] Click at ${x}, ${y}`);

                    if (this.dotNetRef) {
                        this.dotNetRef
                            .invokeMethodAsync("OnCanvasClick", x, y)
                            .catch((err) => {
                                console.error(
                                    "❌ [SURVIVAL] Click handler error:",
                                    err
                                );
                            });
                    } else {
                        console.warn(
                            "⚠️ [SURVIVAL] Click ignored - no .NET ref"
                        );
                    }
                });

                this.isInitialized = true;
                console.log("✅✅✅ [SURVIVAL] FULLY INITIALIZED!");

                this.testRender();
            } catch (err) {
                console.error(
                    "❌ [SURVIVAL] Error during initialization:",
                    err
                );
            }
        };

        tryInit();
    },

    testRender() {
        if (!this.ctx) return;

        console.log("🎨 [SURVIVAL] Test rendering...");
        const ctx = this.ctx;
        ctx.fillStyle = "#0a0a0a";
        ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);

        ctx.fillStyle = "#ffaa00";
        ctx.font = "30px Arial";
        ctx.textAlign = "center";
        ctx.fillText(
            "SURVIVAL READY",
            this.canvas.width / 2,
            this.canvas.height / 2
        );

        ctx.font = "16px Arial";
        ctx.fillStyle = "#888";
        ctx.fillText(
            "Waiting for game to start...",
            this.canvas.width / 2,
            this.canvas.height / 2 + 40
        );

        console.log("✅ [SURVIVAL] Test render complete");
    },

    resize() {
        if (!this.canvas) return;

        const container = this.canvas.parentElement;
        if (!container) {
            console.error("❌ [SURVIVAL] Canvas parent not found!");
            return;
        }

        const rect = container.getBoundingClientRect();
        this.canvas.width = rect.width;
        this.canvas.height = rect.height;

        console.log(
            `📐 [SURVIVAL] Canvas resized: ${rect.width}x${rect.height}`
        );
    },

    setDotNetRef(ref) {
        this.dotNetRef = ref;
        console.log("✅ [SURVIVAL] .NET reference set");
    },

    isReady() {
        const ready =
            this.isInitialized &&
            this.canvas !== null &&
            this.ctx !== null;
        console.log(`🔍 [SURVIVAL] isReady: ${ready}`);
        return ready;
    },

    render(data) {
        if (!this.ctx || !this.canvas) {
            console.warn("⚠️ [SURVIVAL] Canvas not ready for rendering");
            return;
        }

        const ctx = this.ctx;
        const w = this.canvas.width;
        const h = this.canvas.height;

        // Clear
        ctx.fillStyle = "#0a0a0a";
        ctx.fillRect(0, 0, w, h);

        // Grid
        ctx.strokeStyle = "rgba(255, 255, 255, 0.03)";
        ctx.lineWidth = 1;
        for (let x = 0; x < w; x += 50) {
            ctx.beginPath();
            ctx.moveTo(x, 0);
            ctx.lineTo(x, h);
            ctx.stroke();
        }
        for (let y = 0; y < h; y += 50) {
            ctx.beginPath();
            ctx.moveTo(0, y);
            ctx.lineTo(w, y);
            ctx.stroke();
        }

        // Core
        const core = data.core;
        ctx.fillStyle = "rgba(255, 170, 0, 0.2)";
        ctx.beginPath();
        ctx.arc(core.x, core.y, 50, 0, Math.PI * 2);
        ctx.fill();

        ctx.strokeStyle = "#ffaa00";
        ctx.lineWidth = 3;
        ctx.stroke();

        ctx.fillStyle = "#ffaa00";
        ctx.font = "bold 40px Arial";
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.fillText("🏛️", core.x, core.y);

        // Towers
        data.towers.forEach((tower) => {
            if (
                data.selectedTower !== null &&
                data.selectedTower !== undefined
            ) {
                ctx.strokeStyle = "rgba(255, 170, 0, 0.1)";
                ctx.lineWidth = 1;
                ctx.beginPath();
                ctx.arc(tower.x, tower.y, tower.range, 0, Math.PI * 2);
                ctx.stroke();
            }

            ctx.fillStyle = tower.heat > 80 ? "#ff4444" : "#333";
            ctx.beginPath();
            ctx.arc(tower.x, tower.y, 20, 0, Math.PI * 2);
            ctx.fill();

            ctx.strokeStyle = tower.heat > 80 ? "#ff8888" : "#666";
            ctx.lineWidth = 2;
            ctx.stroke();

            const icons = ["🔫", "💥", "⚡", "❄️", "🔥"];
            ctx.fillStyle = "#fff";
            ctx.font = "bold 20px Arial";
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.fillText(icons[tower.type] || "🔫", tower.x, tower.y);

            if (tower.level > 1) {
                ctx.fillStyle = "#ffaa00";
                ctx.font = "bold 10px Arial";
                ctx.fillText(tower.level, tower.x, tower.y + 22);
            }
        });

        data.enemies.forEach((enemy) => {
            const hpPercent = enemy.hp / enemy.maxHP;

            let color;
            if (enemy.isBoss) color = "#ff0000";
            else if (enemy.isCommander) color = "#ff00ff";
            else if (enemy.isLeech) color = "#00ff88";
            else if (enemy.isPhantom)
                color = enemy.phantomActive ? "#88ccff" : "#4488ff";
            else if (enemy.type === 0) color = "#ff4444"; 
            else if (enemy.type === 1) color = "#888"; 
            else if (enemy.type === 2) color = "#444"; 
            else color = "#ff8844"; 

            ctx.fillStyle = color;

            if (enemy.phantomActive) {
                ctx.globalAlpha = 0.5;
            }

            ctx.beginPath();
            ctx.arc(enemy.x, enemy.y, enemy.size, 0, Math.PI * 2);
            ctx.fill();

            ctx.globalAlpha = 1.0;

            if (enemy.isBoss) {
                ctx.strokeStyle = "#ff4444";
                ctx.lineWidth = 3;
                ctx.stroke();
            }

            if (enemy.isCommander) {
                ctx.strokeStyle = "#ff00ff";
                ctx.lineWidth = 2;
                ctx.setLineDash([5, 5]);
                ctx.beginPath();
                ctx.arc(enemy.x, enemy.y, enemy.size + 5, 0, Math.PI * 2);
                ctx.stroke();
                ctx.setLineDash([]);
            }

            if (enemy.isLeech) {
                ctx.strokeStyle = "#00ff88";
                ctx.lineWidth = 2;
                ctx.beginPath();
                ctx.arc(enemy.x, enemy.y, enemy.size + 3, 0, Math.PI * 2);
                ctx.stroke();
            }

            if (hpPercent < 1) {
                ctx.fillStyle = "rgba(0, 0, 0, 0.5)";
                ctx.fillRect(
                    enemy.x - enemy.size,
                    enemy.y - enemy.size - 8,
                    enemy.size * 2,
                    4
                );

                ctx.fillStyle =
                    hpPercent > 0.5
                        ? "#4f4"
                        : hpPercent > 0.25
                            ? "#ff4"
                            : "#f44";
                ctx.fillRect(
                    enemy.x - enemy.size,
                    enemy.y - enemy.size - 8,
                    enemy.size * 2 * hpPercent,
                    4
                );
            }
        });

        data.projectiles.forEach((proj) => {
            ctx.fillStyle =
                proj.type === 2
                    ? "#00ffff"
                    : proj.type === 3
                        ? "#88ccff"
                        : "#ffaa00";

            ctx.shadowBlur = 10;
            ctx.shadowColor = ctx.fillStyle;

            ctx.beginPath();
            ctx.arc(proj.x, proj.y, 4, 0, Math.PI * 2);
            ctx.fill();

            ctx.shadowBlur = 0;
        });

        ctx.fillStyle = "#888";
        ctx.font = "10px monospace";
        ctx.textAlign = "left";
        ctx.fillText(`Enemies: ${data.enemies.length}`, 10, 20);
    },
};

console.log("✅ [SURVIVAL] survival.js loaded - waiting for DOM...");

function autoInit() {
    console.log("🚀 [SURVIVAL] AUTO-INIT starting...");

    if (window.survival.autoInitAttempts > 5) {
        console.error(
            "❌ [SURVIVAL] AUTO-INIT failed after 5 attempts"
        );
        return;
    }

    window.survival.autoInitAttempts++;

    const canvas = document.getElementById("survivalCanvas");
    if (canvas) {
        console.log("✅ [SURVIVAL] Canvas found - starting auto-init");
        window.survival.init();
    } else {
        console.log(
            `⏳ [SURVIVAL] Canvas not found yet, retry in 500ms (attempt ${window.survival.autoInitAttempts})`
        );
        setTimeout(autoInit, 500);
    }
}

setTimeout(autoInit, 1000);