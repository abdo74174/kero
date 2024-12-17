using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModel
{
    public class UserRolesVM
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<RoleVM> Roles { get; set; }

    }
}
