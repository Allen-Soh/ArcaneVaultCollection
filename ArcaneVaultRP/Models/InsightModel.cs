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

    public class ShowoffComparison
    {
        public int ShowoffId { get; set; }
        public string ShowoffName { get; set; }
        public string UserName { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public double LikeRate { get; set; }
    }

    public class CommunityInterestDetail
    {
        public List<TimeSeriesPoint> ViewsOverTime { get; set; } = new();
        public int TotalViews { get; set; }
        public int TotalLikes { get; set; }
        public List<ShowoffComparison> MostViewed { get; set; } = new();
        public List<ShowoffComparison> MostLiked { get; set; } = new();
        public List<ShowoffComparison> AllShowoffs { get; set; } = new();
        public List<SearchTrend> SearchTerms { get; set; } = new();
    }

    public class TopSale
    {
        public int PaymentId { get; set; }
        public string UserName { get; set; }
        public string CosmeticName { get; set; }
        public string CosmeticType { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string CreatedAt { get; set; }
    }

    public class CosmeticOwnership
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int OwnerCount { get; set; }
    }

    public class FinanceAndSalesDetail
    {
        public decimal TotalSalesValue { get; set; }
        public int TotalSalesCount { get; set; }
        public List<TopSale> TopSales { get; set; } = new();
        public List<CosmeticOwnership> FrameOwnership { get; set; } = new();
        public List<CosmeticOwnership> StickerOwnership { get; set; } = new();
    }
}