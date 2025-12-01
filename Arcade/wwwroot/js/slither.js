console.log("slither.js loaded");

// ========== Konstanten ==========
const WORLD_WIDTH = 4000;
const WORLD_HEIGHT = 4000;
const GRID_SIZE = 100;

// Speed / Steering
const PLAYER_BASE_SPEED = 260;
const PLAYER_BOOST_MULTIPLIER = 2.3;
const BOT_BASE_SPEED = 230;
const PLAYER_TURN_RATE = 10; // je höher, desto snappier
const BOT_TURN_RATE = 6;

// ========== Vektor-Klasse ==========
class Vec2 {
    constructor(x = 0, y = 0) {
        this.x = x;
        this.y = y;
    }

    get length() {
        return Math.sqrt(this.x * this.x + this.y * this.y);
    }

    normalized() {
        const len = this.length;
        return len > 0 ? new Vec2(this.x / len, this.y / len) : new Vec2(0, 0);
    }

    add(v) { return new Vec2(this.x + v.x, this.y + v.y); }
    sub(v) { return new Vec2(this.x - v.x, this.y - v.y); }
    mul(s) { return new Vec2(this.x * s, this.y * s); }
}

// ========== Schlangen-Klasse ==========
class Snake {
    constructor(name, isPlayer = false) {
        this.id = Math.random();
        this.name = name;
        this.isPlayer = isPlayer;
        this.color = this.randomColor();
        this.headPos = new Vec2(
            Math.random() * WORLD_WIDTH,
            Math.random() * WORLD_HEIGHT
        );
        this.direction = new Vec2(Math.random() - 0.5, Math.random() - 0.5).normalized();

        this.baseSpeed = isPlayer ? PLAYER_BASE_SPEED : BOT_BASE_SPEED;
        this.speed = this.baseSpeed;

        this.radius = 12;
        this.segments = [];
        this.isDead = false;
        this.kills = 0;
        this.isBoosting = false;

        const spacing = this.radius * 1.8;
        for (let i = 0; i < 20; i++) {
            this.segments.push(this.headPos.sub(this.direction.mul(i * spacing)));
        }
    }

    randomColor() {
        const colors = [
            '#5cf0c8', '#ff6b6b', '#ffd166', '#4dabf7',
            '#b197fc', '#ff922b', '#51cf66', '#ff6b9d'
        ];
        return colors[Math.floor(Math.random() * colors.length)];
    }

    get length() {
        return this.segments.length;
    }

    get score() {
        return Math.floor(this.length * 10);
    }
}

// ========== Food-Klasse ==========
class Food {
    constructor(x, y, value = 1) {
        this.id = Math.random();
        this.pos = new Vec2(x, y);
        this.value = value;
        this.color = value > 5 ? '#ff6b6b' : '#ffd166';
        this.radius = value > 5 ? 8 : 5;
        this.glow = 0;
    }
}

// ========== Spiel-Engine ==========
class Game {
    constructor() {
        this.canvas = document.getElementById('gameCanvas');
        this.ctx = this.canvas.getContext('2d');
        this.snakes = [];
        this.food = [];
        this.player = null;
        this.camera = { x: 0, y: 0 };
        this.mousePos = new Vec2(0, 0);
        this.keys = {};
        this.startTime = 0;
        this.kills = 0;

        // FPS-Messung
        this._fps = 0;
        this._fpsLast = performance.now();
        this._fpsFrames = 0;

        this.resize();
        window.addEventListener('resize', () => this.resize());
    }

    resize() {
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
    }

    init(playerName) {
        this.snakes = [];
        this.food = [];
        this.kills = 0;
        this.startTime = Date.now();

        // Spieler
        this.player = new Snake(playerName, true);
        this.snakes.push(this.player);

        // Bots
        const botNames = [
            'Bot Alpha', 'Bot Beta', 'Bot Gamma', 'Bot Delta', 'Bot Epsilon',
            'Bot Zeta', 'Bot Eta', 'Bot Theta', 'Bot Iota', 'Bot Kappa',
            'Pro Gamer', 'Snake Master', 'Slither King', 'Speed Demon', 'Big Boss'
        ];
        for (let i = 0; i < 25; i++) {
            this.snakes.push(new Snake(botNames[i % botNames.length] + ' ' + (i + 1)));
        }

        // Food
        for (let i = 0; i < 800; i++) {
            this.spawnFood();
        }
    }

