console.log("🐍 Slither.io Clone loaded");

// ========== CONFIG ==========
const CONFIG = {
    WORLD_SIZE: 5000,
    GRID_SIZE: 50,
    PLAYER_SPEED: 200,
    BOT_SPEED: 180,
    BOOST_MULTI: 2.5,
    TURN_SPEED: 5.5,
    MAX_TURN_ANGLE: 2.5, // Maximaler Abbiegewinkel pro Frame (in Radiant)
    SEGMENT_SIZE: 10,
    HEAD_SIZE: 18,
    FOOD_COUNT: 2000,
    BOT_COUNT: 30,
    BOT_AGGRESSION: 0.4 // Wahrscheinlichkeit für aggressives Verhalten
};

// ========== VEC2 ==========
class Vec2 {
    constructor(x = 0, y = 0) {
        this.x = x;
        this.y = y;
    }

    add(v) { return new Vec2(this.x + v.x, this.y + v.y); }
    sub(v) { return new Vec2(this.x - v.x, this.y - v.y); }
    mul(n) { return new Vec2(this.x * n, this.y * n); }

    get len() { return Math.sqrt(this.x * this.x + this.y * this.y); }
    norm() {
        const l = this.len;
        return l > 0 ? new Vec2(this.x / l, this.y / l) : new Vec2(0, 0);
    }

    angle() { return Math.atan2(this.y, this.x); }

    static fromAngle(angle) {
        return new Vec2(Math.cos(angle), Math.sin(angle));
    }
}

// ========== FOOD ==========
class Food {
    constructor() {
        this.pos = new Vec2(
            Math.random() * CONFIG.WORLD_SIZE,
            Math.random() * CONFIG.WORLD_SIZE
        );
        this.hue = Math.random() * 360;
        this.size = 4 + Math.random() * 3;
        this.value = Math.floor(this.size / 2);
        this.t = Math.random() * Math.PI * 2;
    }
}

// ========== SNAKE ==========
class Snake {
    constructor(name, isPlayer = false) {
        this.id = Math.random();
        this.name = name;
        this.isPlayer = isPlayer;
        this.alive = true;

        // Position
        this.pos = new Vec2(
            500 + Math.random() * (CONFIG.WORLD_SIZE - 1000),
            500 + Math.random() * (CONFIG.WORLD_SIZE - 1000)
        );
        this.dir = new Vec2(Math.random() - 0.5, Math.random() - 0.5).norm();

        // Properties
        this.hue = Math.random() * 360;
        this.speed = isPlayer ? CONFIG.PLAYER_SPEED : CONFIG.BOT_SPEED;
        this.boosting = false;

        // AI Properties
        this.aiMode = 'hunt'; // 'hunt', 'flee', 'aggressive'
        this.aiTarget = null;
        this.aiTimer = 0;
        this.kills = 0;

        // Body
        this.body = [];
        for (let i = 0; i < 20; i++) {
            this.body.push(this.pos.sub(this.dir.mul(i * CONFIG.SEGMENT_SIZE)));
        }
    }

    get length() { return this.body.length; }
    get score() { return this.length * 10; }
}

// ========== GAME ==========
class Game {
    constructor() {
        this.canvas = document.getElementById('gameCanvas');
        this.ctx = this.canvas.getContext('2d');

        this.snakes = [];
        this.food = [];
        this.player = null;

        this.cam = { x: 0, y: 0 };
        this.mouse = { x: 0, y: 0 };
        this.keys = {};

        this.kills = 0;
        this.startTime = 0;
        this.botNames = ['Alpha', 'Beta', 'Gamma', 'Delta', 'Viper',
            'Python', 'Anaconda', 'Cobra', 'Killer', 'Hunter',
            'Shadow', 'Ghost', 'Reaper', 'Striker', 'Ninja'];

        this.setupCanvas();
        this.setupInput();
    }

    setupCanvas() {
        const resize = () => {
            this.canvas.width = window.innerWidth;
            this.canvas.height = window.innerHeight;
        };
        resize();
        window.addEventListener('resize', resize);
    }

    setupInput() {
        window.addEventListener('mousemove', e => {
            this.mouse.x = e.clientX;
            this.mouse.y = e.clientY;
        });

        window.addEventListener('keydown', e => {
            this.keys[e.key] = true;
        });

        window.addEventListener('keyup', e => {
            this.keys[e.key] = false;
        });
    }

