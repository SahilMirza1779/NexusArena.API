using System;

namespace NexusArena.Web.Models
{
    public class ManageRevenueViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public decimal Amount { get; set; }
        public DateOnly Date { get; set; }
        public string Status { get; set; }
        public string Method { get; set; }
    }
}