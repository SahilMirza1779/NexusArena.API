namespace NexusArena.Web.Models
{
    public class DashboardStatsViewModel
    {
        public int TotalPlayers { get; set; }
        public int RegisteredOwners { get; set; }
        public int ActiveArenas { get; set; }
        public string? PlatformRevenue { get; set; }
    }
}