    init(playerName) {
        console.log("🎮 Game starting...");

        this.snakes = [];
        this.food = [];
        this.kills = 0;
        this.startTime = Date.now();

        // Create player
        this.player = new Snake(playerName, true);
        this.snakes.push(this.player);

        // Create bots
        for (let i = 0; i < CONFIG.BOT_COUNT; i++) {
            this.spawnBot();
        }

        // Spawn food
        for (let i = 0; i < CONFIG.FOOD_COUNT; i++) {
            this.food.push(new Food());
        }

        console.log(`✅ Created ${this.snakes.length} snakes and ${this.food.length} food`);
    }

    spawnBot() {
        const name = this.botNames[Math.floor(Math.random() * this.botNames.length)] +
            Math.floor(Math.random() * 100);
        const bot = new Snake(name, false);
        bot.hue = Math.random() * 360;
        this.snakes.push(bot);
    }

    update(dt) {
        if (!this.player || !this.player.alive) return;

        // Update player
        this.updatePlayer(dt);

        // Update bots
        this.snakes.filter(s => !s.isPlayer && s.alive).forEach(bot => {
            this.updateBot(bot, dt);
        });

        // Move all snakes
        this.snakes.filter(s => s.alive).forEach(s => {
            this.moveSnake(s, dt);
        });

        // Collisions
        this.checkCollisions();

        // Keep bot count constant
        const aliveBots = this.snakes.filter(s => !s.isPlayer && s.alive).length;
        if (aliveBots < CONFIG.BOT_COUNT) {
            for (let i = aliveBots; i < CONFIG.BOT_COUNT; i++) {
                this.spawnBot();
            }
        }

        // Update camera
        this.cam.x = this.player.pos.x - this.canvas.width / 2;
        this.cam.y = this.player.pos.y - this.canvas.height / 2;

        // Update UI
        this.updateUI();
    }

    updatePlayer(dt) {
        const s = this.player;

        // Get mouse direction
        const worldMouse = new Vec2(
            this.mouse.x + this.cam.x,
            this.mouse.y + this.cam.y
        );
        const toMouse = worldMouse.sub(s.pos).norm();

        // Calculate angle difference with limit
        const currentAngle = s.dir.angle();
        const targetAngle = toMouse.angle();
        let angleDiff = targetAngle - currentAngle;

        // Normalize angle difference to [-PI, PI]
        while (angleDiff > Math.PI) angleDiff -= Math.PI * 2;
        while (angleDiff < -Math.PI) angleDiff += Math.PI * 2;

        // Limit turn speed
        const maxTurn = CONFIG.MAX_TURN_ANGLE * dt * 60;
        angleDiff = Math.max(-maxTurn, Math.min(maxTurn, angleDiff));

        // Apply limited turn
        const newAngle = currentAngle + angleDiff;
        s.dir = Vec2.fromAngle(newAngle);

        // Boost
        if ((this.keys[' '] || this.keys['Shift']) && s.length > 15) {
            s.boosting = true;
            s.speed = CONFIG.PLAYER_SPEED * CONFIG.BOOST_MULTI;

            // Drop mass
            if (Math.random() < 0.25 && s.body.length > 15) {
                const tail = s.body.pop();
                if (tail) {
                    const f = new Food();
                    f.pos = tail;
                    f.size = 8;
                    f.value = 3;
                    this.food.push(f);
                }
            }
        } else {
            s.boosting = false;
            s.speed = CONFIG.PLAYER_SPEED;
        }
    }

    updateBot(snake, dt) {
        snake.aiTimer += dt;

        // Change AI mode periodically
        if (snake.aiTimer > 3) {
            snake.aiTimer = 0;
            const rand = Math.random();

            if (rand < CONFIG.BOT_AGGRESSION && snake.length > 25) {
                snake.aiMode = 'aggressive';
            } else if (snake.length < 15) {
                snake.aiMode = 'flee';
            } else {
                snake.aiMode = 'hunt';
            }
        }

        let desired = snake.dir;

        switch (snake.aiMode) {
            case 'aggressive':
                desired = this.botAggressiveBehavior(snake);
                break;
            case 'flee':
                desired = this.botFleeBehavior(snake);
                break;
            default:
                desired = this.botHuntBehavior(snake);
        }

        // Apply turn with limits
        const currentAngle = snake.dir.angle();
        const targetAngle = desired.angle();
        let angleDiff = targetAngle - currentAngle;

        while (angleDiff > Math.PI) angleDiff -= Math.PI * 2;
        while (angleDiff < -Math.PI) angleDiff += Math.PI * 2;

        const maxTurn = CONFIG.MAX_TURN_ANGLE * 0.8 * dt * 60;
        angleDiff = Math.max(-maxTurn, Math.min(maxTurn, angleDiff));

        const newAngle = currentAngle + angleDiff;
        snake.dir = Vec2.fromAngle(newAngle);

        // Smart boosting
        if (snake.aiMode === 'aggressive' && snake.length > 30) {
            snake.boosting = Math.random() < 0.3;
            snake.speed = snake.boosting ?
                CONFIG.BOT_SPEED * CONFIG.BOOST_MULTI : CONFIG.BOT_SPEED;
        } else {
            snake.boosting = false;
            snake.speed = CONFIG.BOT_SPEED;
        }
    }