    spawnFood(pos = null, value = 1) {
        const p = pos || new Vec2(
            Math.random() * WORLD_WIDTH,
            Math.random() * WORLD_HEIGHT
        );
        this.food.push(new Food(p.x, p.y, value));
    }

    update(dt) {
        if (!this.player || this.player.isDead) return;

        // Spieler
        this.updatePlayer(this.player, dt);

        // Bots
        this.snakes.filter(s => !s.isPlayer && !s.isDead).forEach(bot => {
            this.updateBot(bot, dt);
        });

        // Bewegung
        this.snakes.filter(s => !s.isDead).forEach(s => {
            this.moveSnake(s, dt);
        });

        // Kollisionen
        this.handleFoodCollisions();
        this.handleSnakeCollisions();

        // Tote aufräumen
        this.snakes = this.snakes.filter(s => !s.isDead || s.segments.length > 0);

        // Kamera
        this.camera.x = this.player.headPos.x - this.canvas.width / 2;
        this.camera.y = this.player.headPos.y - this.canvas.height / 2;

        // HUD
        document.getElementById('score').textContent = 'Länge: ' + this.player.length;
        document.getElementById('kills').textContent = 'Kills: ' + this.kills;
        this.updateLeaderboard();
    }

    // ========= Player-Controller (snappier) =========
    updatePlayer(snake, dt) {
        const toMouse = this.mousePos.sub(snake.headPos);
        const targetDir = toMouse.normalized();
        const dist = toMouse.length;

        // schnelleres Einlenken
        const lerp = 1 - Math.exp(-PLAYER_TURN_RATE * dt);
        snake.direction = new Vec2(
            snake.direction.x + (targetDir.x - snake.direction.x) * lerp,
            snake.direction.y + (targetDir.y - snake.direction.y) * lerp
        ).normalized();

        // Basis-Speed abhängig von Distanz zur Maus
        const speedFactor = 0.7 + Math.min(dist / 800, 0.8); // 0.7 .. 1.5
        let targetSpeed = snake.baseSpeed * speedFactor;

        // Boost
        if (this.keys[' '] && snake.length > 25) {
            targetSpeed *= PLAYER_BOOST_MULTIPLIER;
            snake.isBoosting = true;

            if (Math.random() < 0.35) {
                const tail = snake.segments.pop();
                if (tail) this.spawnFood(tail, 3);
            }
        } else {
            snake.isBoosting = false;
        }

        // Speed smooth interpolieren
        const speedLerp = 1 - Math.exp(-6 * dt);
        snake.speed = snake.speed + (targetSpeed - snake.speed) * speedLerp;
    }

    // ========= smarte Bots =========
    updateBot(snake, dt) {
        let target = null;
        let minDist = 600;

        // 1) großes Food
        this.food.forEach(f => {
            if (f.value < 3) return;
            const dist = f.pos.sub(snake.headPos).length;
            if (dist < minDist) {
                minDist = dist;
                target = f.pos;
            }
        });

        // 2) sonst normales Food
        if (!target) {
            this.food.forEach(f => {
                const dist = f.pos.sub(snake.headPos).length;
                if (dist < minDist) {
                    minDist = dist;
                    target = f.pos;
                }
            });
        }

        // 3) größeren Schlangen ausweichen
        this.snakes.forEach(other => {
            if (other.id === snake.id || other.isDead) return;
            if (other.length > snake.length * 1.2) {
                const offset = other.headPos.sub(snake.headPos);
                const dist = offset.length;
                if (dist < 220) {
                    target = snake.headPos.sub(offset); // Flucht
                    snake.speed = snake.baseSpeed * 1.6;
                }
            }
        });

        if (target) {
            const dir = target.sub(snake.headPos).normalized();
            const lerp = 1 - Math.exp(-BOT_TURN_RATE * dt);
            snake.direction = new Vec2(
                snake.direction.x + (dir.x - snake.direction.x) * lerp,
                snake.direction.y + (dir.y - snake.direction.y) * lerp
            ).normalized();
        }

        // leichte Zufallsbewegung
        if (Math.random() < 0.015) {
            const angle = (Math.random() - 0.5) * 0.6;
            const cos = Math.cos(angle);
            const sin = Math.sin(angle);
            snake.direction = new Vec2(
                snake.direction.x * cos - snake.direction.y * sin,
                snake.direction.x * sin + snake.direction.y * cos
            ).normalized();
        }
    }

