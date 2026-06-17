using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using FeedbackAPI.Models;

namespace FeedbackAPI.Controllers
{
    public partial class AuthController
    {
        [HttpPost("admin-login")]
        public IActionResult AdminLogin(LoginRequest request)
        {
            string? adminEmail = _configuration["AdminAccount:Email"];
            string? adminPassword = _configuration["AdminAccount:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                return StatusCode(500, "Admin account is not configured.");
            }

            if (request.Username == adminEmail && request.PasswordHash == adminPassword)
            {
                return Ok(new
                {
                    message = "Admin Login Successful",
                    username = "Admin",
                    role = "admin"
                });
            }

            return Unauthorized("Invalid Admin Credentials");
        }

        private void UpdatePasswordHash(SqlConnection con, int customerId, string password)
        {
            using var cmd = new SqlCommand("sp_UpdateCustomerPasswordHash", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PasswordHash", _passwordHashingService.HashPassword(password));
            cmd.Parameters.AddWithValue("@CustomerId", customerId);
            cmd.ExecuteNonQuery();
        }
    }
}
