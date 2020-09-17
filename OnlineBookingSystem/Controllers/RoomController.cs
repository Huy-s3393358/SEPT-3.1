using OnlineBookingSystem.Models;
using OnlineBookingSystem.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace OnlineBookingSystem.Controllers
{
    public class RoomController : Controller
    {
        // GET: Room
        private BookingDBEntities objBookingDBEntities;
        public RoomController()
        {
            objBookingDBEntities = new BookingDBEntities();
        }
        public ActionResult Index(int RoomTypeSearch = 0, int pageSize = 10, int page = 1)
        {
            RoomViewModel objRoomViewModel = new RoomViewModel();
            //Load all Room Type
            objRoomViewModel.ListOfRoomType = (from obj in objBookingDBEntities.RoomTypes
                                                 select new SelectListItem()
                                                 {
                                                     Text = obj.Name,
                                                     Value = obj.Id.ToString()
                                                 }).ToList();
            //Load all Room Type Search
            objRoomViewModel.ListOfRoomTypeSearch = (from obj in objBookingDBEntities.RoomTypes
                                               select new SelectListItem()
                                               {
                                                   Text = obj.Name,
                                                   Value = obj.Id.ToString()
                                               }).ToList();
            #region Pagination
            //pagination and Per Pages
            objRoomViewModel.RoomTypeSearchId = RoomTypeSearch;
            objRoomViewModel.Page = page;
            objRoomViewModel.PageSize = pageSize;
            #endregion

            return View(objRoomViewModel);
        }
        [HttpPost]
        public ActionResult Index(RoomViewModel objRoomViewModel)
        {
            string message = string.Empty;
            if (objRoomViewModel.Id == 0)
            {
                //Insert new a Room to database
                Room obj = new Room()
                {
                    RoomNumber = objRoomViewModel.RoomNumber,
                    RoomPrice = objRoomViewModel.RoomPrice,
                    RoomTypeid = objRoomViewModel.RoomTypeId,
                    RoomCapacity = objRoomViewModel.RoomCapacity,
                    RoomDescription = objRoomViewModel.RoomDescription,
                    StudentsNotAllowed = objRoomViewModel.StudentsNotAllowed,
                    IsActive = true
                };
                objBookingDBEntities.Rooms.Add(obj);
                message = "Added.";
            }
            else
            {
                //Edit a Room
                Room obj = objBookingDBEntities.Rooms.Single(model => model.Id == objRoomViewModel.Id);
                obj.RoomNumber = objRoomViewModel.RoomNumber;
                obj.RoomPrice = objRoomViewModel.RoomPrice;
                obj.RoomTypeid = objRoomViewModel.RoomTypeId;
                obj.RoomCapacity = objRoomViewModel.RoomCapacity;
                obj.RoomDescription = objRoomViewModel.RoomDescription;
                obj.StudentsNotAllowed = objRoomViewModel.StudentsNotAllowed;
                obj.IsActive = true;
                message = "Updated.";
            }
           
            objBookingDBEntities.SaveChanges();
            return Json(new { message = "Room Successfully "+ message , success = true}, JsonRequestBehavior.AllowGet);
        }

        //Load All Room in UI
        [HttpPost]
        public PartialViewResult GetAllRooms(RoomViewModel objRoomView)
        {
            #region Pagination
            //pagination and Per Pages
            int Page = (objRoomView.Page == 0) ? 1 : objRoomView.Page;
            int PageSize = (objRoomView.PageSize == 0) ? 1 : objRoomView.PageSize;
            ViewBag.RoomTypeSearch = objRoomView.RoomTypeSearchId;
            #endregion

            IEnumerable<RoomDetailsViewModel> listofRoomDetailsViewModel =
                (from objRoom in objBookingDBEntities.Rooms
                 join objRoomType in objBookingDBEntities.RoomTypes on objRoom.RoomTypeid equals objRoomType.Id
                 where objRoom.IsActive == true && ((objRoomView.RoomTypeSearchId == 0) ? true : objRoom.RoomTypeid == objRoomView.RoomTypeSearchId)
                 select new RoomDetailsViewModel()
                 {
                     RoomNumber = objRoom.RoomNumber,
                     RoomCapacity = objRoom.RoomCapacity,
                     RoomPrice = objRoom.RoomPrice,
                     RoomDescription = objRoom.RoomDescription,
                     RoomType = objRoomType.Name,
                     StudentsNotAllowed = objRoom.StudentsNotAllowed,
                     Id = objRoom.Id
                 }).OrderByDescending(x => x.Id).ToPagedList(Page, PageSize); ;
            return PartialView("_RoomDetailsPartial", listofRoomDetailsViewModel);
        }
        [HttpGet]
        public JsonResult EditRoomDetails(int roomid)
        {
            objBookingDBEntities.Configuration.ProxyCreationEnabled = false;
            var result = objBookingDBEntities.Rooms.Single(model => model.Id == roomid);
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public JsonResult DeleteRoomDetails(int roomid)
        {
            Room objRoom = objBookingDBEntities.Rooms.Single(model => model.Id == roomid);
            objRoom.IsActive = false;
            objBookingDBEntities.SaveChanges();
            return Json(new { message = "Record Successfully Deleted.", success = true }, JsonRequestBehavior.AllowGet);

        }

    }
}