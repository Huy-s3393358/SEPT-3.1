using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineBookingSystem.ViewModel
{
    public class UserDetailsViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string Email { get; set; }
    }
}