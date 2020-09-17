using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
namespace OnlineBookingSystem.ViewModel
{
    public class RoomViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Room No.")]
        [Required(ErrorMessage = "Room number is required.")]        public string RoomNumber { get; set; }

        [Display(Name = "Room Price (USD)")]
        [Range(0, 99999999, ErrorMessage = "Room Price should be equal and greater than {1}")]
        [Required(ErrorMessage = "Room Price is required.")]
        public decimal RoomPrice { get; set; }

        [Display(Name = "Room Type")]
        [Required(ErrorMessage = "Room Type is required.")]
        public int RoomTypeId { get; set; }

        [Display(Name = "Room Capacity")]
        [Required(ErrorMessage = "Room Capacity is required.")]
        [Range(1, 99999999, ErrorMessage = "Room Capacity should be equal and greater than {1}")]
        public int RoomCapacity { get; set; }

        [Display(Name = "Room Description")]
        public string RoomDescription { get; set; }
        [Display(Name = "Students Not Allowed")]
        public bool StudentsNotAllowed { get; set; }
        public List<SelectListItem> ListOfRoomType { get; set; }
        [Display(Name = "Type")]
        public int RoomTypeSearchId { get; set; }
        public List<SelectListItem> ListOfRoomTypeSearch { get; set; }

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
    }}
