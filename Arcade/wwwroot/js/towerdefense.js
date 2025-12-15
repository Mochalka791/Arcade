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

        this.canvas.addEventListener('click', (e) => {
            const rect = this.canvas.getBoundingClientRect();
            const scaleX = 800 / rect.width;
            const scaleY = 600 / rect.height;
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
        const scaleX = w / 800;
        const scaleY = h / 600;

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

        // Path
        if (data.path && data.path.length > 0) {
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

                const colors = [
                    '#667eea', // Basic
                    '#f093fb', // Sniper
                    '#ff6b6b', // Cannon
                    '#4dd0e1', // Freeze
                    '#9c27b0', // Poison
                    '#ffd700'  // Lightning
                ];

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

                // Level indicator
                if (t.level > 1) {
                    ctx.fillStyle = '#FFD700';
                    ctx.font = `bold ${14 * scaleX}px Arial`;
                    ctx.textAlign = 'center';
                    ctx.textBaseline = 'middle';
                    ctx.fillText(`L${t.level}`, tx, ty - size - 10);
                }
            });
        }

        // Enemies
        if (data.enemies) {
            data.enemies.forEach(e => {
                const ex = e.x * scaleX;
                const ey = e.y * scaleY;
                const esize = e.size * scaleX;

                // Enemy color by type
                const enemyColors = [
                    '#ff4757', // Normal
                    '#ff6348', // Fast
                    '#5f27cd', // Tank
                    '#00d2d3', // Summoner
                    '#c23616', // BossBrute
                    '#192a56', // BossMage
                    '#273c75'  // BossSummoner
                ];

                const color = enemyColors[e.type] || '#ff4757';

                // Glow
                ctx.shadowBlur = e.isBoss ? 30 : 15;
                ctx.shadowColor = color;

                // Body
                ctx.fillStyle = color;
                ctx.beginPath();
                ctx.arc(ex, ey, esize, 0, Math.PI * 2);
                ctx.fill();

                ctx.shadowBlur = 0;

                // Status effects
                if (e.hasSlow) {
                    ctx.strokeStyle = '#4dd0e1';
                    ctx.lineWidth = 3;
                    ctx.beginPath();
                    ctx.arc(ex, ey, esize + 3, 0, Math.PI * 2);
                    ctx.stroke();
                }

                if (e.hasPoison) {
                    ctx.strokeStyle = '#9c27b0';
                    ctx.lineWidth = 3;
                    ctx.setLineDash([5, 5]);
                    ctx.beginPath();
                    ctx.arc(ex, ey, esize + 5, 0, Math.PI * 2);
                    ctx.stroke();
                    ctx.setLineDash([]);
                }

                // HP Bar
                const barWidth = esize * 2;
                const barHeight = 5;
                const hpPercent = e.hp / e.maxHP;

                ctx.fillStyle = '#222';
                ctx.fillRect(ex - barWidth / 2, ey - esize - 15, barWidth, barHeight);

                const hpColor = hpPercent > 0.5 ? '#4ecb71' : (hpPercent > 0.25 ? '#ffa502' : '#ff4757');
                ctx.fillStyle = hpColor;
                ctx.fillRect(ex - barWidth / 2, ey - esize - 15, barWidth * hpPercent, barHeight);

                // Boss crown
                if (e.isBoss) {
                    ctx.font = `${20 * scaleX}px Arial`;
                    ctx.textAlign = 'center';
                    ctx.fillText('👑', ex, ey - esize - 30);
                }
            });
        }

        // Projectiles
        if (data.projectiles) {
            data.projectiles.forEach(p => {
                const px = p.x * scaleX;
                const py = p.y * scaleY;

                const projectileColors = [
                    '#feca57', // Basic
                    '#ff6b81', // Sniper
                    '#ff4757', // Cannon
                    '#4dd0e1', // Freeze
                    '#9c27b0', // Poison
                    '#ffd700'  // Lightning
                ];

                const color = projectileColors[p.type] || '#feca57';

                ctx.shadowBlur = 12;
                ctx.shadowColor = color;
                ctx.fillStyle = color;

                ctx.beginPath();
                ctx.arc(px, py, 6 * scaleX, 0, Math.PI * 2);
                ctx.fill();

                ctx.shadowBlur = 0;
            });
        }

        // Effects
        if (data.effects) {
            data.effects.forEach(eff => {
                const alpha = eff.duration / 0.5;
                ctx.globalAlpha = alpha;

                if (eff.type === 'hit') {
                    ctx.fillStyle = '#ff4757';
                    ctx.beginPath();
                    ctx.arc(eff.x * scaleX, eff.y * scaleY, 15 * scaleX, 0, Math.PI * 2);
                    ctx.fill();
                }

                if (eff.type === 'death') {
                    ctx.fillStyle = '#ffd700';
                    ctx.font = `${20 * scaleX}px Arial`;
                    ctx.textAlign = 'center';
                    ctx.fillText('💀', eff.x * scaleX, eff.y * scaleY);
                }

                if (eff.type === 'summon') {
                    ctx.strokeStyle = '#00d2d3';
                    ctx.lineWidth = 3;
                    ctx.beginPath();
                    ctx.arc(eff.x * scaleX, eff.y * scaleY, 30 * scaleX * (1 - alpha), 0, Math.PI * 2);
                    ctx.stroke();
                }

                ctx.globalAlpha = 1;
            });
        }
    }
};