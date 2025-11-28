using Arcade.Data;
using Arcade.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arcade.Data.Services;

public class MinesweeperSummary
{
    public int TotalGames { get; set; }
    public int Wins { get; set; }
    public int Losses => TotalGames - Wins;

    public double WinRatePercent => TotalGames == 0
        ? 0
        : (double)Wins / TotalGames * 100.0;

    /// <summary>
    /// Summe aller erspielten Punkte (abhängig von Difficulty).
    /// </summary>
    public int Points { get; set; }
}

public interface IMinesweeperStatsService
{
    /// <summary>
    /// Speichert einen einzelnen Minesweeper-Run (Win oder Loss).
    /// </summary>
    Task AddRecordAsync(MinesweeperStats record);

    /// <summary>
    /// Top 10 für eine bestimmte Difficulty, sortiert nach Zeit (nur Wins).
    /// </summary>
    Task<List<MinesweeperStats>> GetTop10Async(string difficulty);

    /// <summary>
    /// Zusammenfassung für das Profil eines Users:
    /// Spiele gesamt, Wins, Losses, Winrate, Punkte.
    /// </summary>
    Task<MinesweeperSummary> GetUserSummaryAsync(string userId);
}

public sealed class MinesweeperStatsService : IMinesweeperStatsService
{
    private readonly ArcadeDbContext _db;

    public MinesweeperStatsService(ArcadeDbContext db)
    {
        _db = db;
    }

    public async Task AddRecordAsync(MinesweeperStats record)
    {
        _db.MinesweeperStats.Add(record);
        await _db.SaveChangesAsync();
    }

    public async Task<List<MinesweeperStats>> GetTop10Async(string difficulty)
    {
        return await _db.MinesweeperStats
            .Where(x => x.Won && x.Difficulty == difficulty)
            .OrderBy(x => x.TimeSeconds)
            .Take(10)
            .ToListAsync();
    }

    public async Task<MinesweeperSummary> GetUserSummaryAsync(string userId)
    {
        var records = await _db.MinesweeperStats
            .Where(x => x.UserId == userId)
            .ToListAsync();

        if (records.Count == 0)
        {
            return new MinesweeperSummary();
        }

        var wins = records.Count(x => x.Won);
        var points = records.Sum(x => x.Points);

        return new MinesweeperSummary
        {
            TotalGames = records.Count,
            Wins = wins,
            Points = points
        };
    }
}
