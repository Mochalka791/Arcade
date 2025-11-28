using Arcade.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Arcade.Data;

namespace Arcade.Data.Services;
// DTO
public class MinesweeperSummary
{
    public int TotalGames { get; set; }
    public int Wins { get; set; }
    public int Losses => TotalGames - Wins;

    public double BestTime { get; set; }
    public double AverageTime { get; set; }

    public double BestEfficiency { get; set; }
}

public interface IMinesweeperStatsService
{
    Task AddRecordAsync(MinesweeperStats record);
    Task<List<MinesweeperStats>> GetTop10Async(string difficulty);
    Task<MinesweeperSummary> GetUserSummaryAsync(string userId);
}

public class MinesweeperStatsService : IMinesweeperStatsService
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

        if (!records.Any())
        {
            return new MinesweeperSummary();
        }

        var wins = records.Where(x => x.Won).ToList();

        return new MinesweeperSummary
        {
            TotalGames = records.Count,
            Wins = wins.Count,

            BestTime = wins.Any() ? wins.Min(x => x.TimeSeconds) : 0,

            AverageTime = records.Average(x => x.TimeSeconds),
            BestEfficiency = records.Max(x => x.EfficiencyPercent)
        };
    }
}
