using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PraksaProjekat.Models;

namespace PraksaProjekat.Controllers
{

    [Authorize(Roles = "Admin")]
    public class NotificationController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Notification/
        public ActionResult Index()
        {
            return View(db.NotificationEmails.ToList());
        }

        // GET: /Notification/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Notification/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Id,Email")] NotificationEmail notificationemail)
        {
            if (ModelState.IsValid)
            {
                db.NotificationEmails.Add(notificationemail);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(notificationemail);
        }



        // GET: /Notification/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NotificationEmail notificationemail = db.NotificationEmails.Find(id);
            if (notificationemail == null)
            {
                return HttpNotFound();
            }
            return View(notificationemail);
        }

        // POST: /Notification/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            NotificationEmail notificationemail = db.NotificationEmails.Find(id);
            db.NotificationEmails.Remove(notificationemail);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
