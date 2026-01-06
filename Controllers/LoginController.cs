using ASQL_Online_Exam_.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

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

            // First find the student by email to get their salt
            var student = db.Students.FirstOrDefault(s => s.StudentEmail == email);
            
            if (student != null && !string.IsNullOrEmpty(student.StudentPasswordSalt))
            {
                // Hash the entered password with the stored salt
                string hashedInputPassword = HashPassword(password, student.StudentPasswordSalt);
                
                // Compare the hashed passwords
                if (student.StudentPasswordHashed != hashedInputPassword)
                {
                    student = null; // Password doesn't match
                }
            }
            else
            {
                student = null; // Student not found or no salt stored
            }

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

        // 🔐 Hash Helper - same algorithm as RegisterController
        private string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            return Convert.ToBase64String(sha256.ComputeHash(bytes));
        }
    }
}
