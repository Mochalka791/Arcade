window.towerDefense = {
    canvas: null,
    ctx: null,
    dotNetRef: null,

    init() {
        this.canvas = document.getElementById('gameCanvas');
        if (!this.canvas) {
            console.error('Canvas not found');
            return;
        }

        this.resizeCanvas();
        window.addEventListener('resize', () => this.resizeCanvas());

        this.ctx = this.canvas.getContext('2d');

        // Click handler mit korrekter Skalierung
        this.canvas.addEventListener('click', (e) => {
            const rect = this.canvas.getBoundingClientRect();

            // Klick-Position im Canvas (skaliert auf 800x600)
            const scaleX = 800 / rect.width;
            const scaleY = 600 / rect.height;

            const x = (e.clientX - rect.left) * scaleX;
            const y = (e.clientY - rect.top) * scaleY;

            console.log('Canvas clicked:', x, y);

            // Rufe Blazor-Methode auf
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnCanvasClick', x, y);
            }
        });

        console.log('Tower Defense initialized');
    },

    setDotNetRef(ref) {
        this.dotNetRef = ref;
        console.log('DotNet reference set');
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
        if (!this.ctx) {
            this.init();
            if (!this.ctx) return;
        }

        const ctx = this.ctx;
        const w = this.canvas.width;
        const h = this.canvas.height;

        // Clear
        ctx.fillStyle = '#0a0e14';
        ctx.fillRect(0, 0, w, h);

        // Grid
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

        // Skalierungsfaktor für alle Koordinaten
        const scaleX = w / 800;
        const scaleY = h / 600;

        // Path
        if (data.path && data.path.length > 0) {
            // Path shadow
            ctx.strokeStyle = '#5a4a3a';
            ctx.lineWidth = 90 * scaleX;
            ctx.lineCap = 'round';
            ctx.lineJoin = 'round';
            ctx.beginPath();
            data.path.forEach((p, i) => {
                const x = p.x * scaleX;
                const y = p.y * scaleY;
                if (i === 0) ctx.moveTo(x, y);
                else ctx.lineTo(x, y);
            });
            ctx.stroke();

            // Main path
            ctx.strokeStyle = '#8b7355';
            ctx.lineWidth = 80 * scaleX;
            ctx.beginPath();
            data.path.forEach((p, i) => {
                const x = p.x * scaleX;
                const y = p.y * scaleY;
                if (i === 0) ctx.moveTo(x, y);
                else ctx.lineTo(x, y);
            });
            ctx.stroke();
        }

        // Tower ranges
        if (data.selectedTower !== null && data.towers) {
            ctx.fillStyle = 'rgba(102, 126, 234, 0.08)';
            ctx.strokeStyle = 'rgba(102, 126, 234, 0.3)';
            ctx.lineWidth = 3;

            data.towers.forEach(t => {
                const tx = t.x * scaleX;
                const ty = t.y * scaleY;
                const range = t.range * scaleX;

                ctx.beginPath();
                ctx.arc(tx, ty, range, 0, Math.PI * 2);
                ctx.fill();
                ctx.stroke();
            });
        }

        // Towers
        if (data.towers) {
            data.towers.forEach(t => {
                const tx = t.x * scaleX;
                const ty = t.y * scaleY;
                const size = 20 * scaleX;

                const colors = {
                    0: '#667eea',
                    1: '#f093fb',
                    2: '#ff6b6b'
                };

                const color = colors[t.type] || '#667eea';

                // Glow
                ctx.shadowBlur = 25;
                ctx.shadowColor = color;

                // Base
                ctx.fillStyle = color;
                ctx.beginPath();
                ctx.arc(tx, ty, size, 0, Math.PI * 2);
                ctx.fill();

                ctx.shadowBlur = 0;

                // Top
                ctx.fillStyle = '#fff';
                ctx.beginPath();
                ctx.arc(tx, ty, size * 0.6, 0, Math.PI * 2);
                ctx.fill();
            });
        }

        // Enemies
        if (data.enemies) {
            data.enemies.forEach(e => {
                const ex = e.x * scaleX;
                const ey = e.y * scaleY;
                const esize = e.size * scaleX;

                // Glow
                ctx.shadowBlur = 15;
                ctx.shadowColor = '#ff4757';

                // Body
                ctx.fillStyle = '#ff4757';
                ctx.beginPath();
                ctx.arc(ex, ey, esize, 0, Math.PI * 2);
                ctx.fill();

                ctx.shadowBlur = 0;

                // HP Bar
                const barWidth = esize * 2;
                const barHeight = 5;
                const hpPercent = e.hp / e.maxHP;

                ctx.fillStyle = '#222';
                ctx.fillRect(ex - barWidth / 2, ey - esize - 15, barWidth, barHeight);

                const hpColor = hpPercent > 0.5 ? '#4ecb71' : (hpPercent > 0.25 ? '#ffa502' : '#ff4757');
                ctx.fillStyle = hpColor;
                ctx.fillRect(ex - barWidth / 2, ey - esize - 15, barWidth * hpPercent, barHeight);
            });
        }

        // Projectiles
        if (data.projectiles) {
            ctx.shadowBlur = 12;
            ctx.shadowColor = '#feca57';
            ctx.fillStyle = '#feca57';

            data.projectiles.forEach(p => {
                const px = p.x * scaleX;
                const py = p.y * scaleY;

                ctx.beginPath();
                ctx.arc(px, py, 6 * scaleX, 0, Math.PI * 2);
                ctx.fill();
            });

            ctx.shadowBlur = 0;
        }
    }
};