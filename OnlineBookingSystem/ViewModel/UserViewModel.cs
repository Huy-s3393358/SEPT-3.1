using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace OnlineBookingSystem.ViewModel
{
    public class UserViewModel
    {
        public int Id { get; set; }

        [Display(Name = "User Name")]
        [Required(ErrorMessage = "User Name is required.")]
        [RegularExpression(@"([^\s]+)", ErrorMessage = "User Name cannot contain space characters.")]
        [MinLength(2)]
        [MaxLength(16)]
        public string UserName { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(3)]
        [MaxLength(10)]
        public string PassWord { get; set; }

        [System.Web.Mvc.Compare("PassWord", ErrorMessage = "Password and confirmation Re-enter Password do not match.")]
        [Display(Name = "Re-enter Password")]
        public string RePassWord { get; set; }

        [Display(Name = "Full Name")]
        //[Required(ErrorMessage = "Full Name is required.")]
        [RegularExpression(@"(^([a-zA-Z ]*?)\s+([a-zA-Z]*)$)", ErrorMessage = "Full Name cannot contain numbers and must have at least 2 words.")]
        public string FullName { get; set; }

        [Display(Name = "Role Name")]
        [Required(ErrorMessage = "Role Name is required.")]
        public int RoleId { get; set; }
        public Nullable<bool> IsActive { get; set; }

        [Display(Name = "Email Address")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Required(ErrorMessage = "Email Address is required.")]
        public string Email { get; set; }
		//[Required]
        [RegularExpression(@"^(\(?\+?[0-9]*\)?)?[0-9_\- \(\)]*$", ErrorMessage = "Phone Number format is not valid.")]
        [MaxLength(10, ErrorMessage = "Phone Number must be 10 digits.")]        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "About")]
        public string About { get; set; }

        [Display(Name = "Image")]
        public HttpPostedFileBase Image { get; set; }

        public string imgUser { get; set; }
        public List<SelectListItem> ListOfRole { get; set; }

        [Display(Name = "Enter Name:")]
        public string NameSearch { get; set; }


        public int Page { get; set; }
        public int PageSize { get; set; }

        public List<SelectListItem> PerPages()
        {
            return new List<SelectListItem>
            {
                new SelectListItem(){ Text = "10", Value = "10" },
                new SelectListItem(){ Text = "20", Value = "20" },
                new SelectListItem(){ Text = "50", Value = "50" },
                new SelectListItem(){ Text = "100", Value = "100" },
                new SelectListItem(){ Text = "200", Value = "200" }
            };

        }
    }
}
