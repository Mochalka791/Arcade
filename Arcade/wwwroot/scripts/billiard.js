(function () {
    const games = new WeakMap();

    function init(canvas, dotnetRef) {
        if (!canvas) {
            return;
        }

        const ctx = canvas.getContext('2d');
        const state = {
            canvas,
            ctx,
            dotnetRef,
            width: canvas.width,
            height: canvas.height,
            cushion: 36,
            pocketRadius: 28,
            friction: 0.985,
            balls: [],
            shots: 0,
            remaining: 0,
            ready: true,
            victory: false,
            animationId: null,
            wasMoving: false
        };

        games.set(canvas, state);
        resetState(state);
        loop(state);
        notify(state);
    }

    function resetState(state) {
        state.balls = createBalls(state);
        state.shots = 0;
        state.remaining = state.balls.filter(b => !b.cue).length;
        state.ready = true;
        state.victory = false;
        state.wasMoving = false;
    }

    function createBalls(state) {
        const centerY = state.height / 2;
        const targetBaseX = state.width - state.cushion - 140;
        const spacing = 36;

        return [
            createBall(state.cushion + 120, centerY, "#ffffff", true),
            createBall(targetBaseX, centerY, "#ffce54"),
            createBall(targetBaseX + spacing, centerY - spacing / 2, "#55d4ff"),
            createBall(targetBaseX + spacing, centerY + spacing / 2, "#ff6b7a")
        ];
    }

    function createBall(x, y, color, cue = false) {
        return { x, y, vx: 0, vy: 0, radius: 14, color, cue, pocketed: false };
    }

    function shoot(canvas, angleDeg, power) {
        const state = games.get(canvas);
        if (!state || !state.ready || state.victory) {
            return;
        }

        const cue = state.balls.find(b => b.cue);
        if (!cue) {
            return;
        }

        const angle = (angleDeg * Math.PI) / 180;
        const strength = (power / 100) * 14;

        cue.vx = Math.cos(angle) * strength;
        cue.vy = Math.sin(angle) * strength;

        state.shots += 1;
        state.ready = false;
        notify(state);
    }

    function reset(canvas) {
        const state = games.get(canvas);
        if (!state) {
            return;
        }

        resetState(state);
        notify(state);
    }

    function dispose(canvas) {
        const state = games.get(canvas);
        if (!state) {
            return;
        }

        if (state.animationId) {
            cancelAnimationFrame(state.animationId);
        }

        games.delete(canvas);
    }

    function loop(state) {
        update(state);
        draw(state);
        state.animationId = requestAnimationFrame(() => loop(state));
    }

    function update(state) {
        let moving = false;
        const left = state.cushion;
        const right = state.width - state.cushion;
        const top = state.cushion;
        const bottom = state.height - state.cushion;

        for (const ball of state.balls) {
            if (ball.pocketed) {
                continue;
            }

            ball.x += ball.vx;
            ball.y += ball.vy;
            ball.vx *= state.friction;
            ball.vy *= state.friction;

            if (Math.abs(ball.vx) < 0.02) {
                ball.vx = 0;
            }
            if (Math.abs(ball.vy) < 0.02) {
                ball.vy = 0;
            }

            if (ball.vx !== 0 || ball.vy !== 0) {
                moving = true;
            }

            if (ball.x - ball.radius < left) {
                ball.x = left + ball.radius;
                ball.vx *= -1;
            }
            if (ball.x + ball.radius > right) {
                ball.x = right - ball.radius;
                ball.vx *= -1;
            }
            if (ball.y - ball.radius < top) {
                ball.y = top + ball.radius;
                ball.vy *= -1;
            }
            if (ball.y + ball.radius > bottom) {
                ball.y = bottom - ball.radius;
                ball.vy *= -1;
            }
        }

        resolveCollisions(state);
        handlePockets(state);

        if (!moving && state.wasMoving) {
            state.ready = !state.victory;
            notify(state);
        }

        state.wasMoving = moving;
    }

    function resolveCollisions(state) {
        const balls = state.balls;
        for (let i = 0; i < balls.length; i++) {
            const a = balls[i];
            if (a.pocketed) continue;
            for (let j = i + 1; j < balls.length; j++) {
                const b = balls[j];
                if (b.pocketed) continue;

                const dx = b.x - a.x;
                const dy = b.y - a.y;
                const dist = Math.hypot(dx, dy);
                const minDist = a.radius + b.radius;
                if (dist === 0 || dist >= minDist) {
                    continue;
                }

                const overlap = (minDist - dist) / 2;
                const nx = dx / dist;
                const ny = dy / dist;

                a.x -= nx * overlap;
                a.y -= ny * overlap;
                b.x += nx * overlap;
                b.y += ny * overlap;

                const va = a.vx * nx + a.vy * ny;
                const vb = b.vx * nx + b.vy * ny;
                const impulse = vb - va;

                a.vx += impulse * nx;
                a.vy += impulse * ny;
                b.vx -= impulse * nx;
                b.vy -= impulse * ny;
            }
        }
    }

    function handlePockets(state) {
        const pockets = [
            { x: state.cushion, y: state.cushion },
            { x: state.width / 2, y: state.cushion - 10 },
            { x: state.width - state.cushion, y: state.cushion },
            { x: state.cushion, y: state.height - state.cushion },
            { x: state.width / 2, y: state.height - state.cushion + 10 },
            { x: state.width - state.cushion, y: state.height - state.cushion }
        ];

        let updated = false;

        for (const ball of state.balls) {
            if (ball.pocketed) {
                continue;
            }

            for (const pocket of pockets) {
                const dist = Math.hypot(ball.x - pocket.x, ball.y - pocket.y);
                if (dist <= state.pocketRadius) {
                    if (ball.cue) {
                        ball.x = state.cushion + 120;
                        ball.y = state.height / 2;
                        ball.vx = 0;
                        ball.vy = 0;
                        updated = true;
                    } else {
                        ball.pocketed = true;
                        ball.vx = 0;
                        ball.vy = 0;
                        state.remaining = Math.max(0, state.remaining - 1);
                        updated = true;
                    }
                    break;
                }
            }
        }

        if (state.remaining === 0 && !state.victory) {
            state.victory = true;
            state.ready = false;
            updated = true;
        }

        if (updated) {
            notify(state);
        }
    }

    function draw(state) {
        const ctx = state.ctx;
        ctx.clearRect(0, 0, state.width, state.height);

        // Felt
        ctx.fillStyle = "#1f6b44";
        ctx.fillRect(0, 0, state.width, state.height);

        // Inner play area
        ctx.fillStyle = "#248154";
        ctx.fillRect(state.cushion, state.cushion, state.width - state.cushion * 2, state.height - state.cushion * 2);

        // Pockets
        ctx.fillStyle = "#0d1a1f";
        const pocketOffsets = [
            [state.cushion, state.cushion],
            [state.width / 2, state.cushion - 10],
            [state.width - state.cushion, state.cushion],
            [state.cushion, state.height - state.cushion],
            [state.width / 2, state.height - state.cushion + 10],
            [state.width - state.cushion, state.height - state.cushion]
        ];
        for (const [px, py] of pocketOffsets) {
            ctx.beginPath();
            ctx.arc(px, py, state.pocketRadius, 0, Math.PI * 2);
            ctx.fill();
        }

        for (const ball of state.balls) {
            if (ball.pocketed) {
                continue;
            }

            const gradient = ctx.createRadialGradient(ball.x - 4, ball.y - 4, ball.radius / 5, ball.x, ball.y, ball.radius);
            gradient.addColorStop(0, "#ffffff");
            gradient.addColorStop(0.3, lighten(ball.color, 0.35));
            gradient.addColorStop(1, ball.color);

            ctx.beginPath();
            ctx.fillStyle = gradient;
            ctx.arc(ball.x, ball.y, ball.radius, 0, Math.PI * 2);
            ctx.fill();

            ctx.strokeStyle = "rgba(0, 0, 0, 0.35)";
            ctx.lineWidth = 1.2;
            ctx.stroke();
        }
    }

    function lighten(color, amount) {
        const hex = color.replace('#', '');
        const num = parseInt(hex, 16);
        let r = (num >> 16) & 0xff;
        let g = (num >> 8) & 0xff;
        let b = num & 0xff;
        r = Math.min(255, Math.floor(r + (255 - r) * amount));
        g = Math.min(255, Math.floor(g + (255 - g) * amount));
        b = Math.min(255, Math.floor(b + (255 - b) * amount));
        return `rgb(${r}, ${g}, ${b})`;
    }

    function notify(state) {
        if (!state.dotnetRef) {
            return;
        }

        try {
            state.dotnetRef.invokeMethodAsync(
                "UpdateStatus",
                state.remaining,
                state.shots,
                state.victory,
                state.ready
            );
        } catch (err) {
            console.warn('billiard notify failed', err);
        }
    }

    window.billiardGame = {
        init,
        shoot,
        reset,
        dispose
    };
})();