    botHuntBehavior(snake) {
        let target = null;
        let minDist = 800;

        // Find nearest food
        for (let i = 0; i < 50; i++) {
            const f = this.food[Math.floor(Math.random() * this.food.length)];
            if (!f) continue;

            const dist = f.pos.sub(snake.pos).len;
            if (dist < minDist) {
                minDist = dist;
                target = f.pos;
            }
        }

        let desired = snake.dir;
        if (target) {
            desired = target.sub(snake.pos).norm();
        }

        // Avoid obstacles
        const avoid = this.botAvoidObstacles(snake);
        desired = desired.add(avoid.mul(3));

        return desired.norm();
    }

    botAggressiveBehavior(snake) {
        // Find weaker snake to hunt
        let target = null;
        let minDist = 600;

        this.snakes.forEach(other => {
            if (other.id === snake.id || !other.alive) return;
            if (other.length >= snake.length * 0.8) return; // Only hunt weaker

            const dist = other.pos.sub(snake.pos).len;
            if (dist < minDist) {
                minDist = dist;
                target = other;
            }
        });

        let desired = snake.dir;

        if (target) {
            // Try to circle around target
            const toTarget = target.pos.sub(snake.pos);
            const angle = toTarget.angle() + Math.PI / 2;
            const circleDir = Vec2.fromAngle(angle);

            desired = toTarget.norm().add(circleDir.mul(0.7)).norm();
        } else {
            // No target, hunt food
            return this.botHuntBehavior(snake);
        }

        const avoid = this.botAvoidObstacles(snake);
        desired = desired.add(avoid.mul(2));

        return desired.norm();
    }

    botFleeBehavior(snake) {
        // Flee from larger snakes
        let flee = new Vec2(0, 0);

        this.snakes.forEach(other => {
            if (other.id === snake.id || !other.alive) return;
            if (other.length < snake.length * 1.2) return;

            const dist = other.pos.sub(snake.pos).len;
            if (dist < 400) {
                const away = snake.pos.sub(other.pos).norm();
                flee = flee.add(away.mul(3 / (dist + 1)));
            }
        });

        let desired = snake.dir.add(flee);

        // Still collect nearby food
        const nearFood = this.food.find(f =>
            f.pos.sub(snake.pos).len < 150
        );
        if (nearFood) {
            desired = desired.add(nearFood.pos.sub(snake.pos).norm().mul(0.3));
        }

        const avoid = this.botAvoidObstacles(snake);
        desired = desired.add(avoid.mul(3));

        return desired.norm();
    }

    botAvoidObstacles(snake) {
        let avoid = new Vec2(0, 0);

        // Avoid other snakes' bodies
        this.snakes.forEach(other => {
            if (!other.alive) return;

            for (let i = 5; i < other.body.length; i++) {
                const seg = other.body[i];
                const dist = seg.sub(snake.pos).len;

                if (dist < 100) {
                    const away = snake.pos.sub(seg).norm();
                    avoid = avoid.add(away.mul(2 / (dist + 1)));
                }
            }
        });

        // Avoid walls
        const margin = 400;
        if (snake.pos.x < margin) avoid.x += (margin - snake.pos.x) / 100;
        if (snake.pos.x > CONFIG.WORLD_SIZE - margin)
            avoid.x -= (snake.pos.x - (CONFIG.WORLD_SIZE - margin)) / 100;
        if (snake.pos.y < margin) avoid.y += (margin - snake.pos.y) / 100;
        if (snake.pos.y > CONFIG.WORLD_SIZE - margin)
            avoid.y -= (snake.pos.y - (CONFIG.WORLD_SIZE - margin)) / 100;

        return avoid;
    }

