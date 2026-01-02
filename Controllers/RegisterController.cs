using ASQL_Online_Exam_.DTO;
using ASQL_Online_Exam_.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ASQL_Online_Exam_.Controllers
{
    public class RegisterController : Controller
    {
        private readonly AppDbContext _context;

        public RegisterController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Tracks = _context.Tracks
                .Select(t => new SelectListItem
                {
                    Value = t.TrackId.ToString(),
                    Text = t.TrackName
                }).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(RegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                LoadTracks();
                return View(model);
            }

            string salt = Guid.NewGuid().ToString();
            string hashedPassword = HashPassword(model.Password, salt);

            var result = _context.SpResults
                .FromSqlRaw(
                    "EXEC create_student @SSN, @StudentName, @StudentEmail, @StudentPassword, @StudentPhone, @TrackId, @EnrollmentDate, @hashpassword, @salt",
                    new SqlParameter("@SSN", model.StudentSocialSecurityNumber),
                    new SqlParameter("@StudentName", model.StudentName),
                    new SqlParameter("@StudentEmail", model.StudentEmail),
                    new SqlParameter("@StudentPassword", model.Password),
                    new SqlParameter("@StudentPhone", model.StudentPhoneNumber ?? (object)DBNull.Value),
                    new SqlParameter("@TrackId", model.TrackId),
                    new SqlParameter("@EnrollmentDate", DateTime.Now),
                    new SqlParameter("@hashpassword", hashedPassword),
                    new SqlParameter("@salt", salt)
                )
                .AsEnumerable()
                .FirstOrDefault();

            if (result?.Result == "success")
            {
                ViewBag.Success = "Registration completed successfully ";
                ModelState.Clear();
                return RedirectToAction("Login","Login");
            }
            else
            {
                ViewBag.Error = result?.Result ?? "Unknown error";
            }

            LoadTracks();
            return View();
        }
        // 🔐 Hash Helper
        private string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            return Convert.ToBase64String(sha256.ComputeHash(bytes));
        }

        private void LoadTracks()
        {
            ViewBag.Tracks = _context.Tracks
                .Select(t => new SelectListItem
                {
                    Value = t.TrackId.ToString(),
                    Text = t.TrackName
                })
                .ToList();
        }

    }

}
