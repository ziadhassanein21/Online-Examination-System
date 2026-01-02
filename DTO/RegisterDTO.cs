using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ASQL_Online_Exam_.DTO
{
    public class RegisterDTO
    {
    //    [Required(ErrorMessage = "Name is required.")]
    //    [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters.")]
    //    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces.")]
        public string StudentName { get; set; } = string.Empty;


        //[Required(ErrorMessage = "Email is required.")]
        //[EmailAddress(ErrorMessage = "Invalid email address.")]
        public string StudentEmail { get; set; } = string.Empty;


        //[Required(ErrorMessage = "Password is required.")]
        //[DataType(DataType.Password)]
        //[StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        //[RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*#?&]{6,}$",
        //    ErrorMessage = "Password must contain at least one letter and one number.")]
        public string Password { get; set; } = string.Empty;



        //[Phone(ErrorMessage = "Invalid phone number.")]
        //[RegularExpression(@"^\+?\d{10,15}$", ErrorMessage = "Phone number must be 10 to 15 digits.")]
        public string? StudentPhoneNumber { get; set; }



        //[Required(ErrorMessage = "Social Security Number is required.")]
        //[RegularExpression(@"^\d{14}$", ErrorMessage = "Social Security Number must be exactly 14 digits.")]
        public string? StudentSocialSecurityNumber { get; set; }



        //[Required(ErrorMessage = "Track selection is required.")]
        //[Range(1, int.MaxValue, ErrorMessage = "Please select a valid track.")]
        public int? TrackId { get; set; }
    }
}
