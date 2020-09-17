using OnlineBookingSystem.Models;
using OnlineBookingSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;

namespace OnlineBookingSystem.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        private BookingDBEntities objBookingDBEntities;
        public AccountController()
        {
            objBookingDBEntities = new BookingDBEntities();
        }

        public ActionResult Index(string search, int pageSize = 10, int page = 1)
        {
            UserViewModel objUserViewModel = new UserViewModel();
            objUserViewModel.Page = page;
            objUserViewModel.PageSize = pageSize;
            objUserViewModel.NameSearch = search;

            //Load all User for dropdownlist
            objUserViewModel.ListOfRole = (from obj in objBookingDBEntities.Roles
                                           select new SelectListItem()
                                           {
                                               Text = obj.Name,
                                               Value = obj.Id.ToString()
                                           }).ToList();
            return View(objUserViewModel);
        }
        [HttpPost]
        public ActionResult Index(UserViewModel objUserViewModel)
        {
            string message = string.Empty;
            string ImageUniqueName = String.Empty;
            string ActualImageName = String.Empty;

            if (objUserViewModel.Id == 0)
            {
                //get value for img
                ImageUniqueName = Guid.NewGuid().ToString();
                if (objUserViewModel != null && objUserViewModel.Image != null)
                {
                    ActualImageName = ImageUniqueName + Path.GetExtension(objUserViewModel.Image.FileName);
                    objUserViewModel.Image.SaveAs(Server.MapPath("~/UserImages/" + ActualImageName));
                }
                //Insert new a User to database
                User obj = new User()
                {
                    UserName = objUserViewModel.UserName,
                    PassWord = Encrypt(objUserViewModel.PassWord),
                    FullName = objUserViewModel.FullName,
                    Email = objUserViewModel.Email,
                    RoleId = objUserViewModel.RoleId,
                    IsActive = true,
                    UserImage = ActualImageName,
                    PhoneNumber = objUserViewModel.PhoneNumber,
                };
                objBookingDBEntities.Users.Add(obj);
                message = "Added.";
            }
            else
            {
                User obj = objBookingDBEntities.Users.Single(model => model.Id == objUserViewModel.Id && model.IsActive == true);
                //get value for img
                if (objUserViewModel.Image != null)
                {
                    ImageUniqueName = Guid.NewGuid().ToString();
                    ActualImageName = ImageUniqueName + Path.GetExtension(objUserViewModel.Image.FileName);
                    objUserViewModel.Image.SaveAs(Server.MapPath("~/UserImages/" + ActualImageName));
                    obj.UserImage = ActualImageName;
                }
                //Edit a User
                obj.UserName = objUserViewModel.UserName;
                if (!string.IsNullOrEmpty(objUserViewModel.PassWord))
                    obj.PassWord = Encrypt(objUserViewModel.PassWord);
                obj.FullName = objUserViewModel.FullName;
                obj.PhoneNumber = objUserViewModel.PhoneNumber;
                obj.Email = objUserViewModel.Email;
                obj.RoleId = objUserViewModel.RoleId;
                obj.IsActive = true;
                message = "Updated.";
            }

            objBookingDBEntities.SaveChanges();
            return Json(new { message = "User Successfully " + message, success = true }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult EditUserDetails(int userid)
        {
            objBookingDBEntities.Configuration.ProxyCreationEnabled = false;
            var result = objBookingDBEntities.Users.Single(model => model.Id == userid && model.IsActive == true);
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public JsonResult DeleteUserDetails(int userid)
        {
            User objUser = objBookingDBEntities.Users.Single(model => model.Id == userid && model.IsActive == true);
            objUser.IsActive = false;
            if (objUser != null)
            {
                List<RoomUsage> roomUsages = objBookingDBEntities.RoomUsages.Where(x => x.UserID == objUser.Id).ToList();
                foreach (RoomUsage roomUsage in roomUsages)
                {
                    roomUsage.IsActive = false;
                }
            }
            objBookingDBEntities.SaveChanges();

            return Json(new { message = "Record Successfully Deleted.", success = true }, JsonRequestBehavior.AllowGet);

        }
        //Load All User
        [HttpPost]
        public PartialViewResult GetAllUsers(UserViewModel objUserView)
        {

            int Page = string.IsNullOrEmpty(objUserView.Page.ToString()) ? 1 : objUserView.Page;
            int PageSize = string.IsNullOrEmpty(objUserView.PageSize.ToString()) ? 10 : objUserView.PageSize;
            ViewBag.SearchPage = objUserView.NameSearch;
            ViewBag.PageSize = objUserView.PageSize;
            IEnumerable<UserDetailsViewModel> listofUserDetailsViewModel =
                (from objUser in objBookingDBEntities.Users
                 where objUser.IsActive == true && (string.IsNullOrEmpty(objUserView.NameSearch.Trim()) ? true : objUser.FullName.StartsWith(objUserView.NameSearch))
                 select new UserDetailsViewModel()
                 {
                     UserName = objUser.UserName,
                     FullName = objUser.FullName,
                     Email = objUser.Email,
                     RoleName = objUser.Role.Name,
                     Id = objUser.Id
                 }).OrderByDescending(x => x.Id).ToPagedList(Page, PageSize);
            return PartialView("_UserDetailsPartial", listofUserDetailsViewModel);
        }
        // Update Profile
        [HttpPost]
        public ActionResult UpdateProfile(UserViewModel objUser)
        {
            string message = string.Empty;
            string ImageUniqueName = String.Empty;
            string ActualImageName = String.Empty;
            User obj = objBookingDBEntities.Users.Single(model => model.Id == objUser.Id && model.IsActive == true);
            //get value for img
            if (objUser.Image != null)
            {
                ImageUniqueName = Guid.NewGuid().ToString();
                ActualImageName = ImageUniqueName + Path.GetExtension(objUser.Image.FileName);
                objUser.Image.SaveAs(Server.MapPath("~/UserImages/" + ActualImageName));
                obj.UserImage = ActualImageName;
            }
            if (!string.IsNullOrEmpty(objUser.PassWord))
                obj.PassWord = Encrypt(objUser.PassWord);
            obj.FullName = objUser.FullName;
            obj.Email = objUser.Email;
            obj.PhoneNumber = objUser.PhoneNumber;
            obj.About = objUser.About;
            objBookingDBEntities.SaveChanges();
            message = "Updated.";
            return Json(new { message = "User Successfully " + message, success = true }, JsonRequestBehavior.AllowGet);
        }



        // Validate account usser, if user create new account with user name same 
        // with user name in db, system will show error.
        [HttpPost]
        public JsonResult UserNameExists(UserViewModel objUserView)
        {
            if (objUserView.UserName != null)
            {
                if (objBookingDBEntities.Users.Any(x => x.UserName == objUserView.UserName && x.Id == objUserView.Id && x.IsActive == true))
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                if (objBookingDBEntities.Users.Any(x => x.UserName == objUserView.UserName && x.Id != objUserView.Id && x.IsActive == true))
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Edit(string id)
        {
            int userId = Convert.ToInt32(id);
            objBookingDBEntities.Configuration.ProxyCreationEnabled = false;
            var result = objBookingDBEntities.Users.Single(model => model.Id == userId && model.IsActive == true);

            UserViewModel objUseEdit = new UserViewModel();

            //Load all User for dropdownlist
            objUseEdit.ListOfRole = (from obj in objBookingDBEntities.Roles
                                     select new SelectListItem()
                                     {
                                         Text = obj.Name,
                                         Value = obj.Id.ToString()
                                     }).ToList();
            objUseEdit.Id = result.Id;
            objUseEdit.imgUser = "../../UserImages/" + result.UserImage;
            objUseEdit.FullName = result.FullName;
            objUseEdit.Email = result.Email;
            objUseEdit.RoleId = result.RoleId;
            objUseEdit.UserName = result.UserName;
            objUseEdit.PhoneNumber = result.PhoneNumber;
            objUseEdit.About = result.About;

            return View(objUseEdit);
        }

        public ActionResult Login()
        {
            return View();
        }
        //Verify that the user exists in the system
        [HttpPost]
        public ActionResult Login(LoginViewModel u)
        {
            try
            {
                // Uncomment below throw exception to test Singleton Logger for login
                // throw new System.NullReferenceException("Login account is not valid.");
                //this action is for handle post (login)
                if (ModelState.IsValid)// this is check validity
                {
                    var User = objBookingDBEntities.Users.Where(x => x.UserName.Equals(u.UserName) && x.IsActive == true).FirstOrDefault();

                    if (User != null)
                    {
                        string DecyptPassw = "";
                        if (u.PassWord != null)
                            DecyptPassw = Decrypt(User.PassWord);
                        if (string.IsNullOrEmpty("DecyptPassw"))
                            ModelState.AddModelError(string.Empty, "Please input password.");
                        else if (DecyptPassw != u.PassWord)
                        {
                            ModelState.AddModelError(string.Empty, "Invalid user name or password.");
                        }
                        else if (DecyptPassw == u.PassWord)
                        {
                            Session["UserName"] = User.UserName.ToString();
                            Session["LogedUserID"] = User.Id.ToString();
                            Session["RoleName"] = User.Role.Name.ToString();
                            Session["LogedFullName"] = User.FullName.ToString();
                            return RedirectToAction("Index", "RoomUsage");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid user name or password.");
                    }
                }
            }
            catch (Exception ex)
            {
                MyLogger.Instance.TraceEvent(TraceEventType.Error, 0, System.DateTime.Now.ToString() + ": Login fail with error: " + ex.Message);
                ModelState.AddModelError(string.Empty, "Login fail. Please try again.");
            }
            return View();
        }
        public ActionResult LogOut()
        {
            //this action is for handle post (login)
            Session["LogedUserID"] = null;
            Session["LogedFullName"] = null;
            Session["RoleName"] = null;
            Session["UserName"] = null;
            return RedirectToAction("Login", "Account");
        }
        private string Encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        private string Decrypt(string cipherText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}
