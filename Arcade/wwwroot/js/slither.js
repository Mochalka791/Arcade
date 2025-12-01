window.slitherGame = {
    canvas: null,
    ctx: null,

    init: function (canvasElement) {
        this.canvas = canvasElement;
        this.ctx = this.canvas.getContext('2d');
    },

    render: function (data) {
        if (!this.ctx) return;

        const ctx = this.ctx;
        const canvas = this.canvas;

        // Hintergrund
        ctx.fillStyle = '#0a0e1a';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        ctx.save();
        ctx.translate(-data.camX, -data.camY);

        // Grid
        ctx.strokeStyle = 'rgba(255, 255, 255, 0.05)';
        ctx.lineWidth = 1;
        const gridSize = 100;

        const startX = Math.floor(data.camX / gridSize) * gridSize;
        const endX = Math.ceil((data.camX + canvas.width) / gridSize) * gridSize;
        const startY = Math.floor(data.camY / gridSize) * gridSize;
        const endY = Math.ceil((data.camY + canvas.height) / gridSize) * gridSize;

        for (let x = startX; x <= endX; x += gridSize) {
            ctx.beginPath();
            ctx.moveTo(x, 0);
            ctx.lineTo(x, data.worldHeight);
            ctx.stroke();
        }

        for (let y = startY; y <= endY; y += gridSize) {
            ctx.beginPath();
            ctx.moveTo(0, y);
            ctx.lineTo(data.worldWidth, y);
            ctx.stroke();
        }

        // Food rendern
        data.food.forEach(f => {
            ctx.fillStyle = f.color;
            ctx.beginPath();
            ctx.arc(f.x, f.y, f.radius, 0, Math.PI * 2);
            ctx.fill();

            // Glow-Effekt
            ctx.shadowBlur = 10;
            ctx.shadowColor = f.color;
            ctx.fill();
            ctx.shadowBlur = 0;
        });

        // Schlangen rendern
        data.snakes.forEach(snake => {
            // Körper
            ctx.strokeStyle = snake.color;
            ctx.lineWidth = snake.radius * 2;
            ctx.lineCap = 'round';
            ctx.lineJoin = 'round';

            ctx.beginPath();
            snake.segments.forEach((seg, i) => {
                if (i === 0) ctx.moveTo(seg.x, seg.y);
                else ctx.lineTo(seg.x, seg.y);
            });
            ctx.stroke();

            // Kopf-Highlight
            ctx.fillStyle = 'rgba(255, 255, 255, 0.3)';
            ctx.beginPath();
            ctx.arc(snake.headX, snake.headY, snake.radius * 1.3, 0, Math.PI * 2);
            ctx.fill();

            // Augen
            const eyeOffset = snake.radius * 0.5;
            const eyeAngle = Math.atan2(snake.dirY, snake.dirX);
            const perpX = -Math.sin(eyeAngle);
            const perpY = Math.cos(eyeAngle);

            ctx.fillStyle = '#000';

            ctx.beginPath();
            ctx.arc(
                snake.headX + perpX * eyeOffset,
                snake.headY + perpY * eyeOffset,
                3, 0, Math.PI * 2
            );
            ctx.fill();

            ctx.beginPath();
            ctx.arc(
                snake.headX - perpX * eyeOffset,
                snake.headY - perpY * eyeOffset,
                3, 0, Math.PI * 2
            );
            ctx.fill();

            if (snake.isPlayer) {
                ctx.fillStyle = '#fff';
                ctx.font = 'bold 16px Arial';
                ctx.textAlign = 'center';
                ctx.shadowBlur = 4;
                ctx.shadowColor = '#000';
                ctx.fillText(snake.name, snake.headX, snake.headY - snake.radius - 10);
                ctx.shadowBlur = 0;
            }
        });

        ctx.restore();
    }
};