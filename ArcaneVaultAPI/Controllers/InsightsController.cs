using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

[Route("api/[controller]")]
[ApiController]
public class InsightsController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    // GET: api/Insights/popularitems
    [HttpGet("popularitems")]
    public ActionResult GetPopularItems()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Top 10 items by distinct owning users
        var command = new SqliteCommand(
            @"SELECT ci.ItemName,
                     COUNT(DISTINCT ci.UserName) as OwnerCount,
                     c.CategoryName
              FROM CollectionItems ci
              LEFT JOIN CollectionItemCategories cic ON ci.ItemId = cic.ItemId
              LEFT JOIN Categories c ON cic.CategoryCode = c.CategoryCode
              WHERE ci.IsDeleted = 0
              GROUP BY ci.ItemName
              ORDER BY OwnerCount DESC
              LIMIT 10",
            connection);

        var results = new List<object>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new
            {
                ItemName = reader["ItemName"].ToString(),
                OwnerCount = Convert.ToInt32(reader["OwnerCount"]),
                CategoryName = reader["CategoryName"] == DBNull.Value
                    ? null : reader["CategoryName"].ToString()
            });
        }

        return Ok(results);
    }

    // GET: api/Insights/popularitems/categories — category breakdown for drill-down
    [HttpGet("popularitems/categories")]
    public ActionResult GetPopularItemsByCategory()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT c.CategoryName,
                     COUNT(DISTINCT ci.ItemName) as ItemCount,
                     COUNT(DISTINCT ci.UserName) as OwnerCount
              FROM CollectionItems ci
              LEFT JOIN CollectionItemCategories cic ON ci.ItemId = cic.ItemId
              LEFT JOIN Categories c ON cic.CategoryCode = c.CategoryCode
              WHERE ci.IsDeleted = 0
              GROUP BY c.CategoryName
              ORDER BY OwnerCount DESC",
            connection);

        var results = new List<object>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new
            {
                CategoryName = reader["CategoryName"] == DBNull.Value
                    ? "Uncategorized" : reader["CategoryName"].ToString(),
                ItemCount = Convert.ToInt32(reader["ItemCount"]),
                OwnerCount = Convert.ToInt32(reader["OwnerCount"])
            });
        }

        return Ok(results);
    }

    // GET: api/Insights/marketdemand?days=7
    [HttpGet("marketdemand")]
    public ActionResult GetMarketDemand([FromQuery] int days = 7)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Items added in window vs before window
        var command = new SqliteCommand(
            @"SELECT ci.ItemName,
                     COUNT(DISTINCT CASE WHEN ci.ItemId > 0 THEN ci.UserName END) as CurrentOwners,
                     c.CategoryName
              FROM CollectionItems ci
              LEFT JOIN CollectionItemCategories cic ON ci.ItemId = cic.ItemId
              LEFT JOIN Categories c ON cic.CategoryCode = c.CategoryCode
              WHERE ci.IsDeleted = 0
              GROUP BY ci.ItemName
              ORDER BY CurrentOwners DESC
              LIMIT 10",
            connection);

        var results = new List<object>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new
            {
                ItemName = reader["ItemName"].ToString(),
                CurrentOwners = Convert.ToInt32(reader["CurrentOwners"]),
                CategoryName = reader["CategoryName"] == DBNull.Value
                    ? null : reader["CategoryName"].ToString()
            });
        }

        return Ok(results);
    }

    // GET: api/Insights/marketdemand/overtime?days=7
    [HttpGet("marketdemand/overtime")]
    public ActionResult GetMarketDemandOverTime([FromQuery] int days = 7)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Daily new items added over the window
        var command = new SqliteCommand(
            @"SELECT DATE(ll.LoggedInAt) as Date,
                     COUNT(DISTINCT ll.UserName) as ActiveUsers
              FROM LoginLogs ll
              WHERE ll.LoggedInAt >= @Since
              GROUP BY DATE(ll.LoggedInAt)
              ORDER BY Date ASC",
            connection);
        command.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-days));

        var results = new List<object>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new
            {
                Date = reader["Date"].ToString(),
                ActiveUsers = Convert.ToInt32(reader["ActiveUsers"])
            });
        }

        return Ok(results);
    }

    // GET: api/Insights/useractivity
    [HttpGet("useractivity")]
    public ActionResult GetUserActivity()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var users = new List<object>();
        var userCommand = new SqliteCommand(
            @"SELECT u.UserName,
                     COUNT(DISTINCT ll.LogId) as TotalLogins,
                     COUNT(DISTINCT s.ShowoffId) as TotalShowoffs,
                     MAX(ll.LoggedInAt) as LastLogin
              FROM ArcaneVaultUsers u
              LEFT JOIN LoginLogs ll ON u.UserName = ll.UserName
              LEFT JOIN Showoffs s ON u.UserName = s.UserName AND s.IsDeleted = 0
              WHERE u.IsDeleted = 0
              GROUP BY u.UserName
              ORDER BY TotalLogins DESC",
            connection);

        using var reader = userCommand.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new
            {
                UserName = reader["UserName"].ToString(),
                TotalLogins = Convert.ToInt32(reader["TotalLogins"]),
                TotalShowoffs = Convert.ToInt32(reader["TotalShowoffs"]),
                LastLogin = reader["LastLogin"] == DBNull.Value
                    ? null : reader["LastLogin"].ToString()
            });
        }

        return Ok(users);
    }

    // GET: api/Insights/communityinterest
    [HttpGet("communityinterest")]
    public ActionResult GetCommunityInterest()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Most viewed showoffs
        var viewsCommand = new SqliteCommand(
            @"SELECT s.ShowoffName, s.UserName,
                     COUNT(sv.ViewId) as ViewCount,
                     COUNT(sl.UserName) as LikeCount
              FROM Showoffs s
              LEFT JOIN ShowoffViews sv ON s.ShowoffId = sv.ShowoffId
              LEFT JOIN ShowoffLikes sl ON s.ShowoffId = sl.ShowoffId
              WHERE s.IsDeleted = 0 AND s.IsPrivate = 0
              GROUP BY s.ShowoffId
              ORDER BY ViewCount DESC
              LIMIT 10",
            connection);

        var mostViewed = new List<object>();
        using var viewsReader = viewsCommand.ExecuteReader();
        while (viewsReader.Read())
        {
            mostViewed.Add(new
            {
                ShowoffName = viewsReader["ShowoffName"].ToString(),
                UserName = viewsReader["UserName"].ToString(),
                ViewCount = Convert.ToInt32(viewsReader["ViewCount"]),
                LikeCount = Convert.ToInt32(viewsReader["LikeCount"])
            });
        }

        // Most liked showoffs — separate query ordered by LikeCount
        var likesCommand = new SqliteCommand(
            @"SELECT s.ShowoffName, s.UserName,
                 COUNT(sl.UserName) as LikeCount,
                 COUNT(sv.ViewId) as ViewCount
          FROM Showoffs s
          LEFT JOIN ShowoffLikes sl ON s.ShowoffId = sl.ShowoffId
          LEFT JOIN ShowoffViews sv ON s.ShowoffId = sv.ShowoffId
          WHERE s.IsDeleted = 0 AND s.IsPrivate = 0
          GROUP BY s.ShowoffId
          ORDER BY LikeCount DESC
          LIMIT 10",
            connection);

        var mostLiked = new List<object>();
        using var likesReader = likesCommand.ExecuteReader();
        while (likesReader.Read())
        {
            mostLiked.Add(new
            {
                ShowoffName = likesReader["ShowoffName"].ToString(),
                UserName = likesReader["UserName"].ToString(),
                LikeCount = Convert.ToInt32(likesReader["LikeCount"]),
                ViewCount = Convert.ToInt32(likesReader["ViewCount"])
            });
        }

        // Category popularity by user count
        var catCommand = new SqliteCommand(
            @"SELECT c.CategoryName,
                     COUNT(DISTINCT ci.UserName) as UserCount
              FROM Categories c
              JOIN CollectionItemCategories cic ON c.CategoryCode = cic.CategoryCode
              JOIN CollectionItems ci ON cic.ItemId = ci.ItemId
              WHERE ci.IsDeleted = 0
              GROUP BY c.CategoryName
              ORDER BY UserCount DESC",
            connection);

        var categoryPopularity = new List<object>();
        using var catReader = catCommand.ExecuteReader();
        while (catReader.Read())
        {
            categoryPopularity.Add(new
            {
                CategoryName = catReader["CategoryName"].ToString(),
                UserCount = Convert.ToInt32(catReader["UserCount"])
            });
        }

        return Ok(new { MostViewed = mostViewed, CategoryPopularity = categoryPopularity });
    }

    // GET: api/Insights/communityinterest/overtime?days=30
    [HttpGet("communityinterest/overtime")]
    public ActionResult GetCommunityInterestOverTime([FromQuery] int days = 30)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT DATE(sv.ViewedAt) as Date,
                     COUNT(sv.ViewId) as ViewCount
              FROM ShowoffViews sv
              JOIN Showoffs s ON sv.ShowoffId = s.ShowoffId
              WHERE s.IsPrivate = 0 AND s.IsDeleted = 0
              AND sv.ViewedAt >= @Since
              GROUP BY DATE(sv.ViewedAt)
              ORDER BY Date ASC",
            connection);
        command.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-days));

        var results = new List<object>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new
            {
                Date = reader["Date"].ToString(),
                ViewCount = Convert.ToInt32(reader["ViewCount"])
            });
        }

        return Ok(results);
    }

    // GET: api/Insights/marketdemand/detail
    [HttpGet("marketdemand/detail")]
    public ActionResult GetMarketDemandDetail()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT ci.ItemName,
                 c.CategoryName,
                 SUM(ci.StartingQuantity) as TotalStart,
                 SUM(ci.CurrentQuantity) as TotalCurrent,
                 COUNT(DISTINCT ci.UserName) as OwnerCount
          FROM CollectionItems ci
          LEFT JOIN CollectionItemCategories cic ON ci.ItemId = cic.ItemId
          LEFT JOIN Categories c ON cic.CategoryCode = c.CategoryCode
          WHERE ci.IsDeleted = 0
          GROUP BY ci.ItemName
          ORDER BY (SUM(ci.CurrentQuantity) - SUM(ci.StartingQuantity)) DESC",
            connection);

        var results = new List<object>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var start = Convert.ToInt32(reader["TotalStart"]);
            var current = Convert.ToInt32(reader["TotalCurrent"]);
            var pct = start == 0 ? 0 : (int)Math.Round(((double)(current - start) / start) * 100);

            results.Add(new
            {
                Name = reader["ItemName"].ToString(),
                Category = reader["CategoryName"] == DBNull.Value
                                ? "Uncategorized" : reader["CategoryName"].ToString(),
                Start = start,
                Current = current,
                OwnerCount = Convert.ToInt32(reader["OwnerCount"]),
                Pct = pct
            });
        }

        return Ok(results);
    }
}