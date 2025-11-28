public sealed class MinesweeperEngine
{
    public int Width { get; }
    public int Height { get; }
    public int Mines { get; }

    public Cell[,] Grid { get; }

    public bool IsGameOver { get; private set; }
    public bool IsWin { get; private set; }

    public int RevealedSafeCells { get; private set; }
    public int TotalSafeCells => (Width * Height) - Mines;

    private readonly Random _rng = new();

    public MinesweeperEngine(int width, int height, int mines)
    {
        Width = width;
        Height = height;
        Mines = mines;
        Grid = new Cell[Width, Height];

        Init();
    }

    public void Init()
    {
        IsGameOver = false;
        IsWin = false;
        RevealedSafeCells = 0;

        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                Grid[x, y] = new Cell(x, y);

        int placed = 0;
        while (placed < Mines)
        {
            int x = _rng.Next(Width);
            int y = _rng.Next(Height);
            var cell = Grid[x, y];

            if (!cell.IsMine)
            {
                cell.IsMine = true;
                placed++;
            }
        }

        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                if (!Grid[x, y].IsMine)
                    Grid[x, y].Adjacent = CountAdjacent(x, y);
    }

    private int CountAdjacent(int x, int y)
    {
        int c = 0;
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx, ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= Width || ny >= Height) continue;
                if (Grid[nx, ny].IsMine) c++;
            }
        return c;
    }

    public void Reveal(Cell c)
    {
        if (IsGameOver || c.IsFlagged || c.IsRevealed)
            return;

        c.IsRevealed = true;

        if (c.IsMine)
        {
            IsGameOver = true;
            IsWin = false;
            RevealAllMines();
            return;
        }

        RevealedSafeCells++;

        if (c.Adjacent == 0)
            Flood(c);

        if (RevealedSafeCells == TotalSafeCells)
        {
            IsGameOver = true;
            IsWin = true;
        }
    }

    private void RevealAllMines()
    {
        foreach (var c in Grid)
        {
            if (c.IsMine)
                c.IsRevealed = true;
        }
    }


    private void Flood(Cell c)
    {
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = c.X + dx, ny = c.Y + dy;
                if (nx < 0 || ny < 0 || nx >= Width || ny >= Height) continue;

                var n = Grid[nx, ny];
                if (!n.IsMine && !n.IsRevealed && !n.IsFlagged)
                    Reveal(n);
            }
    }

    public void ToggleFlag(Cell c)
    {
        if (IsGameOver || c.IsRevealed) return;
        c.IsFlagged = !c.IsFlagged;
    }

    public void ForceWin()
    {
        foreach (var c in Grid)
        {
            if (!c.IsMine)
                c.IsRevealed = true;
        }

        RevealedSafeCells = TotalSafeCells;
        IsGameOver = true;
        IsWin = true;
    }
}

public sealed class Cell
{
    public int X { get; }
    public int Y { get; }

    public bool IsMine { get; set; }
    public bool IsRevealed { get; set; }
    public bool IsFlagged { get; set; }

    public int Adjacent { get; set; }

    public Cell(int x, int y)
    {
        X = x; Y = y;
    }
}
