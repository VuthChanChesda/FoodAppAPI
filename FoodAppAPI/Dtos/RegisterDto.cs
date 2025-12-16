using System.ComponentModel.DataAnnotations;

namespace FoodAppAPI.Dtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "User name is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

    }
}