    moveSnake(snake, dt) {
        const newHead = snake.headPos.add(snake.direction.mul(snake.speed * dt));

        // Wrap Around
        newHead.x = (newHead.x + WORLD_WIDTH) % WORLD_WIDTH;
        newHead.y = (newHead.y + WORLD_HEIGHT) % WORLD_HEIGHT;

        snake.headPos = newHead;

        const spacing = snake.radius * 1.8;
        snake.segments[0] = newHead;

        for (let i = 1; i < snake.segments.length; i++) {
            const prev = snake.segments[i - 1];
            const cur = snake.segments[i];
            const delta = prev.sub(cur);

            if (delta.length > spacing) {
                snake.segments[i] = prev.sub(delta.normalized().mul(spacing));
            }
        }
    }

    handleFoodCollisions() {
        const eatRadius = 20;

        this.snakes.filter(s => !s.isDead).forEach(snake => {
            for (let i = this.food.length - 1; i >= 0; i--) {
                const f = this.food[i];
                if (f.pos.sub(snake.headPos).length <= eatRadius) {
                    this.food.splice(i, 1);
                    this.growSnake(snake, f.value);
                    this.spawnFood();
                }
            }
        });
    }

    growSnake(snake, value) {
        const extra = Math.floor(value * 2);
        const tail = snake.segments[snake.segments.length - 1];
        for (let i = 0; i < extra; i++) {
            snake.segments.push(new Vec2(tail.x, tail.y));
        }
    }

    handleSnakeCollisions() {
        const hitRadius = 15;

        this.snakes.filter(s => !s.isDead).forEach(snake => {
            this.snakes.filter(other => !other.isDead && other.id !== snake.id).forEach(other => {
                for (let i = 3; i < other.segments.length; i++) {
                    if (snake.headPos.sub(other.segments[i]).length < hitRadius) {
                        this.killSnake(snake);
                        if (snake.isPlayer) {
                            other.kills++;
                        } else if (other.isPlayer) {
                            this.kills++;
                        }
                        return;
                    }
                }
            });
        });
    }

    killSnake(snake) {
        snake.isDead = true;

        for (let i = 0; i < snake.segments.length; i += 3) {
            this.spawnFood(snake.segments[i], 8);
        }

        if (snake.isPlayer) {
            setTimeout(() => this.gameOver(), 500);
        }
    }

    gameOver() {
        const playTime = Math.floor((Date.now() - this.startTime) / 1000);
        document.getElementById('finalScore').textContent = 'Finale Länge: ' + this.player.length;
        document.getElementById('finalKills').textContent = 'Kills: ' + this.kills;
        document.getElementById('playTime').textContent = 'Spielzeit: ' + playTime + ' Sekunden';
        document.getElementById('gameOver').classList.add('visible');
    }

    updateLeaderboard() {
        const sorted = [...this.snakes]
            .filter(s => !s.isDead)
            .sort((a, b) => b.length - a.length)
            .slice(0, 10);

        const html = sorted.map((s, i) => {
            const cls = s.isPlayer ? 'player' : '';
            return `<div class="leaderboard-entry ${cls}">
                <span>${i + 1}. ${s.name}</span>
                <span>${s.score}</span>
            </div>`;
        }).join('');

        document.getElementById('leaderboardList').innerHTML = html;
    }

