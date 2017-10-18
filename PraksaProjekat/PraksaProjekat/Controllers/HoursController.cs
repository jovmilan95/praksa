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
    public class HoursController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Hours/
        public ActionResult Index()
        {
            return View(db.WorkingHours.ToList());
        }

        // GET: /Hours/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkingHours workinghours = db.WorkingHours.Find(id);
            if (workinghours == null)
            {
                return HttpNotFound();
            }
            return View(workinghours);
        }

        // GET: /Hours/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Hours/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Id,Hours,Date")] WorkingHours workinghours)
        {
            if (ModelState.IsValid)
            {
                db.WorkingHours.Add(workinghours);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(workinghours);
        }

        // GET: /Hours/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkingHours workinghours = db.WorkingHours.Find(id);
            if (workinghours == null)
            {
                return HttpNotFound();
            }
            return View(workinghours);
        }

        // POST: /Hours/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Id,Hours,Date")] WorkingHours workinghours)
        {
            if (ModelState.IsValid)
            {
                db.Entry(workinghours).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(workinghours);
        }

        // GET: /Hours/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkingHours workinghours = db.WorkingHours.Find(id);
            if (workinghours == null)
            {
                return HttpNotFound();
            }
            return View(workinghours);
        }

        // POST: /Hours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            WorkingHours workinghours = db.WorkingHours.Find(id);
            db.WorkingHours.Remove(workinghours);
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
