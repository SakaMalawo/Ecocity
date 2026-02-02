using System.Collections.Generic;
using EcoCity.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcoCity.Areas.Admin.Models.ViewModels
{
    public class UserListViewModel
    {
        public PaginatedList<ApplicationUser> Users { get; set; }
        public List<string> Roles { get; set; }
        public SelectList RoleList => new SelectList(Roles);
        public string SearchTerm { get; set; } = string.Empty;
        public string SelectedRole { get; set; } = string.Empty;
        public string SortOrder { get; set; } = string.Empty;
    }
}