    moveSnake(snake, dt) {
        // Move head
        const vel = snake.dir.mul(snake.speed * dt);
        snake.pos = snake.pos.add(vel);

        // Clamp to world
        snake.pos.x = Math.max(0, Math.min(CONFIG.WORLD_SIZE, snake.pos.x));
        snake.pos.y = Math.max(0, Math.min(CONFIG.WORLD_SIZE, snake.pos.y));

        // Update body
        snake.body.unshift(new Vec2(snake.pos.x, snake.pos.y));

        // Keep body length
        while (snake.body.length > snake.length) {
            snake.body.pop();
        }

        // Smooth body
        for (let i = 1; i < snake.body.length; i++) {
            const prev = snake.body[i - 1];
            const curr = snake.body[i];
            const diff = prev.sub(curr);
            const dist = diff.len;

            if (dist > CONFIG.SEGMENT_SIZE) {
                snake.body[i] = prev.sub(diff.norm().mul(CONFIG.SEGMENT_SIZE));
            }
        }
    }

    checkCollisions() {
        // Food collision
        this.snakes.filter(s => s.alive).forEach(snake => {
            for (let i = this.food.length - 1; i >= 0; i--) {
                const f = this.food[i];
                const dist = f.pos.sub(snake.pos).len;

                if (dist < CONFIG.HEAD_SIZE) {
                    snake.body.push(snake.body[snake.body.length - 1]);
                    this.food.splice(i, 1);

                    if (this.food.length < CONFIG.FOOD_COUNT) {
                        this.food.push(new Food());
                    }
                }
            }
        });

        // Snake collision
        const alive = this.snakes.filter(s => s.alive);

        for (let i = 0; i < alive.length; i++) {
            const s1 = alive[i];

            for (let j = 0; j < alive.length; j++) {
                const s2 = alive[j];
                if (s1.id === s2.id) continue;

                for (let k = 5; k < s2.body.length; k++) {
                    const seg = s2.body[k];
                    const dist = s1.pos.sub(seg).len;

                    if (dist < CONFIG.HEAD_SIZE) {
                        this.killSnake(s1, s2);
                        break;
                    }
                }

                if (!s1.alive) break;
            }
        }
    }

    killSnake(snake, killer) {
        snake.alive = false;

        // Drop food
        snake.body.forEach((seg, i) => {
            if (i % 2 === 0) {
                const f = new Food();
                f.pos = seg;
                f.size = 7;
                f.value = 2;
                this.food.push(f);
            }
        });

        // Update kills
        if (killer) {
            killer.kills++;
            if (killer.isPlayer) {
                this.kills++;
            }
        }

        // Game over for player
        if (snake.isPlayer) {
            setTimeout(() => this.gameOver(), 1000);
        }
    }

    gameOver() {
        const time = Math.floor((Date.now() - this.startTime) / 1000);
        document.getElementById('finalScore').textContent = 'Score: ' + this.player.score;
        document.getElementById('finalKills').textContent = 'Kills: ' + this.kills;
        document.getElementById('playTime').textContent = 'Zeit: ' + time + 's';
        document.getElementById('gameOver').classList.add('visible');
    }

    updateUI() {
        document.getElementById('score').textContent = 'Länge: ' + this.player.length;
        document.getElementById('kills').textContent = 'Kills: ' + this.kills;

        const top = [...this.snakes]
            .filter(s => s.alive)
            .sort((a, b) => b.score - a.score)
            .slice(0, 10);

        const html = top.map((s, i) => {
            const cls = s.isPlayer ? 'player' : '';
            return `<div class="leaderboard-entry ${cls}">
                <span>${i + 1}. ${s.name}</span>
                <span>${s.score}</span>
            </div>`;
        }).join('');

        const el = document.getElementById('leaderboardList');
        if (el) el.innerHTML = html;
    }

