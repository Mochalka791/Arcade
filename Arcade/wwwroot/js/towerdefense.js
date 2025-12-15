window.towerDefense = {
    canvas: null,
    ctx: null,
    dotNetRef: null,

    init() {
        this.canvas = document.getElementById('gameCanvas');
        if (!this.canvas) return;

        this.resizeCanvas();
        window.addEventListener('resize', () => this.resizeCanvas());

        this.ctx = this.canvas.getContext('2d', { alpha: false });

        this.canvas.addEventListener('click', (e) => {
            const rect = this.canvas.getBoundingClientRect();
            const scaleX = 1600 / rect.width;
            const scaleY = 900 / rect.height;
            const x = (e.clientX - rect.left) * scaleX;
            const y = (e.clientY - rect.top) * scaleY;

            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnCanvasClick', x, y);
            }
        });
    },

    setDotNetRef(ref) {
        this.dotNetRef = ref;
    },

    resizeCanvas() {
        const container = this.canvas.parentElement;
        const aspectRatio = 1600 / 900;

        let width = container.clientWidth;
        let height = width / aspectRatio;

        if (height > container.clientHeight) {
            height = container.clientHeight;
            width = height * aspectRatio;
        }

        this.canvas.width = 1600;
        this.canvas.height = 900;
        this.canvas.style.width = width + 'px';
        this.canvas.style.height = height + 'px';
    },

    render(data) {
        if (!this.ctx) return;

        const ctx = this.ctx;
        const w = this.canvas.width;
        const h = this.canvas.height;

        ctx.fillStyle = '#0a0e14';
        ctx.fillRect(0, 0, w, h);

        ctx.strokeStyle = 'rgba(102, 126, 234, 0.03)';
        ctx.lineWidth = 1;
        for (let x = 0; x < w; x += 100) {
            ctx.beginPath();
            ctx.moveTo(x, 0);
            ctx.lineTo(x, h);
            ctx.stroke();
        }
        for (let y = 0; y < h; y += 100) {
            ctx.beginPath();
            ctx.moveTo(0, y);
            ctx.lineTo(w, y);
            ctx.stroke();
        }

        if (data.paths) {
            data.paths.forEach((path, idx) => {
                const isActive = data.activePaths && data.activePaths.includes(idx);

                ctx.strokeStyle = isActive ? '#5a4a3a' : '#2a2a2a';
                ctx.lineWidth = 70;
                ctx.lineCap = 'round';
                ctx.lineJoin = 'round';
                ctx.beginPath();
                path.forEach((p, i) => {
                    if (i === 0) ctx.moveTo(p.x, p.y);
                    else ctx.lineTo(p.x, p.y);
                });
                ctx.stroke();

                if (isActive) {
                    const entry = path[0];
                    ctx.fillStyle = '#4ade80';
                    ctx.font = 'bold 24px Arial';
                    ctx.textAlign = 'center';
                    ctx.fillText('➤', entry.x, entry.y);
                }
            });

            if (data.activePaths && data.activePaths.length > 0) {
                const lastPath = data.paths[data.activePaths[0]];
                const exit = lastPath[lastPath.length - 1];
                ctx.fillStyle = '#ff4757';
                ctx.font = 'bold 24px Arial';
                ctx.textAlign = 'center';
                ctx.fillText('END', exit.x, exit.y);
            }
        }

        // Tower Ranges - zeige immer wenn showTowerRanges aktiviert oder Tower ausgewählt
        if (data.towers && (data.showTowerRanges || data.selectedTower !== null)) {
            data.towers.forEach(t => {
                ctx.fillStyle = 'rgba(102, 126, 234, 0.06)';
                ctx.strokeStyle = 'rgba(102, 126, 234, 0.4)';
                ctx.lineWidth = 2;
                ctx.beginPath();
                ctx.arc(t.x, t.y, t.range, 0, Math.PI * 2);
                ctx.fill();
                ctx.stroke();
            });
        }

        if (data.towers) {
            data.towers.forEach(t => {
                const size = 18;
                const colors = ['#667eea', '#f093fb', '#ff6b6b', '#4dd0e1', '#9c27b0', '#ffd700'];
                ctx.fillStyle = colors[t.type] || '#667eea';

                ctx.beginPath();
                ctx.arc(t.x, t.y, size, 0, Math.PI * 2);
                ctx.fill();

                ctx.strokeStyle = 'rgba(255, 255, 255, 0.3)';
                ctx.lineWidth = 2;
                ctx.stroke();

                if (t.level > 1) {
                    ctx.fillStyle = '#FFD700';
                    ctx.font = 'bold 12px Arial';
                    ctx.textAlign = 'center';
                    ctx.fillText(`★${t.level}`, t.x, t.y - size - 6);
                }
            });
        }

        if (data.enemies) {
            data.enemies.forEach(e => {
                const colors = ['#ff4757', '#ff6348', '#5f27cd', '#00d2d3', '#8b0000', '#191970', '#2f1d5c'];
                ctx.fillStyle = colors[e.type] || '#ff4757';

                ctx.beginPath();
                ctx.arc(e.x, e.y, e.size, 0, Math.PI * 2);
                ctx.fill();

                if (e.isBoss) {
                    ctx.strokeStyle = '#ffd700';
                    ctx.lineWidth = 3;
                    ctx.stroke();
                }

                const barWidth = e.size * 2.2;
                const hpPercent = e.hp / e.maxHP;
                const hpColor = hpPercent > 0.5 ? '#4ade80' : '#ff4757';

                ctx.fillStyle = 'rgba(0, 0, 0, 0.8)';
                ctx.fillRect(e.x - barWidth / 2, e.y - e.size - 8, barWidth, 3);
                ctx.fillStyle = hpColor;
                ctx.fillRect(e.x - barWidth / 2, e.y - e.size - 8, barWidth * hpPercent, 3);
            });
        }

        if (data.projectiles) {
            data.projectiles.forEach(p => {
                const colors = ['#feca57', '#ff6b81', '#ff4757', '#4dd0e1', '#9c27b0', '#ffd700'];
                ctx.fillStyle = colors[p.type] || '#feca57';
                ctx.beginPath();
                ctx.arc(p.x, p.y, 5, 0, Math.PI * 2);
                ctx.fill();
            });
        }

        if (data.effects && data.effects.length < 50) {
            data.effects.forEach(eff => {
                if (eff.type === 'death') {
                    ctx.fillStyle = '#ffd700';
                    ctx.font = '18px Arial';
                    ctx.textAlign = 'center';
                    ctx.fillText('💀', eff.x, eff.y);
                }
            });
        }
    }
};