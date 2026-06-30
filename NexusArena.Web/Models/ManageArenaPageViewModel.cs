using System.Collections.Generic;

namespace NexusArena.Web.Models
{
    public class ArenaViewModel
    {
        public int ArenaId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class ManageArenaPageViewModel
    {
        public ArenaViewModel NewArena { get; set; } = new ArenaViewModel();
        public List<ArenaViewModel> ArenasList { get; set; } = new List<ArenaViewModel>();
    }
}