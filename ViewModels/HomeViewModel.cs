using EcoCity.Models;
using System.Collections.Generic;

namespace EcoCity.ViewModels
{
    public class HomeViewModel
    {
        public List<Initiative> RecentInitiatives { get; set; } = new List<Initiative>();
        public List<Initiative> PopularInitiatives { get; set; } = new List<Initiative>();
        public int TotalInitiatives { get; set; }
        public int TotalUsers { get; set; }
        public int TotalVotes { get; set; }
        public int TotalComments { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}
