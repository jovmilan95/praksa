using PagedList;
using PraksaProjekat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace PraksaProjekat.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministratorController : Controller
    {
        private static string adminRoleName = "Admin";
        private ApplicationDbContext db = new ApplicationDbContext();
        public UserManager<ApplicationUser> UserManager { get; private set; }
        public AdministratorController()
        {
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }
        public ActionResult Hours(int page=1, int pageSize=15)
        {
            if (pageSize == 0)
                pageSize = 1;
            var result = db.WorkingHours
                                    .Include(x=>x.User)
                                    .OrderByDescending(x => x.Date)
                                    .ThenByDescending(x => x.Hours)
                                    .Skip((page - 1)*pageSize).Take(pageSize)
                                    .ToList();
            int workingHoursCount = db.WorkingHours.Count();
            int pageCount = (workingHoursCount + pageSize - 1) / pageSize;
            ViewBag.PageCount = pageCount;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(result);
        }

        public ActionResult Users()
        {
            AccountController accountControler = new AccountController();
            
            var m = DateTime.Now.AddMonths(-1).Month;
            var model = db.Users.Select(x => new UserViewModel
            {
                UserName = x.UserName,
                FirstName = x.FirstName,
                LastName = x.LastName,
                AdminRole = x.Roles.Any(r => r.Role.Name == adminRoleName),
                HoursThisMonth = x.HoursList.Where(s => s.User.Id == x.Id && s.Date.Month == DateTime.Now.Month).Any() ? db.WorkingHours.Where(s => s.User.Id == x.Id && s.Date.Month == DateTime.Now.Month).Sum(s => s.Hours) : 0,
                HoursPrevMonth = x.HoursList.Where(s => s.User.Id == x.Id && s.Date.Month == m).Any() ? db.WorkingHours.Where(s => s.User.Id == x.Id && s.Date.Month == m).Sum(s => s.Hours) : 0,
                LastContract = x.ContractList.Where(s => s.User.Id == x.Id).OrderByDescending(s => s.Id).Select(s => new ContractViewModel() {ExpiryDate=s.ExpiryDate }).FirstOrDefault()
            }).ToList();
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminRoleAddToUser(string id)
        {
            ApplicationUser user = db.Users.Where(u => u.UserName == id).FirstOrDefault();
     
            var rm = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(new ApplicationDbContext()));
            if (rm.FindByName(adminRoleName) == null)
            {
                rm.Create(new IdentityRole(adminRoleName));
                
            }
            var account = new AccountController();
            account.UserManager.AddToRole(user.Id, adminRoleName);

            return Redirect("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminRoleRemoveToUser(string id)
        {
            ApplicationUser user = db.Users.Where(u => u.UserName == id).FirstOrDefault();

            var rm = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(new ApplicationDbContext()));
            var role = rm.FindByName(adminRoleName);
            if(role!=null && user.Roles.Any(x=>x.Role.Name==adminRoleName))
            {
                var account = new AccountController();
                account.UserManager.RemoveFromRole(user.Id, adminRoleName);
            }
            
            

            return Redirect("Users");
        }

      
    }
}
