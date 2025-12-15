window.towerDefense = {
    canvas: null,
    ctx: null,
    dotNetRef: null,

    init(dotNetRef) {
        this.dotNetRef = dotNetRef;

        this.canvas = document.getElementById('gameCanvas');
        if (!this.canvas) {
            console.error('Canvas not found');
            return;
        }

        this.resizeCanvas();
        window.addEventListener('resize', () => this.resizeCanvas());

        this.ctx = this.canvas.getContext('2d');

        this.canvas.addEventListener('click', (e) => {
            if (!this.dotNetRef) return;

            const rect = this.canvas.getBoundingClientRect();
            const scaleX = 800 / rect.width;
            const scaleY = 600 / rect.height;

            const x = (e.clientX - rect.left) * scaleX;
            const y = (e.clientY - rect.top) * scaleY;

            this.dotNetRef.invokeMethodAsync('OnCanvasClick', x, y);
        });
    },

    resizeCanvas() {
        const container = this.canvas.parentElement;
        const maxWidth = container.clientWidth - 40;
        const maxHeight = container.clientHeight - 40;
        const aspectRatio = 4 / 3;

        let width = maxWidth;
        let height = width / aspectRatio;

        if (height > maxHeight) {
            height = maxHeight;
            width = height * aspectRatio;
        }

        this.canvas.width = 1200;
        this.canvas.height = 900;
        this.canvas.style.width = width + 'px';
        this.canvas.style.height = height + 'px';
    },

    render(data) {
        if (!this.ctx) return;

        const ctx = this.ctx;
        const w = this.canvas.width;
        const h = this.canvas.height;

        const scaleX = w / 800;
        const scaleY = h / 600;

        /* Clear */
        ctx.fillStyle = '#0a0e14';
        ctx.fillRect(0, 0, w, h);

        /* Grid */
        ctx.strokeStyle = 'rgba(102, 126, 234, 0.08)';
        ctx.lineWidth = 1;
        for (let x = 0; x < w; x += 60) {
            ctx.beginPath();
            ctx.moveTo(x, 0);
            ctx.lineTo(x, h);
            ctx.stroke();
        }
        for (let y = 0; y < h; y += 60) {
            ctx.beginPath();
            ctx.moveTo(0, y);
            ctx.lineTo(w, y);
            ctx.stroke();
        }

        /* Path */
        if (data.path?.length) {
            ctx.lineCap = 'round';
            ctx.lineJoin = 'round';

            ctx.strokeStyle = '#5a4a3a';
            ctx.lineWidth = 90 * scaleX;
            ctx.beginPath();
            data.path.forEach((p, i) => {
                const x = p.x * scaleX;
                const y = p.y * scaleY;
                i === 0 ? ctx.moveTo(x, y) : ctx.lineTo(x, y);
            });
            ctx.stroke();

            ctx.strokeStyle = '#8b7355';
            ctx.lineWidth = 80 * scaleX;
            ctx.beginPath();
            data.path.forEach((p, i) => {
                const x = p.x * scaleX;
                const y = p.y * scaleY;
                i === 0 ? ctx.moveTo(x, y) : ctx.lineTo(x, y);
            });
            ctx.stroke();
        }

        /* Towers */
        data.towers?.forEach(t => {
            const tx = t.x * scaleX;
            const ty = t.y * scaleY;
            const size = 20 * scaleX;

            const colors = [
                '#667eea', '#f093fb', '#ff6b6b',
                '#4dd0e1', '#9c27b0', '#ffd700'
            ];

            const color = colors[t.type] || '#667eea';

            ctx.shadowBlur = 25;
            ctx.shadowColor = color;
            ctx.fillStyle = color;

            ctx.beginPath();
            ctx.arc(tx, ty, size, 0, Math.PI * 2);
            ctx.fill();

            ctx.shadowBlur = 0;
            ctx.fillStyle = '#fff';
            ctx.beginPath();
            ctx.arc(tx, ty, size * 0.6, 0, Math.PI * 2);
            ctx.fill();

            if (t.level > 1) {
                ctx.fillStyle = '#ffd700';
                ctx.font = `${14 * scaleX}px Arial`;
                ctx.textAlign = 'center';
                ctx.fillText(`L${t.level}`, tx, ty - size - 8);
            }
        });

        /* Enemies */
        data.enemies?.forEach(e => {
            const ex = e.x * scaleX;
            const ey = e.y * scaleY;
            const esize = e.size * scaleX;

            const enemyColors = [
                '#ff4757', '#ff6348', '#5f27cd',
                '#00d2d3', '#c23616', '#192a56', '#273c75'
            ];

            const color = enemyColors[e.type] || '#ff4757';

            ctx.shadowBlur = e.isBoss ? 30 : 15;
            ctx.shadowColor = color;
            ctx.fillStyle = color;

            ctx.beginPath();
            ctx.arc(ex, ey, esize, 0, Math.PI * 2);
            ctx.fill();

            ctx.shadowBlur = 0;

            /* HP Bar */
            const hpPct = e.hp / e.maxHP;
            ctx.fillStyle = '#222';
            ctx.fillRect(ex - esize, ey - esize - 14, esize * 2, 5);

            ctx.fillStyle =
                hpPct > 0.5 ? '#4ecb71' :
                    hpPct > 0.25 ? '#ffa502' : '#ff4757';

            ctx.fillRect(ex - esize, ey - esize - 14, esize * 2 * hpPct, 5);

            if (e.isBoss) {
                ctx.font = `${18 * scaleX}px Arial`;
                ctx.textAlign = 'center';
                ctx.fillText('👑', ex, ey - esize - 26);
            }
        });

        /* Projectiles */
        data.projectiles?.forEach(p => {
            const px = p.x * scaleX;
            const py = p.y * scaleY;

            ctx.fillStyle = '#feca57';
            ctx.beginPath();
            ctx.arc(px, py, 6 * scaleX, 0, Math.PI * 2);
            ctx.fill();
        });
    }
};