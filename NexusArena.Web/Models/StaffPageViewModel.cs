using NexusArena.Web.Models;
using System.Collections.Generic;

namespace NexusArena.MVC.Models
{
    public class StaffPageViewModel
    {
        public ManageReceptionistViewModel NewStaff { get; set; } = new ManageReceptionistViewModel();
        public List<ManageReceptionistViewModel> StaffList { get; set; } = new List<ManageReceptionistViewModel>();
    }
}