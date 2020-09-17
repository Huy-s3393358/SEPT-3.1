using OnlineBookingSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineBookingSystem.ViewModel
{
    public class BookingViewModel
    {
        [Display(Name = "Room")]
        public int RoomId { get; set; }
        public string RoomType { get; set; }
        public string RoomNumber { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        [Display(Name = "Date")]
        public DateTime BookingDate { get; set; }
        [Display(Name = "Booking Hours")]
        public int BookingHourID { get; set; }
        public int RoomCapacity { get; set; }
        public decimal RoomPrice { get; set; }
        public string RoomDescription { get; set; }
        [Display(Name = "Number Of Members")]
        [Required(ErrorMessage = "Number of members is required.")]
        [Range(1, 99999999, ErrorMessage = "Number of members should be equal and greater than {1}")]
        public string NumberOfMembers { get; set; }
        public List<SelectListItem> ListBookingHours { get; set; }
        public List<SelectListItem> RoomBookeds { get; set; }
        public IEnumerable<SelectListItem> ListOfRooms { get; set; }
        public int RoomTypeSearchId { get; set; }
        public List<SelectListItem> ListOfRoomTypeSearch { get; set; }
        public int RoomSearchId { get; set; }
        public List<SelectListItem> ListOfRoomSearch { get; set; }

        //pagination and Per Pages
        public int Page { get; set; }
        public int PageSize { get; set; }

        public List<SelectListItem> PerPages()
        {
            return new List<SelectListItem>
            {
                new SelectListItem(){ Text = "10", Value = "1" },
                new SelectListItem(){ Text = "20", Value = "2" },
                new SelectListItem(){ Text = "50", Value = "5" },
                new SelectListItem(){ Text = "100", Value = "10" },
                new SelectListItem(){ Text = "200", Value = "20" }
            };

        }
        
    }
}