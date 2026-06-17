using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using FeedbackAPI.Models;

namespace FeedbackAPI.Controllers
{
    public partial class AuthController
    {
        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            try
            {
                string? connectionString =
                    _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return StatusCode(500, "Database connection is not configured.");
                }

                using var con = new SqlConnection(connectionString);
                con.Open();

                using var cmd = new SqlCommand("sp_GetCustomerByUsername", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", request.Username);

                using var reader = cmd.ExecuteReader();

                if (reader.Read() && _passwordHashingService.VerifyPassword(request.PasswordHash, reader["PasswordHash"].ToString() ?? ""))
                {
                    int customerId = Convert.ToInt32(reader["CustomerId"]);
                    string username = reader["Username"].ToString() ?? "";
                    string fullName = reader["FullName"].ToString() ?? "";
                    string storedPassword = reader["PasswordHash"].ToString() ?? "";

                    if (!_passwordHashingService.IsHashedPassword(storedPassword))
                    {
                        reader.Close();
                        UpdatePasswordHash(con, customerId, request.PasswordHash);
                    }

                    return Ok(new
                    {
                        message = "Login Successful",
                        customerId,
                        username,
                        fullName,
                        role = "customer"
                    });
                }

                return Unauthorized("Invalid Username or Password");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
