namespace ArcaneVaultRP.Models
{
    public class PopularItem
    {
        public string ItemName { get; set; }
        public int OwnerCount { get; set; }
        public string? CategoryName { get; set; }
    }

    public class CategoryPopularity
    {
        public string CategoryName { get; set; }
        public int ItemCount { get; set; }
        public int OwnerCount { get; set; }
    }

    public class UserActivity
    {
        public string UserName { get; set; }
        public int TotalLogins { get; set; }
        public int TotalShowoffs { get; set; }
        public string? LastLogin { get; set; }
    }
    public class ShowoffView
    {
        public string ShowoffName { get; set; }
        public string UserName { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
    }
    
    public class ShowoffEngagement
    {
        public string ShowoffName { get; set; }
        public string UserName { get; set; }
        public int LikeCount { get; set; }
        public int ViewCount { get; set; }
    }

    public class SearchTrend
    {
        public string SearchTerm { get; set; }
        public string SearchType { get; set; }
        public int Count { get; set; }
    }

    public class TimeSeriesPoint
    {
        public string Date { get; set; }
        public int Count { get; set; }
    }

    public class ActivityPoint
    {
        public string Date { get; set; }
        public int ActiveUsers { get; set; }
    }

    public class MarketDemandItem
    {
        public string ItemName { get; set; }
        public int CurrentOwners { get; set; }
        public string? CategoryName { get; set; }
    }

    public class CommunityInterestData
    {
        public List<ShowoffView> MostViewed { get; set; } = new();
        public List<ShowoffEngagement> MostLiked { get; set; } = new();
        public List<CategoryPopularity> CategoryPopularity { get; set; } = new();
    }

    public class MarketDemandDetailItem
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public int Start { get; set; }
        public int Current { get; set; }
        public int OwnerCount { get; set; }
        public int Pct { get; set; }
    }

    public class CountResult
    {
        public int Count { get; set; }
    }

    public class PopularItemDetail
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public int Owners { get; set; }
        public int Qty { get; set; }
        public double Ratio { get; set; }
    }

    public class UserActivityDetail
    {
        public List<TimeSeriesPoint> LoginActivity { get; set; } = new();
        public int[] DayOfWeek { get; set; } = new int[7];
        public int[] MonthData { get; set; } = new int[12];
        public List<UserActivityUser> Users { get; set; } = new();
    }

    public class UserActivityUser
    {
        public string Name { get; set; }
        public int Logins { get; set; }
        public int Showoffs { get; set; }
        public string? LastLogin { get; set; }
    }

    public class CountryDemographic
    {
        public string Country { get; set; }
        public int UserCount { get; set; }
        public double Percentage { get; set; }
    }
}