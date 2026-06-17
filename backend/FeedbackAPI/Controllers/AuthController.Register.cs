using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using FeedbackAPI.Models;

namespace FeedbackAPI.Controllers
{
    public partial class AuthController
    {
        [HttpPost("register")]
        public IActionResult Register(Customer customer)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(customer.FullName) ||
                    string.IsNullOrWhiteSpace(customer.Email) ||
                    string.IsNullOrWhiteSpace(customer.PasswordHash))
                {
                    return BadRequest("Name, email and password are required.");
                }

                string? connectionString =
                    _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return StatusCode(500, "Database connection is not configured.");
                }

                using var con = new SqlConnection(connectionString);
                con.Open();

                using var checkCmd = new SqlCommand("sp_CustomerExists", con);
                checkCmd.CommandType = CommandType.StoredProcedure;
                checkCmd.Parameters.AddWithValue("@Email", customer.Email);
                checkCmd.Parameters.AddWithValue("@Username", customer.Username);

                int existingCount = (int)checkCmd.ExecuteScalar();

                if (existingCount > 0)
                {
                    return Conflict("This email is already registered.");
                }

                using var cmd = new SqlCommand("sp_RegisterCustomer", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FullName", customer.FullName);
                cmd.Parameters.AddWithValue("@Email", customer.Email);
                cmd.Parameters.AddWithValue("@Username", customer.Username);
                cmd.Parameters.AddWithValue("@PasswordHash", _passwordHashingService.HashPassword(customer.PasswordHash));

                int customerId = (int)cmd.ExecuteScalar();

                return Ok(new
                {
                    message = "User Registered Successfully",
                    customerId,
                    username = customer.Email,
                    fullName = customer.FullName,
                    role = "customer"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
