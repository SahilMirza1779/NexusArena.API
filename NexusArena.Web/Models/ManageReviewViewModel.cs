using System;

namespace NexusArena.Web.Models
{
    public class ManageReviewViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string ArenaName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}