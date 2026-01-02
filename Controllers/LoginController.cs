using ASQL_Online_Exam_.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASQL_Online_Exam_.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext db;

        public LoginController(AppDbContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If student is already logged in, redirect to dashboard
            if (HttpContext.Session.GetInt32("StudentId") != null)
            {
                return RedirectToAction("Index", "Student");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            // Check student credentials
            var student = db.Students.FirstOrDefault(s => s.StudentEmail == email && s.StudentPasswordHashed == password);

            if (student != null)
            {
                
                HttpContext.Session.SetInt32("StudentId", student.StudentId);
                HttpContext.Session.SetString("StudentEmail", student.StudentEmail);

               
                return RedirectToAction("Index", "Home");
                 
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        public IActionResult Logout()
        {
            // Clear session
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
