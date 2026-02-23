using System.Globalization;
using System.IO;
using FocusBuddy.Models;
using Microsoft.Data.Sqlite;
using Serilog;

namespace FocusBuddy.Data;

public sealed class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FocusBuddy");
        Directory.CreateDirectory(appData);
        var dbPath = Path.Combine(appData, "focusbuddy.db");
        _connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
    }

    public async Task InitializeAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS usage_sessions (
    id INTEGER PRIMARY KEY,
    process_name TEXT NOT NULL,
    window_title TEXT,
    category TEXT NOT NULL,
    start_time TEXT NOT NULL,
    end_time TEXT NOT NULL,
    duration_seconds INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS daily_summary (
    date TEXT NOT NULL,
    category TEXT NOT NULL,
    duration_seconds INTEGER NOT NULL,
    PRIMARY KEY(date, category)
);";

        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertUsageSessionAsync(UsageSession session)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO usage_sessions(process_name, window_title, category, start_time, end_time, duration_seconds)
VALUES($process, $title, $category, $start, $end, $duration);";

        command.Parameters.AddWithValue("$process", session.ProcessName);
        command.Parameters.AddWithValue("$title", session.WindowTitle);
        command.Parameters.AddWithValue("$category", session.Category);
        command.Parameters.AddWithValue("$start", session.StartTime.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$end", session.EndTime.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$duration", session.DurationSeconds);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> GetTodayTotalSecondsAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT COALESCE(SUM(duration_seconds), 0)
FROM usage_sessions
WHERE DATE(start_time) = DATE('now', 'localtime');";

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<List<DailyUsageSummary>> GetLast7DaysByCategoryAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT DATE(start_time) as usage_date, category, COALESCE(SUM(duration_seconds),0)
FROM usage_sessions
WHERE DATE(start_time) >= DATE('now', 'localtime', '-6 day')
GROUP BY usage_date, category
ORDER BY usage_date ASC;";

        var output = new List<DailyUsageSummary>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            output.Add(new DailyUsageSummary
            {
                Date = DateOnly.Parse(reader.GetString(0), CultureInfo.InvariantCulture),
                Category = reader.GetString(1),
                DurationSeconds = reader.GetInt32(2)
            });
        }

        return output;
    }

    public async Task<List<DailyUsageSummary>> GetTodayByCategoryAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT category, COALESCE(SUM(duration_seconds),0)
FROM usage_sessions
WHERE DATE(start_time) = DATE('now', 'localtime')
GROUP BY category
ORDER BY 2 DESC;";

        var output = new List<DailyUsageSummary>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            output.Add(new DailyUsageSummary
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Category = reader.GetString(0),
                DurationSeconds = reader.GetInt32(1)
            });
        }

        return output;
    }

    public async Task<List<AppUsageSummary>> GetTopAppsTodayAsync(int take = 5)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT process_name, COALESCE(SUM(duration_seconds),0) as total
FROM usage_sessions
WHERE DATE(start_time) = DATE('now', 'localtime')
GROUP BY process_name
ORDER BY total DESC
LIMIT $take;";
        command.Parameters.AddWithValue("$take", take);

        var output = new List<AppUsageSummary>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            output.Add(new AppUsageSummary
            {
                ProcessName = reader.GetString(0),
                DurationSeconds = reader.GetInt32(1)
            });
        }

        return output;
    }
}