    render() {
        const ctx = this.ctx;
        const canvas = this.canvas;

        // Hintergrund
        ctx.setTransform(1, 0, 0, 1, 0, 0);
        ctx.fillStyle = '#2b2b2b';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        // Welt-Transform
        ctx.save();
        ctx.translate(-this.camera.x, -this.camera.y);

        // Grid
        ctx.strokeStyle = 'rgba(255, 255, 255, 0.1)';
        ctx.lineWidth = 1;

        const startX = Math.floor(this.camera.x / GRID_SIZE) * GRID_SIZE;
        const endX = startX + canvas.width + GRID_SIZE;
        const startY = Math.floor(this.camera.y / GRID_SIZE) * GRID_SIZE;
        const endY = startY + canvas.height + GRID_SIZE;

        for (let x = startX; x <= endX; x += GRID_SIZE) {
            ctx.beginPath();
            ctx.moveTo(x, 0);
            ctx.lineTo(x, WORLD_HEIGHT);
            ctx.stroke();
        }

        for (let y = startY; y <= endY; y += GRID_SIZE) {
            ctx.beginPath();
            ctx.moveTo(0, y);
            ctx.lineTo(WORLD_WIDTH, y);
            ctx.stroke();
        }

        // Food
        this.food.forEach(f => {
            f.glow = (f.glow + 0.1) % (Math.PI * 2);
            const glowSize = f.radius + Math.sin(f.glow) * 2;

            ctx.fillStyle = f.color;
            ctx.shadowBlur = 15;
            ctx.shadowColor = f.color;
            ctx.beginPath();
            ctx.arc(f.pos.x, f.pos.y, glowSize, 0, Math.PI * 2);
            ctx.fill();
            ctx.shadowBlur = 0;
        });

        // Schlangen
        const sorted = [...this.snakes].filter(s => !s.isDead).sort((a, b) => a.length - b.length);

        sorted.forEach(snake => {
            ctx.strokeStyle = snake.color;
            ctx.lineWidth = snake.radius * 2;
            ctx.lineCap = 'round';
            ctx.lineJoin = 'round';

            if (snake.isBoosting) {
                ctx.shadowBlur = 20;
                ctx.shadowColor = snake.color;
            }

            ctx.beginPath();
            snake.segments.forEach((seg, i) => {
                if (i === 0) ctx.moveTo(seg.x, seg.y);
                else ctx.lineTo(seg.x, seg.y);
            });
            ctx.stroke();
            ctx.shadowBlur = 0;

            // Kopf
            ctx.fillStyle = 'rgba(255, 255, 255, 0.4)';
            ctx.beginPath();
            ctx.arc(snake.headPos.x, snake.headPos.y, snake.radius * 1.4, 0, Math.PI * 2);
            ctx.fill();

            const eyeOffset = snake.radius * 0.6;
            const angle = Math.atan2(snake.direction.y, snake.direction.x);
            const perpX = -Math.sin(angle) * eyeOffset;
            const perpY = Math.cos(angle) * eyeOffset;

            // Augen
            ctx.fillStyle = '#fff';
            ctx.beginPath();
            ctx.arc(snake.headPos.x + perpX, snake.headPos.y + perpY, 5, 0, Math.PI * 2);
            ctx.fill();
            ctx.beginPath();
            ctx.arc(snake.headPos.x - perpX, snake.headPos.y - perpY, 5, 0, Math.PI * 2);
            ctx.fill();

            // Pupillen
            ctx.fillStyle = '#000';
            ctx.beginPath();
            ctx.arc(
                snake.headPos.x + perpX + snake.direction.x * 2,
                snake.headPos.y + perpY + snake.direction.y * 2,
                3, 0, Math.PI * 2
            );
            ctx.fill();
            ctx.beginPath();
            ctx.arc(
                snake.headPos.x - perpX + snake.direction.x * 2,
                snake.headPos.y - perpY + snake.direction.y * 2,
                3, 0, Math.PI * 2
            );
            ctx.fill();

            // Name
            ctx.fillStyle = '#fff';
            ctx.font = 'bold 18px Arial';
            ctx.textAlign = 'center';
            ctx.strokeStyle = '#000';
            ctx.lineWidth = 3;
            ctx.strokeText(snake.name, snake.headPos.x, snake.headPos.y - snake.radius - 15);
            ctx.fillText(snake.name, snake.headPos.x, snake.headPos.y - snake.radius - 15);
        });

        ctx.restore();

        // Minimap
        this.renderMinimap();

        // FPS anzeigen (oben links)
        const now = performance.now();
        this._fpsFrames++;
        if (now - this._fpsLast >= 1000) {
            this._fps = this._fpsFrames;
            this._fpsFrames = 0;
            this._fpsLast = now;
        }

        ctx.save();
        ctx.setTransform(1, 0, 0, 1, 0, 0);
        ctx.fillStyle = 'white';
        ctx.font = '14px Arial';
        ctx.fillText(`FPS: ${this._fps}`, 10, 20);
        ctx.restore();
    }

