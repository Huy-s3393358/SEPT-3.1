using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineBookingSystem.ViewModel
{
    public class LoginViewModel
    {
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string PassWord { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
        public string LoginErrorMsg { get; set; }
    }

}