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

    // GET: api/Insights/popularitems/detail
    [HttpGet("popularitems/detail")]
    public ActionResult GetPopularItemsDetail()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT ci.ItemName,
                 c.CategoryName,
                 COUNT(DISTINCT ci.UserName) as Owners,
                 SUM(ci.CurrentQuantity) as TotalQty
          FROM CollectionItems ci
          LEFT JOIN CollectionItemCategories cic ON ci.ItemId = cic.ItemId
          LEFT JOIN Categories c ON cic.CategoryCode = c.CategoryCode
          WHERE ci.IsDeleted = 0
          GROUP BY ci.ItemName
          ORDER BY Owners DESC",
            connection);

        var results = new List<object>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var owners = Convert.ToInt32(reader["Owners"]);
            var qty = Convert.ToInt32(reader["TotalQty"]);
            results.Add(new
            {
                Name = reader["ItemName"].ToString(),
                Category = reader["CategoryName"] == DBNull.Value
                            ? "Uncategorized" : reader["CategoryName"].ToString(),
                Owners = owners,
                Qty = qty,
                Ratio = owners == 0 ? 0.0 : Math.Round((double)qty / owners, 1)
            });
        }

        return Ok(results);
    }

    // GET: api/Insights/useractivity/detail?days=30
    [HttpGet("useractivity/detail")]
    public ActionResult GetUserActivityDetail([FromQuery] int days = 30)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Login activity over time
        var activityCommand = new SqliteCommand(
            @"SELECT DATE(LoggedInAt) as Date, COUNT(DISTINCT UserName) as Count
          FROM LoginLogs
          WHERE LoggedInAt >= @Since
          GROUP BY DATE(LoggedInAt)
          ORDER BY Date ASC",
            connection);
        activityCommand.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-days));

        var loginActivity = new List<object>();
        using var actReader = activityCommand.ExecuteReader();
        while (actReader.Read())
        {
            loginActivity.Add(new
            {
                Date = actReader["Date"].ToString(),
                Count = Convert.ToInt32(actReader["Count"])
            });
        }

        // Day of week breakdown
        var dowCommand = new SqliteCommand(
            @"SELECT STRFTIME('%w', LoggedInAt) as DayOfWeek, COUNT(*) as Count
          FROM LoginLogs
          WHERE LoggedInAt >= @Since
          GROUP BY DayOfWeek
          ORDER BY DayOfWeek",
            connection);
        dowCommand.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-days));

        var dowData = new int[7];
        using var dowReader = dowCommand.ExecuteReader();
        while (dowReader.Read())
        {
            var idx = Convert.ToInt32(dowReader["DayOfWeek"]);
            dowData[idx] = Convert.ToInt32(dowReader["Count"]);
        }

        // Month breakdown
        var monthCommand = new SqliteCommand(
            @"SELECT STRFTIME('%m', LoggedInAt) as Month, COUNT(*) as Count
          FROM LoginLogs
          WHERE LoggedInAt >= @Since
          GROUP BY Month
          ORDER BY Month",
            connection);
        monthCommand.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-days));

        var monthData = new int[12];
        using var monthReader = monthCommand.ExecuteReader();
        while (monthReader.Read())
        {
            var idx = Convert.ToInt32(monthReader["Month"]) - 1;
            monthData[idx] = Convert.ToInt32(monthReader["Count"]);
        }

        // Per-user breakdown
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

        var users = new List<object>();
        using var userReader = userCommand.ExecuteReader();
        while (userReader.Read())
        {
            users.Add(new
            {
                Name = userReader["UserName"].ToString(),
                Logins = Convert.ToInt32(userReader["TotalLogins"]),
                Showoffs = Convert.ToInt32(userReader["TotalShowoffs"]),
                LastLogin = userReader["LastLogin"] == DBNull.Value
                                ? null : userReader["LastLogin"].ToString()
            });
        }

        return Ok(new
        {
            LoginActivity = loginActivity,
            DayOfWeek = dowData,
            MonthData = monthData,
            Users = users
        });
    }

    // GET: api/Insights/countrydemographic
    [HttpGet("countrydemographic")]
    public ActionResult GetCountryDemographic()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT COALESCE(Country, 'Not specified') as Country,
                 COUNT(*) as UserCount
          FROM ArcaneVaultUsers
          WHERE IsDeleted = 0
          GROUP BY Country
          ORDER BY UserCount DESC",
            connection);

        var results = new List<object>();
        int total = 0;

        using var reader = command.ExecuteReader();
        var rows = new List<(string Country, int Count)>();
        while (reader.Read())
        {
            var country = reader["Country"].ToString();
            var count = Convert.ToInt32(reader["UserCount"]);
            rows.Add((country, count));
            total += count;
        }

        foreach (var (country, count) in rows)
        {
            results.Add(new
            {
                Country = country,
                UserCount = count,
                Percentage = total > 0 ? Math.Round((double)count / total * 100, 1) : 0
            });
        }

        return Ok(results);
    }

    // GET: api/Insights/communityinterest/detail?days=7
    [HttpGet("communityinterest/detail")]
    public ActionResult GetCommunityInterestDetail([FromQuery] int days = 7)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Views over time
        var viewsCommand = new SqliteCommand(
            @"SELECT DATE(sv.ViewedAt) as Date, COUNT(sv.ViewId) as Count
          FROM ShowoffViews sv
          JOIN Showoffs s ON sv.ShowoffId = s.ShowoffId
          WHERE s.IsPrivate = 0 AND s.IsDeleted = 0
          AND sv.ViewedAt >= @Since
          GROUP BY DATE(sv.ViewedAt)
          ORDER BY Date ASC",
            connection);
        viewsCommand.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-days));

        var viewsOverTime = new List<object>();
        using var vReader = viewsCommand.ExecuteReader();
        while (vReader.Read())
        {
            viewsOverTime.Add(new
            {
                Date = vReader["Date"].ToString(),
                Count = Convert.ToInt32(vReader["Count"])
            });
        }

        // Total views in period
        var totalViewsCmd = new SqliteCommand(
            @"SELECT COUNT(sv.ViewId) as Total
          FROM ShowoffViews sv
          JOIN Showoffs s ON sv.ShowoffId = s.ShowoffId
          WHERE s.IsPrivate = 0 AND s.IsDeleted = 0
          AND sv.ViewedAt >= @Since",
            connection);
        totalViewsCmd.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-days));
        var totalViews = Convert.ToInt32(totalViewsCmd.ExecuteScalar());

        // Total likes in period — all time since no timestamp on likes
        var totalLikesCmd = new SqliteCommand(
            @"SELECT COUNT(*) FROM ShowoffLikes sl
          JOIN Showoffs s ON sl.ShowoffId = s.ShowoffId
          WHERE s.IsPrivate = 0 AND s.IsDeleted = 0",
            connection);
        var totalLikes = Convert.ToInt32(totalLikesCmd.ExecuteScalar());

        // Most viewed showoffs
        var mostViewedCmd = new SqliteCommand(
            @"SELECT s.ShowoffId, s.ShowoffName, s.UserName,
                 COUNT(sv.ViewId) as ViewCount,
                 COUNT(DISTINCT sl.UserName) as LikeCount
          FROM Showoffs s
          LEFT JOIN ShowoffViews sv ON s.ShowoffId = sv.ShowoffId
              AND sv.ViewedAt >= @Since
          LEFT JOIN ShowoffLikes sl ON s.ShowoffId = sl.ShowoffId
          WHERE s.IsPrivate = 0 AND s.IsDeleted = 0
          GROUP BY s.ShowoffId
          ORDER BY ViewCount DESC
          LIMIT 6",
            connection);
        mostViewedCmd.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-days));

        var mostViewed = new List<object>();
        using var mvReader = mostViewedCmd.ExecuteReader();
        while (mvReader.Read())
        {
            var views = Convert.ToInt32(mvReader["ViewCount"]);
            var likes = Convert.ToInt32(mvReader["LikeCount"]);
            mostViewed.Add(new
            {
                ShowoffId = Convert.ToInt32(mvReader["ShowoffId"]),
                ShowoffName = mvReader["ShowoffName"].ToString(),
                UserName = mvReader["UserName"].ToString(),
                ViewCount = views,
                LikeCount = likes,
                LikeRate = views > 0 ? Math.Round((double)likes / views * 100, 1) : 0
            });
        }

        // Most liked showoffs
        var mostLikedCmd = new SqliteCommand(
            @"SELECT s.ShowoffId, s.ShowoffName, s.UserName,
                 COUNT(DISTINCT sl.UserName) as LikeCount,
                 COUNT(sv.ViewId) as ViewCount
          FROM Showoffs s
          LEFT JOIN ShowoffLikes sl ON s.ShowoffId = sl.ShowoffId
          LEFT JOIN ShowoffViews sv ON s.ShowoffId = sv.ShowoffId
              AND sv.ViewedAt >= @Since
          WHERE s.IsPrivate = 0 AND s.IsDeleted = 0
          GROUP BY s.ShowoffId
          ORDER BY LikeCount DESC
          LIMIT 6",
            connection);
        mostLikedCmd.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-days));

        var mostLiked = new List<object>();
        using var mlReader = mostLikedCmd.ExecuteReader();
        while (mlReader.Read())
        {
            var views = Convert.ToInt32(mlReader["ViewCount"]);
            var likes = Convert.ToInt32(mlReader["LikeCount"]);
            mostLiked.Add(new
            {
                ShowoffId = Convert.ToInt32(mlReader["ShowoffId"]),
                ShowoffName = mlReader["ShowoffName"].ToString(),
                UserName = mlReader["UserName"].ToString(),
                ViewCount = views,
                LikeCount = likes,
                LikeRate = views > 0 ? Math.Round((double)likes / views * 100, 1) : 0
            });
        }

        // All showoffs for comparison table
        var allShowoffsCmd = new SqliteCommand(
            @"SELECT s.ShowoffId, s.ShowoffName, s.UserName,
                 COUNT(sv.ViewId) as ViewCount,
                 COUNT(DISTINCT sl.UserName) as LikeCount
          FROM Showoffs s
          LEFT JOIN ShowoffViews sv ON s.ShowoffId = sv.ShowoffId
              AND sv.ViewedAt >= @Since
          LEFT JOIN ShowoffLikes sl ON s.ShowoffId = sl.ShowoffId
          WHERE s.IsPrivate = 0 AND s.IsDeleted = 0
          GROUP BY s.ShowoffId
          ORDER BY ViewCount DESC",
            connection);
        allShowoffsCmd.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-days));

        var allShowoffs = new List<object>();
        using var asReader = allShowoffsCmd.ExecuteReader();
        while (asReader.Read())
        {
            var views = Convert.ToInt32(asReader["ViewCount"]);
            var likes = Convert.ToInt32(asReader["LikeCount"]);
            allShowoffs.Add(new
            {
                ShowoffId = Convert.ToInt32(asReader["ShowoffId"]),
                ShowoffName = asReader["ShowoffName"].ToString(),
                UserName = asReader["UserName"].ToString(),
                ViewCount = views,
                LikeCount = likes,
                LikeRate = views > 0 ? Math.Round((double)likes / views * 100, 1) : 0
            });
        }

        // Most searched terms in period
        var searchCmd = new SqliteCommand(
            @"SELECT SearchTerm, COUNT(*) as Count
          FROM SearchLogs
          WHERE SearchedAt >= @Since
          GROUP BY SearchTerm
          ORDER BY Count DESC
          LIMIT 10",
            connection);
        searchCmd.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-days));

        var searchTerms = new List<object>();
        using var sReader = searchCmd.ExecuteReader();
        while (sReader.Read())
        {
            searchTerms.Add(new
            {
                Term = sReader["SearchTerm"].ToString(),
                Count = Convert.ToInt32(sReader["Count"])
            });
        }

        return Ok(new
        {
            ViewsOverTime = viewsOverTime,
            TotalViews = totalViews,
            TotalLikes = totalLikes,
            MostViewed = mostViewed,
            MostLiked = mostLiked,
            AllShowoffs = allShowoffs,
            SearchTerms = searchTerms
        });
    }
}