    renderMinimap() {
        const minimap = document.getElementById('minimap');
        const rect = minimap.getBoundingClientRect();
        const tmpCanvas = document.createElement('canvas');
        tmpCanvas.width = rect.width;
        tmpCanvas.height = rect.height;
        const ctx = tmpCanvas.getContext('2d');

        const scale = rect.width / WORLD_WIDTH;

        ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
        ctx.fillRect(0, 0, rect.width, rect.height);

        this.snakes.filter(s => !s.isDead).forEach(snake => {
            ctx.fillStyle = snake.isPlayer ? '#5cf0c8' : '#fff';
            ctx.beginPath();
            ctx.arc(
                snake.headPos.x * scale,
                snake.headPos.y * scale,
                snake.isPlayer ? 4 : 2,
                0, Math.PI * 2
            );
            ctx.fill();
        });

        minimap.style.backgroundImage = `url(${tmpCanvas.toDataURL()})`;
        minimap.style.backgroundSize = 'cover';
    }

    // 60-FPS Fixed-Timestep Loop
    start() {
        const STEP = 1 / 60;
        let last = performance.now();
        let accumulator = 0;

        const loop = (now) => {
            let frameTime = (now - last) / 1000;
            if (frameTime > 0.25) frameTime = 0.25;
            last = now;

            accumulator += frameTime;

            while (accumulator >= STEP) {
                this.update(STEP);
                accumulator -= STEP;
            }

            this.render();
            requestAnimationFrame(loop);
        };

        requestAnimationFrame(loop);
    }
}

// ========== Wrapper für Blazor ==========
window.slitherStandalone = {
    _initialized: false,
    _game: null,

    init: function () {
        if (this._initialized) {
            return;
        }
        this._initialized = true;
        console.log("slitherStandalone.init called");

        const game = new Game();
        this._game = game;

        const playButton = document.getElementById('playButton');
        const nameInput = document.getElementById('nameInput');
        const restartButton = document.getElementById('restartButton');

        if (!playButton) {
            console.warn("playButton not found");
            return;
        }

        playButton.addEventListener('click', () => {
            const name = (nameInput.value || '').trim() || 'Player';
            document.getElementById('startScreen').classList.add('hidden');
            game.init(name);
            game.start();
        });

        restartButton.addEventListener('click', () => {
            document.getElementById('gameOver').classList.remove('visible');
            const name = game.player ? game.player.name : 'Player';
            game.init(name);
        });

        nameInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                playButton.click();
            }
        });

        document.addEventListener('mousemove', (e) => {
            if (game.player) {
                game.mousePos = new Vec2(
                    e.clientX + game.camera.x,
                    e.clientY + game.camera.y
                );
            }
        });

        document.addEventListener('keydown', (e) => {
            game.keys[e.key] = true;
        });

        document.addEventListener('keyup', (e) => {
            game.keys[e.key] = false;
        });
    }
};
