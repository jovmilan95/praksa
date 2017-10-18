using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using PraksaProjekat.Models;
using System.Data.Entity;
using PagedList;
using System.Net.Mail;
using System.Net;

namespace PraksaProjekat.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        /*
         Vazna stvar, da se unese email na koji ce stizati email i odakle se salju za kreiranje default admina 
         fu-ja je CreateDefaultAdmin
         *U klasi JobScheduler fu-ja .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(sat, minut)) postaviti u koje vreme da se salju emailovi
         Klasa EmailJob postaviti sa kog emaila se salju obavestenja, 
         a emailovi na koje se salju se cuvaju u bazi i na strani su Notification
         */
        private ApplicationDbContext db = new ApplicationDbContext();
        public UserManager<ApplicationUser> UserManager { get; private set; }
        public AccountController()
        {
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }



        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

       
        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    await SignInAsync(user, model.RememberMe);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser() { UserName = model.UserName,Email=model.Email, FirstName=model.FirstName,LastName=model.LastName};
                
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> UserProfile(string id,int page = 1, int pageSize=15)//id je username
        {
            var user = await UserManager.FindByNameAsync(id);
            
            return View(user);
        }

        [HttpGet]
        public ActionResult UserProfileHours(string id,int page=1,int pageSize=5)
        {
            var model = this.HoursSumForMonths(id, pageSize, (page - 1) * pageSize);
            
            ViewBag.UserName = id;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            int pageCount = (this.HoursSumForMonthsCount(id) + pageSize - 1) / pageSize;
            ViewBag.PageCount = pageCount;
            return PartialView(model);
        }

        [HttpGet]
        public ActionResult UserProfileContract(string id)
        {
            var model = db.Contracts.Where(x => x.User.UserName == id)
                                    .OrderByDescending(x => x.Id)
                                    .FirstOrDefault();
            ViewBag.UserName = UserManager.FindByName(id).UserName;

            return PartialView(model);
        }

        [HttpGet]
        public ActionResult AddContract(string id)
        {
            ViewBag.UserName = id;
           return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddContract(ContractViewModel model,string id)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(id);

                if (user == null)
                    return HttpNotFound();
                Contract contract = new Contract() {    ExpiryDate = model.ExpiryDate,
                                                        TipUgovora = model.TipUgovora, 
                                                        User = user  };
                db.Contracts.Add(contract);
                await db.SaveChangesAsync();
                return RedirectToAction("UserProfile", new { id = user.UserName });
            }
            return View();
        }
        [HttpGet]
        public ActionResult EditContract(int id)
        {
            var model = db.Contracts.Include(x => x.User).Where(x => x.Id == id).FirstOrDefault();
            if (model == null)
                return HttpNotFound();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditContract(Contract model)
        {
            if (ModelState.IsValid)
            {
                

                var contract = db.Contracts.Include(x=>x.User).Where(x=>x.Id==model.Id).FirstOrDefault();
                if (contract == null)
                    return HttpNotFound();
                var user = await UserManager.FindByNameAsync(contract.User.UserName);

                if (user == null)
                    return HttpNotFound();
                contract.TipUgovora = model.TipUgovora;
                contract.ExpiryDate = model.ExpiryDate;
                await db.SaveChangesAsync();
                return RedirectToAction("UserProfile", new { id = user.UserName });
            }
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> CreateDefaultAdmin()
        {
            if (db.Users.Where(x=>x.Roles.Any(r=>r.Role.Name=="Admin")).Count()==0)
            {
                var email = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8) + "@" + Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8) + ".com";
                var user = new ApplicationUser() { UserName = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8),
                                                   Email = email,
                                                   FirstName = "Admin",
                                                   LastName = "Admin"
                };
                var password= Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);
                var result = await UserManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    //await SignInAsync(user, isPersistent: false);
                    var admincontroler = new AdministratorController();
                    admincontroler.AdminRoleAddToUser(user.UserName);
                    //send email to boss
                    using (var message = new MailMessage("email@gmail.com", "email@gmail.com"))
                    {
                        message.Subject = "Default admin";
                        message.Body = "Username = " + user.UserName + " , password= " + password;
                        using (SmtpClient client = new SmtpClient
                        {
                            EnableSsl = true,
                            Host = "smtp.gmail.com",
                            Port = 587,
                            Credentials = new NetworkCredential("email@gmail.com", "password")
                        })
                        {
                            client.Send(message);
                        }
                    }

                    //end send email
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return RedirectToAction("Index", "Home");


        }

        public List<MonthlyHours> HoursSumForMonths(string userName,int lastMonthCount = 1,int skip=0)
        {
            var user = UserManager.FindByName(userName);
            var b = db.WorkingHours.Where(x => x.User.Id == user.Id)
                .GroupBy(r => new { r.Date.Year, r.Date.Month })
                 .Select(u => new { hours = u.Sum(q => q.Hours), month = u.Key.Month, year = u.Key.Year })
                 .OrderByDescending(u => u.year)
                 .ThenByDescending(u => u.month)
                 .Skip(skip)
                 .Take(lastMonthCount)
                .ToList()
                .Select(a => new MonthlyHours { Hours = a.hours, Date = new DateTime(a.year, a.month, 1) })
                .ToList();

            return b;
        }
        public int HoursSumForMonthsCount(string userName)
        {
            var user = UserManager.FindByName(userName);
            var b = db.WorkingHours.Where(x => x.User.Id == user.Id)
                .GroupBy(r => new { r.Date.Year, r.Date.Month })
                 .Select(u => new { hours = u.Sum(q => q.Hours), month = u.Key.Month, year = u.Key.Year })
                 .Count();
            return b;
        }
        public List<MonthlyHours> HoursSumForAllMonths(string userName)
        {
            var user = UserManager.FindByName(userName);
            var b = db.WorkingHours.Where(x => x.User.Id == user.Id)
                .GroupBy(r => new { r.Date.Year, r.Date.Month })
                 .Select(u => new { hours = u.Sum(q => q.Hours), month = u.Key.Month, year = u.Key.Year })
                 .OrderByDescending(u => u.year)
                 .ThenByDescending(u => u.month)
                .ToList()
                .Select(a => new MonthlyHours { Hours = a.hours, Date = new DateTime(a.year, a.month, 1) })
                .ToList();

            return b;
        }

        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
            }
        }

        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser() { UserName = model.UserName };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}