using OnlineBookingSystem.Models;
using OnlineBookingSystem.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineBookingSystem.Controllers
{
    public class BookingController : Controller
    {
        // GET: Booking
        private BookingDBEntities objBookingDBEntities;
        public BookingController()
        {
            objBookingDBEntities = new BookingDBEntities();
        }
        public ActionResult Index(DateTime? BookingDate, int RoomSearch=0, int RoomTypeSearch =0, int page =1, int pageSize = 1)
        {
            BookingViewModel objBookingViewModel = new BookingViewModel();
            objBookingViewModel.BookingDate = DateTime.Now;

            objBookingViewModel.ListOfRooms = (from obj in objBookingDBEntities.Rooms
                                               where obj.IsActive == true
                                               select new SelectListItem()
                                               {
                                                   Text = obj.RoomNumber,
                                                   Value = obj.Id.ToString()
                                               }
                                                ).ToList();
            objBookingViewModel.ListBookingHours = (from obj in objBookingDBEntities.BookingHours
                                                    orderby obj.Id ascending
                                                    select new SelectListItem()
                                                    {
                                                        Text = obj.Name,
                                                        Value = obj.Id.ToString()
                                                    }).ToList();
            //Load all Room Type Search
            objBookingViewModel.ListOfRoomTypeSearch = (from obj in objBookingDBEntities.RoomTypes
                                                        select new SelectListItem()
                                                        {
                                                            Text = obj.Name,
                                                            Value = obj.Id.ToString()
                                                        }).ToList();

            #region Get user login
            //Get user based on user's login on the system.
            User user = new User();
            string userName = (string)Session["UserName"];
            if (!string.IsNullOrEmpty(userName))
                user = objBookingDBEntities.Users.Single(model => model.UserName == userName && model.IsActive == true);
            #endregion

            #region Permission user
            // Permission for user, if user has the role is Student, system will load all Rooms with Room Type is Sport Room 
            // or Room Type is Meeting Room and StudentsNotAllowed = false and it will be based on the selected type.
            // Otherwise if the user has another role, the system will load all active rooms
            if (user != null && user.Role.Name == "Student")
            {
                objBookingViewModel.ListOfRoomSearch = (from obj in objBookingDBEntities.Rooms
                                                        where obj.IsActive == true
                                                              && (obj.RoomType.Name == "Sport Room" | (obj.RoomType.Name == "Meeting Room" && obj.StudentsNotAllowed == false))
                                                        select new SelectListItem()
                                                        {
                                                            Text = obj.RoomNumber,
                                                            Value = obj.Id.ToString()
                                                        }).ToList();
            }
            else
            {
                objBookingViewModel.ListOfRoomSearch = (from obj in objBookingDBEntities.Rooms
                                                        where obj.IsActive == true
                                                        select new SelectListItem()
                                                        {
                                                            Text = obj.RoomNumber,
                                                            Value = obj.Id.ToString()
                                                        }).ToList();
            }
            #endregion

            #region Pagination
            //pagination and Per Pages
            objBookingViewModel.Page = page;
            objBookingViewModel.PageSize = pageSize;
            objBookingViewModel.RoomSearchId = RoomSearch;
            objBookingViewModel.RoomTypeSearchId = RoomTypeSearch;
            if(BookingDate != null)
                objBookingViewModel.BookingDate = BookingDate.Value;
            #endregion

            return View(objBookingViewModel);
        }

        [HttpGet]
        public JsonResult GetRoomWithRoomtype(int rType)
        {
            List<SelectListItem> result = null;
            #region Get user login
            //Get user based on user's login on the system.
            User user = new User();
            string userName = (string)Session["UserName"];
            if (!string.IsNullOrEmpty(userName))
                user = objBookingDBEntities.Users.Single(model => model.UserName == userName && model.IsActive == true);
            #endregion

            #region Permission user
            // Permission for user, if user has the role is Student, system will load all Rooms with Room Type is Sport Room 
            // or Room Type is Meeting Room and StudentsNotAllowed = false and it will be based on the selected type.
            // Otherwise if the user has another role, the system will load all active rooms and it will be based on the selected type.
            if (user != null && user.Role.Name == "Student")
            {
                result = (from obj in objBookingDBEntities.Rooms
                          where obj.IsActive == true
                                && (rType == 0 ? true : obj.RoomTypeid == rType)
                                && (obj.RoomType.Name == "Sport Room" | (obj.RoomType.Name == "Meeting Room" && obj.StudentsNotAllowed == false))
                          select new SelectListItem()
                          {
                              Text = obj.RoomNumber,
                              Value = obj.Id.ToString()
                          }).ToList();
            }
            else
            {
                result = (from obj in objBookingDBEntities.Rooms
                          where obj.IsActive == true && (rType == 0 ? true : obj.RoomTypeid == rType)
                          select new SelectListItem()
                          {
                              Text = obj.RoomNumber,
                              Value = obj.Id.ToString()
                          }).ToList();
            }
            #endregion

            return Json(result, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult Index(BookingViewModel objBookingViewModel)
        {
            try
            {
                // Uncomment below throw exception to test Singleton Logger for booking
                // throw new System.NullReferenceException("Booking is not valid.");
                //Users can only choose a maximum of 2 time frames in a day
                int userID = 0;
                if (Session["LogedUserID"] != null)
                {
                    userID = Convert.ToInt32(Session["LogedUserID"]);
                }
                int UserHour = objBookingDBEntities.RoomUsages.Count(model => model.UserID == userID
                                                                    && model.BookingDate.Year == objBookingViewModel.BookingDate.Year
                                                                    && model.BookingDate.Month == objBookingViewModel.BookingDate.Month
                                                                    && model.BookingDate.Day == objBookingViewModel.BookingDate.Day
                                                                    && model.IsActive == true);
                if (UserHour >= 2)
                {
                    return Json(new { message = "Users can only choose maximum 2 time frames in a day.", success = true }, JsonRequestBehavior.AllowGet); ;
                }

                //Users cannot book a room already booked
                int UserBooked = objBookingDBEntities.RoomUsages.Count(model => model.BookingDate.Year == objBookingViewModel.BookingDate.Year
                                                                    && model.BookingDate.Month == objBookingViewModel.BookingDate.Month
                                                                    && model.BookingDate.Day == objBookingViewModel.BookingDate.Day
                                                                    && model.BookingHoursID == objBookingViewModel.BookingHourID
                                                                	&& model.RoomId == objBookingViewModel.RoomId
                                                                    && model.IsActive == true);
                if (UserBooked >= 1)
                {
                    return Json(new { message = "The room has already been booked.", success = true }, JsonRequestBehavior.AllowGet); ;
                }

                RoomUsage obj = new RoomUsage()
                {
                    RoomId = objBookingViewModel.RoomId,
                    BookingHoursID = objBookingViewModel.BookingHourID,
                    BookingDate = objBookingViewModel.BookingDate,
                    UserID = userID,
                    NumberOfMembers = objBookingViewModel.NumberOfMembers,
                    IsActive = true
                };
                objBookingDBEntities.RoomUsages.Add(obj);
                objBookingDBEntities.SaveChanges();
                return Json(new { message = "Booking Successfully Created.", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                MyLogger.Instance.TraceEvent(TraceEventType.Error, 0, System.DateTime.Now.ToString() + ": Booking fail with error: " + ex.Message);
                return Json(new { message = "Booking not successful. Please try again.", success = true }, JsonRequestBehavior.AllowGet);
            }

        }

        //Get all Rooms to booking
        [HttpPost]
        public PartialViewResult GetAllRooms(BookingViewModel objBVM)
        {
            #region Pagination
            //pagination and Per Pages
            int Page = (objBVM.Page == 0) ? 1 : objBVM.Page;
            int PageSize = (objBVM.PageSize == 0) ? 1 : objBVM.PageSize;
            ViewBag.PageSize = objBVM.PageSize;
            ViewBag.RoomTypeSearch = objBVM.RoomTypeSearchId;
            ViewBag.RoomSearch = objBVM.RoomSearchId;
            ViewBag.BookingDate = objBVM.BookingDate;
            #endregion


            string bookingDate = objBVM.BookingDate.ToString("dd-MMM-yyyy");
            IEnumerable<BookingViewModel> listofRoomDetailsViewModel = null;

            #region Get user login
            //Get user based on user's login on the system.
            User user = new User();
            string userName = (string)Session["UserName"];
            if (!string.IsNullOrEmpty(userName))
            {
                user = objBookingDBEntities.Users.Single(model => model.UserName == userName && model.IsActive == true);

            }
            # endregion

            #region Permission user
            // Permission for user, if user has the role is Student, system will load all Rooms with Room Type is Sport Room 
            // or Room Type is Meeting Room and StudentsNotAllowed = false.
            // Otherwise if the user has another role, the system will load all active rooms
            // The system will display all room number users are allowed to book with 
            // current date when user go to "Booking" module.

            if (user != null && user.Role.Name == "Student")
            {
                listofRoomDetailsViewModel = (from objRoom in objBookingDBEntities.Rooms
                                              join objRoomType in objBookingDBEntities.RoomTypes on objRoom.RoomTypeid equals objRoomType.Id
                                              where objRoom.IsActive == true && (objRoomType.Name == "Sport Room" | (objRoomType.Name == "Meeting Room" && objRoom.StudentsNotAllowed == false))
                                                     && (objBVM.RoomTypeSearchId == 0 ? true : objRoom.RoomTypeid == objBVM.RoomTypeSearchId)
                                                     && (objBVM.RoomSearchId == 0 ? true : objRoom.Id == objBVM.RoomSearchId)
                                              orderby objRoom.RoomNumber descending
                                              select new BookingViewModel()
                                              {
                                                  RoomNumber = objRoom.RoomNumber,
                                                  RoomCapacity = objRoom.RoomCapacity,
                                                  RoomPrice = objRoom.RoomPrice,
                                                  RoomId = objRoom.Id,
                                                  RoomDescription = objRoom.RoomDescription,
                                                  RoomType = objRoomType.Name,
                                                  BookingDate = objBVM.BookingDate,
                                                  ListBookingHours = (from obj in objBookingDBEntities.BookingHours
                                                                      orderby obj.Id ascending
                                                                      select new SelectListItem()
                                                                      {
                                                                          Text = obj.Name,
                                                                          Value = obj.Id.ToString()
                                                                      }).ToList(),
                                                  RoomBookeds = (from obj in objBookingDBEntities.RoomUsages
                                                                 where obj.RoomId == objRoom.Id &&
                                                                       obj.BookingDate.Year == objBVM.BookingDate.Year
                                                                       && obj.BookingDate.Month == objBVM.BookingDate.Month
                                                                       && obj.BookingDate.Day == objBVM.BookingDate.Day
                                                                       && obj.IsActive == true
                                                                 select new SelectListItem()
                                                                 {
                                                                     Text = bookingDate,
                                                                     Value = obj.BookingHoursID.ToString()
                                                                 }).ToList(),
                                              }).OrderByDescending(x => x.RoomId).ToPagedList(Page, PageSize);
            }
            else
            {
                listofRoomDetailsViewModel = (from objRoom in objBookingDBEntities.Rooms
                                              join objRoomType in objBookingDBEntities.RoomTypes on objRoom.RoomTypeid equals objRoomType.Id
                                              where objRoom.IsActive == true
                                                     && (objBVM.RoomTypeSearchId == 0 ? true : objRoom.RoomTypeid == objBVM.RoomTypeSearchId)
                                                     && (objBVM.RoomSearchId == 0 ? true : objRoom.Id == objBVM.RoomSearchId)
                                              orderby objRoom.RoomNumber descending
                                              select new BookingViewModel()
                                              {
                                                  RoomNumber = objRoom.RoomNumber,
                                                  RoomCapacity = objRoom.RoomCapacity,
                                                  RoomPrice = objRoom.RoomPrice,
                                                  RoomId = objRoom.Id,
                                                  RoomDescription = objRoom.RoomDescription,
                                                  RoomType = objRoomType.Name,
                                                  BookingDate = objBVM.BookingDate,
                                                  ListBookingHours = (from obj in objBookingDBEntities.BookingHours
                                                                      orderby obj.Id ascending
                                                                      select new SelectListItem()
                                                                      {
                                                                          Text = obj.Name,
                                                                          Value = obj.Id.ToString()
                                                                      }).ToList(),
                                                  RoomBookeds = (from obj in objBookingDBEntities.RoomUsages
                                                                 where obj.RoomId == objRoom.Id &&
                                                                       obj.BookingDate.Year == objBVM.BookingDate.Year
                                                                       && obj.BookingDate.Month == objBVM.BookingDate.Month
                                                                       && obj.BookingDate.Day == objBVM.BookingDate.Day
                                                                       && obj.IsActive == true
                                                                 select new SelectListItem()
                                                                 {
                                                                     Text = bookingDate,
                                                                     Value = obj.BookingHoursID.ToString()
                                                                 }).ToList(),
                                              }).OrderByDescending(x => x.RoomId).ToPagedList(Page, PageSize); 

            }
            # endregion

            return PartialView("_BookingDetailsPartial", listofRoomDetailsViewModel);
        }

    }
}