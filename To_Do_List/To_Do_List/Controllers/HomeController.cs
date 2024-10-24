using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using To_Do_List.Models;
using Task = To_Do_List.Models.Task;

namespace To_Do_List.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public HomeController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public IActionResult verify(Task task)
        {
            var userId = _appDbContext.Users
            .Where(u => u.Name == HttpContext.User.Identity.Name)
            .Select(u => u.Id)
            .FirstOrDefault();
            if (task == null)
            {
                TempData["UnauthorizedMessage"] = "Task not found";
                return RedirectToAction("Index");
            }
            else if (task.UserId != userId)
            {
                TempData["UnauthorizedMessage"] = "You do not have permission to access this task.";
                return RedirectToAction("Index");
            }
            return null;
        }

        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Signup(SignupViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User();
                user.Name = model.Name;
                user.Password = model.Password;

                try
                {
                    _appDbContext.Users.Add(user);
                    _appDbContext.SaveChanges();

                    ModelState.Clear();
                    //ViewBag.Message = $"{user.Name} Registered Successfully. PLEASE LOGIN!";

                    return RedirectToAction("Login");
                }
                catch (DbUpdateException ex)
                {
                    Debug.WriteLine(ex.Message);
                    ModelState.AddModelError("", "Username Already Exists");
                    return View(model);
                }
            }
            return View(model); // this return is for if any error then user need not to re enter anything
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _appDbContext.Users.Where(x => x.Name == model.Name && x.Password == model.Password).FirstOrDefault();
                if (user != null)
                {
                    // success, create cookie and redirect to index
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Name)
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid Username or Password");
                }

            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [Authorize]
        public IActionResult Index(string filter = "all", string searchquery = "")
        {
            ViewBag.filter = filter; // this is for css to show in which category you are
            ViewBag.searchquery = searchquery;
            ViewData["name"] = HttpContext.User.Identity.Name.ToString();

            var userId = _appDbContext.Users
                        .Where(u => u.Name == HttpContext.User.Identity.Name)
                        .Select(u => u.Id)
                        .FirstOrDefault();

            var tasksQuery = _appDbContext.Tasks.Include(t => t.User).Where(t => t.UserId == userId);  //get all task of user

            if (!string.IsNullOrWhiteSpace(searchquery))
            {
                tasksQuery = tasksQuery.Where(t => t.Task_title.Contains(searchquery));
            }

            IEnumerable<Task> tasks;

            // now apply specific category filter and by default it is all task 
            switch (filter.ToLower())
            {
                case "completed":
                    tasks = tasksQuery.Where(t => t.IsComplete).ToList();
                    break;
                case "expired":
                    tasks = tasksQuery.Where(t => !t.IsComplete && t.DueDate < DateTime.Now).ToList();
                    break;
                case "pending":
                    tasks = tasksQuery.Where(t => !t.IsComplete && t.DueDate >= DateTime.Now).ToList();
                    break;
                default:
                    tasks = tasksQuery.ToList(); // All tasks
                    break;
            }

            return View(tasks);

        }

        [Authorize]
        public IActionResult Create_task()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create_task(TaskViewModel model)
        {
            if (ModelState.IsValid)
            {

                var userId = _appDbContext.Users
                        .Where(u => u.Name == HttpContext.User.Identity.Name)
                        .Select(u => u.Id)
                        .FirstOrDefault();
                if (userId == 0)
                { 
                    ModelState.AddModelError("", "User not found");
                    return View(model);
                }

                var task = new Task()
                {
                    Task_title = model.Task_title,
                    Task_description = model.Task_description,
                    DueDate = model.DueDate,
                    IsComplete = model.IsComplete,
                    UserId = userId
                };

                _appDbContext.Tasks.Add(task);
                _appDbContext.SaveChanges();

                ModelState.Clear();
                ViewBag.Message = "Task Added Successfully";
                return View();
            }
            return View(model);
        }

        [Authorize]
        public IActionResult Details(int id)
        {
            var task = _appDbContext.Tasks.Find(id);
            var verification = verify(task);
            if(verification != null)
            {
                return verification;
            }
            return View(task);
        }

        [HttpPost]
        [Authorize]
        public IActionResult CompleteTask(int id)
        {
            var task = _appDbContext.Tasks.Find(id);
            var verification = verify(task);
            if(verification != null)
            {
                return verification;
            }
            if (task != null)
            {
                task.IsComplete = true;
                _appDbContext.SaveChanges();
                return RedirectToAction("Details", new { id = task.TaskId });
            }

            return View("Details.cshtml", task);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Edit_Task(int id)
        {
            var task = _appDbContext.Tasks.Find(id);
            var verification = verify(task);
            if(verification != null)
            {
                return verification;
            }

            return View(task);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Edit_Task(Task task)
        {
            ModelState.Remove("User");
            ModelState.Remove("UserId");
            if (ModelState.IsValid)
            {
                var existingTask = _appDbContext.Tasks.Find(task.TaskId);
                if (existingTask == null)
                {
                    ModelState.AddModelError("", "Task not found");
                    return View(task);
                }
                var userId = _appDbContext.Users
                     .Where(u => u.Name == HttpContext.User.Identity.Name)
                     .Select(u => u.Id)
                     .FirstOrDefault();
                if(userId == 0)
                {
                    ModelState.AddModelError("", "User not found");
                    return View(task);
                }

                existingTask.Task_title = task.Task_title;
                existingTask.Task_description = task.Task_description;
                existingTask.DueDate = task.DueDate;
                existingTask.IsComplete = task.IsComplete;
                existingTask.UserId = userId;

                //_appDbContext.Attach(existingTask);
                _appDbContext.SaveChanges();
                return RedirectToAction("Details", new { id = task.TaskId });
            }
            else{
                IEnumerable<ModelError> allerror = ModelState.Values.SelectMany(v => v.Errors);
                foreach (ModelError err in allerror)
                {
                    ModelState.AddModelError("", err.ErrorMessage);
                }
            }
            return View(task);
        }



        [HttpPost]
        [Authorize]
        public IActionResult DeleteTask(int id)
        {
            var task = _appDbContext.Tasks.Find(id);
            var verificcation = verify(task);
            if(verificcation != null)
            {
                return verificcation;
            }
            _appDbContext.Tasks.Remove(task);
            _appDbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