    render() {
        const ctx = this.ctx;
        const w = this.canvas.width;
        const h = this.canvas.height;

        ctx.fillStyle = '#0a1628';
        ctx.fillRect(0, 0, w, h);

        ctx.save();
        ctx.translate(-this.cam.x, -this.cam.y);

        this.drawGrid(ctx);

        // Food
        this.food.forEach(f => {
            f.t += 0.05;
            const glow = Math.sin(f.t) * 2;

            ctx.shadowBlur = 8 + glow;
            ctx.shadowColor = `hsl(${f.hue}, 100%, 50%)`;
            ctx.fillStyle = `hsl(${f.hue}, 100%, 60%)`;

            ctx.beginPath();
            ctx.arc(f.pos.x, f.pos.y, f.size, 0, Math.PI * 2);
            ctx.fill();
        });
        ctx.shadowBlur = 0;

        // Snakes
        const sorted = [...this.snakes].filter(s => s.alive)
            .sort((a, b) => a.length - b.length);

        sorted.forEach(s => {
            // Body
            const color = `hsl(${s.hue}, 80%, 50%)`;

            if (s.boosting) {
                ctx.shadowBlur = 20;
                ctx.shadowColor = color;
            }

            ctx.strokeStyle = color;
            ctx.lineWidth = CONFIG.HEAD_SIZE * 1.5;
            ctx.lineCap = 'round';
            ctx.lineJoin = 'round';

            ctx.beginPath();
            s.body.forEach((pt, i) => {
                if (i === 0) ctx.moveTo(pt.x, pt.y);
                else ctx.lineTo(pt.x, pt.y);
            });
            ctx.stroke();

            ctx.shadowBlur = 0;

            // Direction arrows
            this.drawDirectionArrows(ctx, s);

            // Head
            ctx.fillStyle = 'rgba(255, 255, 255, 0.3)';
            ctx.beginPath();
            ctx.arc(s.pos.x, s.pos.y, CONFIG.HEAD_SIZE, 0, Math.PI * 2);
            ctx.fill();

            // Eyes
            const angle = s.dir.angle();
            const eyeDist = 10;
            const eyeSize = 5;

            const eye1 = new Vec2(
                s.pos.x + Math.cos(angle - 0.4) * eyeDist,
                s.pos.y + Math.sin(angle - 0.4) * eyeDist
            );
            const eye2 = new Vec2(
                s.pos.x + Math.cos(angle + 0.4) * eyeDist,
                s.pos.y + Math.sin(angle + 0.4) * eyeDist
            );

            ctx.fillStyle = '#fff';
            ctx.beginPath();
            ctx.arc(eye1.x, eye1.y, eyeSize, 0, Math.PI * 2);
            ctx.arc(eye2.x, eye2.y, eyeSize, 0, Math.PI * 2);
            ctx.fill();

            ctx.fillStyle = '#000';
            ctx.beginPath();
            ctx.arc(eye1.x + s.dir.x * 2, eye1.y + s.dir.y * 2, 3, 0, Math.PI * 2);
            ctx.arc(eye2.x + s.dir.x * 2, eye2.y + s.dir.y * 2, 3, 0, Math.PI * 2);
            ctx.fill();

            // Name
            ctx.fillStyle = '#fff';
            ctx.strokeStyle = '#000';
            ctx.lineWidth = 3;
            ctx.font = 'bold 14px Arial';
            ctx.textAlign = 'center';
            ctx.strokeText(s.name, s.pos.x, s.pos.y - 25);
            ctx.fillText(s.name, s.pos.x, s.pos.y - 25);
        });

        ctx.restore();
        this.drawMinimap(ctx);
    }

