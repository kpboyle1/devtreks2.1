using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace DevTreks.ViewModels.Manage
{
    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        //210 changes
        public IList<AuthenticationScheme> OtherLogins { get; set; }
        //public IList<AuthenticationDescription> OtherLogins { get; set; }
    }
}
