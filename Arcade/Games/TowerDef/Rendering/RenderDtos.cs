namespace Arcade.Games.TowerDef.Rendering;

public class RenderData
{
    public List<PathDto> path { get; init; } = new();
    public List<EnemyDto> enemies { get; init; } = new();
    public List<TowerDto> towers { get; init; } = new();
    public List<ProjectileDto> projectiles { get; init; } = new();
}

/* ---------- PATH ---------- */

public class PathDto
{
    public float x { get; init; }
    public float y { get; init; }
}

/* ---------- ENEMY ---------- */

public class EnemyDto
{
    public float x { get; init; }
    public float y { get; init; }

    public int hp { get; init; }
    public int maxHP { get; init; }

    public float size { get; init; }
    public int type { get; init; }

    public bool isBoss { get; init; }
    public bool hasPoison { get; init; }
    public bool hasSlow { get; init; }
}

/* ---------- TOWER ---------- */

public class TowerDto
{
    public float x { get; init; }
    public float y { get; init; }

    public int type { get; init; }
    public int level { get; init; }
    public float range { get; init; }
}

/* ---------- PROJECTILE ---------- */

public class ProjectileDto
{
    public float x { get; init; }
    public float y { get; init; }

    public int type { get; init; }
}