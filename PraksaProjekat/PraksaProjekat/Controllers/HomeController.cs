using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PraksaProjekat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PraksaProjekat.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private UserManager<ApplicationUser> UserManager { get; set; }

        public HomeController()
        {
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }

        //
        //Get: /Home/Index/
        public ActionResult Index()
        {
            if (TempData["message"] !=null && !String.IsNullOrWhiteSpace(TempData["message"].ToString()))
            {
                ViewBag.Message = TempData["message"];
            }
            return View(new WorkingHoursViewModel() { Date=DateTime.Now,Hours=0});
        }

        //
        // POST: /Home/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(WorkingHoursViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(User.Identity.Name);
                var workingHours = new WorkingHours() { Date = model.Date, Hours = model.Hours,User=user };
                db.WorkingHours.Add(workingHours);

                await db.SaveChangesAsync();
                ModelState.Clear();
                TempData["message"] = "Uspesno ste dodali sate!";
                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}