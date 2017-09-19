﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MOUNB.Models;
using X.PagedList;
using System.Web.Configuration;

namespace MOUNB.Controllers
{
    [Authorize(Roles = "Администратор")]
    public class UsersController : Controller
    {
        private MounbDbContext db = new MounbDbContext();

        // GET: Users
        public async Task<ActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.LoginSortParm = sortOrder == "Login" ? "Login_desc" : "Login";
            ViewBag.PasswordSortParm = sortOrder == "Password" ? "Password_desc" : "Password";
            ViewBag.PositionSortParm = sortOrder == "Position" ? "Position_desc" : "Position";
            ViewBag.RoleSortParm = sortOrder == "Role" ? "Role_desc" : "Role";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var users = from s in await db.Users.ToListAsync()
                        select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(s => s.Name.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    users = users.OrderByDescending(s => s.Name);
                    break;

                case "Login":
                    users = users.OrderBy(s => s.Login);
                    break;
                case "Login_desc":
                    users = users.OrderByDescending(s => s.Login);
                    break;

                case "Password":
                    users = users.OrderBy(s => s.Password);
                    break;
                case "Password_desc":
                    users = users.OrderByDescending(s => s.Password);
                    break;

                case "Position":
                    users = users.OrderBy(s => s.Position);
                    break;
                case "Position_desc":
                    users = users.OrderByDescending(s => s.Position);
                    break;

                case "Role":
                    users = users.OrderBy(s => s.Role);
                    break;
                case "Role_desc":
                    users = users.OrderByDescending(s => s.Role);
                    break;

                default:  // Name ascending 
                    users = users.OrderBy(s => s.Name);
                    break;
            } // Конец  switch (sortOrder)

            int pageSize = Convert.ToInt32(WebConfigurationManager.AppSettings["pageSize"]);
            int pageNumber = (page ?? 1);
            return View(await users.ToPagedListAsync(pageNumber, pageSize));
        } // Конец index    

        // GET: Users/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Login,Password,Position,Role")] User user)
        {
            await CheckLogin(this.ModelState, user);

            if (ModelState.IsValid)
            {

                db.Users.Add(user);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // Проверка логина на уникальность 
        private async Task CheckLogin(ModelStateDictionary model, User user)
        {
            // поиск других пользователей с введённым логином
            var login = await (from l in db.Users
                        .Where(l => l.Login == user.Login)
                        .Where(t => t.Id != user.Id)
                               select l.Login).FirstOrDefaultAsync();

            if (login != null)
            {
                ModelState.AddModelError("Login", "Логин должен быть уникальным");
            }

        } // Конец метода

        // GET: Users/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Login,Password,Position,Role")] User user)
        {
            await CheckLogin(this.ModelState, user);

            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            User user = await db.Users.FindAsync(id);
            db.Users.Remove(user);
            await db.SaveChangesAsync();
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
