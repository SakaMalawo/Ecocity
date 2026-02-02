using System.Collections.Generic;
using EcoCity.Models;

namespace EcoCity.Areas.Admin.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalInitiatives { get; set; }
        public int TotalCategories { get; set; }
        public int TotalComments { get; set; }
        public IEnumerable<ApplicationUser> RecentUsers { get; set; }
        public IEnumerable<Initiative> RecentInitiatives { get; set; }
    }
}
