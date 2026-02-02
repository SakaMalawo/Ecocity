using EcoCity.Models;

namespace EcoCity.ViewModels
{
    public class InitiativeDetailsViewModel
    {
        public Initiative Initiative { get; set; }
        public Comment NewComment { get; set; }
        public bool CanEdit { get; set; }
    }
}
