using EcoCity.Models;
using System.Collections.Generic;

namespace EcoCity.ViewModels
{
    public class InitiativeIndexViewModel
    {
        public List<Initiative> Initiatives { get; set; } = new List<Initiative>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<string> StatusList { get; set; } = new List<string>();
        public string SearchString { get; set; }
        public int? CategoryId { get; set; }
        public string Status { get; set; }
        public string SortOrder { get; set; }
    }
}
