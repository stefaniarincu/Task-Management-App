﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Threading.Tasks;
using TaskManagementApp.Data;
using TaskManagementApp.Models;
using Task = TaskManagementApp.Models.Task;

namespace TaskManagementApp.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private static string returnUrl = "";
        public CommentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                var comments = from comm in db.Comments.Include("Task").Include("Task.Project").Include("User")
                               orderby comm.CommentDate
                               select comm;

                var search = "";

                // Search engine
                if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
                {
                    search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();
                    List<int> commIds = db.Comments.Where
                                            (
                                             at => at.CommentContent.Contains(search)
                                             || at.User.UserName.Contains(search)
                                            ).Select(a => a.CommentId).ToList();

                    comments = db.Comments.Where(comment => commIds.Contains(comment.CommentId))
                                        .Include("User")
                                        .OrderBy(a => a.CommentDate);

                }
                ViewBag.SearchString = search;

                // Paging the comments
                int _perPage = 3;
                if (TempData.ContainsKey("message"))
                {
                    ViewBag.message = TempData["message"].ToString();
                }
                int totalItems = comments.Count();
                var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
                var offset = 0;
                if (!currentPage.Equals(0))
                {
                    offset = (currentPage - 1) * _perPage;
                }
                var paginatedComments = comments.Skip(offset).Take(_perPage);
                ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
                ViewBag.Comments = paginatedComments;
                ViewBag.TasksList = GetAllTasks();
                if (search != "")
                {
                    ViewBag.PaginationBaseUrl = "/Comments/Index/?search=" + search + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/Comments/Index/?page";
                }
                SetAccessRights();

                return View();
            }
            else
            {
                var user = _userManager.GetUserId(User);
                var teams = from team in db.Teams.Include("Project")
                            join member in db.TeamMembers
                                   on team.TeamId equals member.TeamId
                            where member.UserId == user
                            orderby team.TeamName
                            select team;

                var tasks = from task in db.Tasks.Include("Project").Include("Stat").Include("TeamMember.User")
                            join team in teams
                                    on task.ProjectId equals team.ProjectId
                            orderby team.TeamName, task.TaskTitle
                            select task;

                var comments = from comm in db.Comments.Include("Task").Include("Task.Project").Include("User")
                               join task in tasks
                                    on comm.TaskId equals task.TaskId
                               orderby task.TaskTitle, comm.CommentDate
                               select comm;

                var search = "";

                // Search engine
                if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
                {
                    search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();
                    List<int> commIds = db.Comments.Where
                                            (
                                             at => at.CommentContent.Contains(search)
                                             || at.User.UserName.Contains(search)
                                            ).Select(a => a.CommentId).ToList();

                    comments = db.Comments.Where(comment => commIds.Contains(comment.CommentId))
                                        .Include("User")
                                        .OrderBy(a => a.CommentDate);

                }
                ViewBag.SearchString = search;

                // Paging the comments
                int _perPage = 3;
                if (TempData.ContainsKey("message"))
                {
                    ViewBag.message = TempData["message"].ToString();
                }
                int totalItems = comments.Count();
                var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
                var offset = 0;
                if (!currentPage.Equals(0))
                {
                    offset = (currentPage - 1) * _perPage;
                }
                var paginatedComments = comments.Skip(offset).Take(_perPage);
                ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
                ViewBag.Comments = paginatedComments;
                ViewBag.TasksList = GetAllTasks();
                if (search != "")
                {
                    ViewBag.PaginationBaseUrl = "/Comments/Index/?search=" + search + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/Comments/Index/?page";
                }
                SetAccessRights();

                return View();
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            Comment comm = new Comment();
            ViewBag.TasksList = GetAllTasks();
            return View(comm);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult New([FromForm] Comment comment)
        {
            comment.CommentDate = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return View();
            }
            else
            {
                Comment comm = db.Comments.Include("User")
                                         .Where(c => c.CommentId == comment.CommentId)
                                         .First();

                return View(comm);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(int id)
        {
            Comment comm = db.Comments
                .Include("User")
                .Where(c => c.CommentId == id)
                .First();

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comm);
                db.SaveChanges();
                return Redirect("/Tasks/Show/" + comm.TaskId);
            }
            else
            {
                TempData["message"] = "You do not have the right to delete this comment!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Tasks");
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            var spl = Request.Headers["Referer"].ToString().Substring(10);
            spl = spl.Substring(spl.IndexOf("/"));
            returnUrl = spl;
            Comment comm = db.Comments
                                .Include("User")
                                .Where(c => c.CommentId == id)
                                .First();
            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {

                return View(comm);
            }
            else
            {
                TempData["message"] = "You do not have the right to edit this comment!";
                TempData["messageType"] = "alert-danger";
                return Redirect(returnUrl);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment comm = db.Comments
                            .Include("User")
                            .Where(c => c.CommentId == id)
                            .First();

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {

                    comm.CommentContent = requestComment.CommentContent;
                    comm.CommentDate = DateTime.Now;

                    db.SaveChanges();

                    return Redirect("/Tasks/Show/" + comm.TaskId);
                }
                else
                {
                    return View(requestComment);
                }
            }
            else
            {
               
                TempData["message"] = "You do not have the right to edit this comment!";
                TempData["messageType"] = "alert-danger";
                return Redirect(returnUrl);
            }
        }

        [NonAction]
        private void SetAccessRights()
        {
            ViewBag.ShowButtons = false;
            ViewBag.IsOrganizer = false;
            ViewBag.IsAdmin = false;
            ViewBag.ShowButtonTask = true;
            ViewBag.CurrentUser = _userManager.GetUserId(User);

            if (User.IsInRole("Admin"))
            {
                ViewBag.ShowButtons = true;
                ViewBag.IsAdmin = true;
            }
        }

        [NonAction]
        private IEnumerable<SelectListItem> GetAllTasks()
        {
            var selectList = new List<SelectListItem>();

            var teamMembers = db.TeamMembers.Where(tm => tm.UserId == _userManager.GetUserId(User)).ToList();
            var projects = new List<Project>();
            foreach (TeamMember tm in teamMembers)
            {
                foreach (Team t in db.Teams)
                {
                    if (tm.TeamId == t.TeamId)
                    {
                        var project = db.Projects.Find(t.ProjectId);
                        projects.Add(project);
                    }
                }
            }

            foreach (Task t in db.Tasks)
            {
                foreach (var p in projects)
                {
                    if (t.ProjectId == p.ProjectId)
                    {
                        selectList.Add(new SelectListItem
                        {
                            Value = t.TaskId.ToString(),
                            Text = t.TaskTitle.ToString()
                        });
                        break;
                    }
                }
            }
            return selectList;
        }
    }
}