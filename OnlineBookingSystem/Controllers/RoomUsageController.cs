using OnlineBookingSystem.Models;
using OnlineBookingSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Net;
using System.Web.Mvc;
using PagedList;

namespace OnlineBookingSystem.Controllers
{
    public class RoomUsageController : Controller
    {
        // GET: RoomUsage
        private BookingDBEntities objBookingDBEntities;
        public RoomUsageController()
        {
            objBookingDBEntities = new BookingDBEntities();
        }
        public ActionResult Index(int page = 1, int pageSize = 10)
        {
            RoomUsageDetailsViewModel objRoomUsageDetailsViewModel = new RoomUsageDetailsViewModel();
            objRoomUsageDetailsViewModel.Page = page;
            objRoomUsageDetailsViewModel.PageSize = pageSize;
            return View(objRoomUsageDetailsViewModel);
        }

        //Delete Room Booking
        public JsonResult DeleteRoomUsageDetails(int roomUsageId)
        {
            #region Get booked room
            // Get booked room based on Id when user click on booked room to delete.
            // System will update IsActive = false, when load booked rooms the system will
            // load all booked rooms with IsActive = true
            RoomUsage objRoomUsage = objBookingDBEntities.RoomUsages.Single(model => model.Id == roomUsageId);
            objRoomUsage.IsActive = false;
            objBookingDBEntities.SaveChanges();
            #endregion

            #region Get user login
            //Get user based on user's login on the system.
            string userName = (string)Session["UserName"];
            User u = objBookingDBEntities.Users.Single(model => model.UserName == userName && model.IsActive == true);
            #endregion

            #region Send Email
            // if user has role is "Admin", the system will send an email to the user with 
            // room information that has been deleted.
            if (u.Role != null && u.Role.Name == "Admin")
            {
                //Send email
                string content = System.IO.File.ReadAllText(Server.MapPath("~/Views/Shared/_TemplateSendEmail.cshtml"));
                content = content.Replace("{{RoomNumber}}", objRoomUsage.Room.RoomNumber);
                content = content.Replace("{{BookingDate}}", objRoomUsage.BookingDate.ToString("dd-MMM-yyyy"));
                content = content.Replace("{{BookingHours}}", objRoomUsage.BookingHour.Name);
                SendMail(objRoomUsage.User.Email, "Your room booking has been cancelled.", content);
                //End Send email
            }
            #endregion

            return Json(new { message = "Record Successfully Deleted.", success = true }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        //Get all booking history for user login
        public PartialViewResult GetAllBookingHistory(RoomUsageDetailsViewModel objRoomUsageViewModel)
        {


            int Page = (objRoomUsageViewModel.Page == 0) ? 1 : objRoomUsageViewModel.Page;
            int PageSize = (objRoomUsageViewModel.PageSize == 0) ? 10 : objRoomUsageViewModel.PageSize;
            ViewBag.PageSize = objRoomUsageViewModel.PageSize;

            #region Get user login
            //Get user based on user's login on the system.
            string userName = (string)Session["UserName"];
            User u = objBookingDBEntities.Users.Single(model => model.UserName == userName && model.IsActive == true);
            #endregion

            #region Permission user
            // Permission for user, if user has the role is Admin, 
            // system will load all booked rooms of all user
            // Otherwise if the user has another role, 
            // system will load all booked rooms of that user.
            IEnumerable<RoomUsageDetailsViewModel> listofRoomUsageDetailsViewModel = null;
            if (u.Role != null && u.Role.Name == "Admin")
            {
                listofRoomUsageDetailsViewModel =
                    (from objRoomUsage in objBookingDBEntities.RoomUsages
                     join objBookingHours in objBookingDBEntities.BookingHours on objRoomUsage.BookingHoursID equals objBookingHours.Id
                     join objRoom in objBookingDBEntities.Rooms on objRoomUsage.RoomId equals objRoom.Id
                     join objUser in objBookingDBEntities.Users on objRoomUsage.UserID equals objUser.Id
                     where objRoomUsage.IsActive == true
                     orderby objRoomUsage.Id descending
                     select new RoomUsageDetailsViewModel()
                     {
                         Id = objRoomUsage.Id,
                         Room = objRoom.RoomNumber,
                         User = objUser.FullName,
                         BookingDate = objRoomUsage.BookingDate,
                         BookingHours = objBookingHours.Name,
                         NumberOfMembers = objRoomUsage.NumberOfMembers,
                         TotalAmount = objRoom.RoomPrice,
                 }).OrderByDescending(x => x.Id).ToPagedList(Page, PageSize);
            }
            else
            {
                listofRoomUsageDetailsViewModel =
                   (from objRoomUsage in objBookingDBEntities.RoomUsages
                    join objBookingHours in objBookingDBEntities.BookingHours on objRoomUsage.BookingHoursID equals objBookingHours.Id
                    join objRoom in objBookingDBEntities.Rooms on objRoomUsage.RoomId equals objRoom.Id
                    join objUser in objBookingDBEntities.Users on objRoomUsage.UserID equals objUser.Id
                    where objRoomUsage.IsActive == true && objUser.Id == u.Id
                    orderby objRoomUsage.Id descending
                    select new RoomUsageDetailsViewModel()
                    {
                        Id = objRoomUsage.Id,
                        Room = objRoom.RoomNumber,
                        User = objUser.FullName,
                        BookingDate = objRoomUsage.BookingDate,
                        BookingHours = objBookingHours.Name,
                        NumberOfMembers = objRoomUsage.NumberOfMembers,
                        TotalAmount = objRoom.RoomPrice,
 						}).OrderByDescending(x => x.Id).ToPagedList(Page, PageSize);
            }
            #endregion

            return PartialView("_BookingHistoryPartial", listofRoomUsageDetailsViewModel);
        }

        public void SendMail(string toEmailAddress, string subject, string content)
        {
            #region Declare the fields
            //Declare all the fields configured in web.config
            //content: the contents of the email.
            var fromEmailAddress = ConfigurationManager.AppSettings["FromEmailAddress"].ToString();
            var fromEmailDisplayName = ConfigurationManager.AppSettings["FromEmailDisplayName"].ToString();
            var fromEmailPassWord = ConfigurationManager.AppSettings["FromEmailPassWord"].ToString();
            var smtpHost = ConfigurationManager.AppSettings["SMTPHost"].ToString();
            var smtpPort = ConfigurationManager.AppSettings["SMTPPort"].ToString();
            bool enableSSL = bool.Parse(ConfigurationManager.AppSettings["EnableSSL"].ToString());
            string body = content;
            #endregion
            
            #region Configure the protocol
            //Configure the protocol and send email with all the information added
            MailMessage message = new MailMessage(new MailAddress(fromEmailAddress, fromEmailDisplayName), new MailAddress(toEmailAddress));
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = body;
            var client = new SmtpClient();
            client.Credentials = new NetworkCredential(fromEmailAddress, fromEmailPassWord);
            client.Host = smtpHost;
            client.EnableSsl = enableSSL;
            client.Port = !string.IsNullOrEmpty(smtpPort) ? Convert.ToInt32(smtpPort) : 0;
            client.Send(message);
            #endregion

        }
    }
}  
