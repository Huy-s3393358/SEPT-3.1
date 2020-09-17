using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineBookingSystem.ViewModel
{
    public class RoomUsageDetailsViewModel
    {
        public int Id { get; set; }
        public string Room { get; set; }
        public string User { get; set; }
        public System.DateTime BookingDate { get; set; }
        public string BookingHours { get; set; }
        public string NumberOfMembers { get; set; }
        public decimal TotalAmount { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public int NumberOfDays { get; set; }

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