    drawDirectionArrows(ctx, snake) {
        const angle = snake.dir.angle();
        const arrowDist = CONFIG.HEAD_SIZE + 15;
        const arrowSize = 8;

        // Main arrow
        const arrowPos = new Vec2(
            snake.pos.x + Math.cos(angle) * arrowDist,
            snake.pos.y + Math.sin(angle) * arrowDist
        );

        ctx.fillStyle = snake.isPlayer ?
            'rgba(92, 240, 200, 0.8)' : 'rgba(255, 255, 255, 0.5)';
        ctx.strokeStyle = snake.isPlayer ?
            'rgba(92, 240, 200, 1)' : 'rgba(255, 255, 255, 0.8)';
        ctx.lineWidth = 2;

        ctx.save();
        ctx.translate(arrowPos.x, arrowPos.y);
        ctx.rotate(angle);

        ctx.beginPath();
        ctx.moveTo(arrowSize, 0);
        ctx.lineTo(-arrowSize / 2, -arrowSize / 2);
        ctx.lineTo(-arrowSize / 2, arrowSize / 2);
        ctx.closePath();
        ctx.fill();
        ctx.stroke();

        ctx.restore();

        // Side indicators (only for player)
        if (snake.isPlayer) {
            const sideAlpha = 0.3;

            // Left arrow
            const leftAngle = angle - Math.PI / 6;
            const leftPos = new Vec2(
                snake.pos.x + Math.cos(leftAngle) * arrowDist,
                snake.pos.y + Math.sin(leftAngle) * arrowDist
            );

            ctx.fillStyle = `rgba(92, 240, 200, ${sideAlpha})`;
            ctx.save();
            ctx.translate(leftPos.x, leftPos.y);
            ctx.rotate(leftAngle);
            ctx.beginPath();
            ctx.moveTo(6, 0);
            ctx.lineTo(-3, -3);
            ctx.lineTo(-3, 3);
            ctx.closePath();
            ctx.fill();
            ctx.restore();

            // Right arrow
            const rightAngle = angle + Math.PI / 6;
            const rightPos = new Vec2(
                snake.pos.x + Math.cos(rightAngle) * arrowDist,
                snake.pos.y + Math.sin(rightAngle) * arrowDist
            );

            ctx.save();
            ctx.translate(rightPos.x, rightPos.y);
            ctx.rotate(rightAngle);
            ctx.beginPath();
            ctx.moveTo(6, 0);
            ctx.lineTo(-3, -3);
            ctx.lineTo(-3, 3);
            ctx.closePath();
            ctx.fill();
            ctx.restore();
        }
    }

    drawGrid(ctx) {
        const startX = Math.floor(this.cam.x / CONFIG.GRID_SIZE) * CONFIG.GRID_SIZE;
        const endX = startX + this.canvas.width + CONFIG.GRID_SIZE;
        const startY = Math.floor(this.cam.y / CONFIG.GRID_SIZE) * CONFIG.GRID_SIZE;
        const endY = startY + this.canvas.height + CONFIG.GRID_SIZE;

        ctx.strokeStyle = 'rgba(255, 255, 255, 0.05)';
        ctx.lineWidth = 1;
        ctx.beginPath();

        for (let x = startX; x <= endX; x += CONFIG.GRID_SIZE) {
            ctx.moveTo(x, startY);
            ctx.lineTo(x, endY);
        }
        for (let y = startY; y <= endY; y += CONFIG.GRID_SIZE) {
            ctx.moveTo(startX, y);
            ctx.lineTo(endX, y);
        }
        ctx.stroke();
    }

    drawMinimap(ctx) {
        const size = 150;
        const x = this.canvas.width - size - 20;
        const y = 20;

        ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
        ctx.fillRect(x, y, size, size);

        const scale = size / CONFIG.WORLD_SIZE;

        this.snakes.filter(s => s.alive).forEach(s => {
            const mx = x + s.pos.x * scale;
            const my = y + s.pos.y * scale;

            ctx.fillStyle = s.isPlayer ? '#5cf0c8' : `hsl(${s.hue}, 80%, 50%)`;
            ctx.beginPath();
            ctx.arc(mx, my, s.isPlayer ? 3 : 1.5, 0, Math.PI * 2);
            ctx.fill();
        });

        // Border
        ctx.strokeStyle = 'rgba(255, 255, 255, 0.3)';
        ctx.lineWidth = 2;
        ctx.strokeRect(x, y, size, size);
    }

    start() {
        let last = performance.now();

        const loop = (now) => {
            const dt = Math.min((now - last) / 1000, 0.1);
            last = now;

            this.update(dt);
            this.render();

            requestAnimationFrame(loop);
        };

        requestAnimationFrame(loop);
    }
}

// ========== INIT ==========
window.slitherStandalone = {
    _game: null,

    init: function () {
        console.log("🚀 Initializing Slither.io...");

        const game = new Game();
        this._game = game;

        const playBtn = document.getElementById('playButton');
        const nameInput = document.getElementById('nameInput');
        const restartBtn = document.getElementById('restartButton');

        if (playBtn) {
            playBtn.onclick = () => {
                const name = nameInput.value.trim() || 'Player';
                document.getElementById('startScreen').classList.add('hidden');
                game.init(name);
                game.start();
            };
        }

        if (restartBtn) {
            restartBtn.onclick = () => {
                document.getElementById('gameOver').classList.remove('visible');
                const name = game.player ? game.player.name : 'Player';
                game.init(name);
            };
        }

        if (nameInput) {
            nameInput.addEventListener('keypress', e => {
                if (e.key === 'Enter') playBtn.click();
            });
        }

        console.log("✅ Ready to play!");
    }
};