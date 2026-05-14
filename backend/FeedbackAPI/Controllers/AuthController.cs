using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using FeedbackAPI.Models;

namespace FeedbackAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register(Customer customer)
        {
            try
            {
                string connectionString =
                    _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
                    INSERT INTO Customers
                    (FullName, Email, Username, PasswordHash)

                    VALUES

                    (@FullName, @Email, @Username, @PasswordHash)
                    ";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@FullName", customer.FullName);

                    cmd.Parameters.AddWithValue("@Email", customer.Email);

                    cmd.Parameters.AddWithValue("@Username", customer.Username);

                    cmd.Parameters.AddWithValue("@PasswordHash", customer.PasswordHash);

                    cmd.ExecuteNonQuery();
                }

                return Ok("User Registered Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            try
            {
                string connectionString =
                    _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
                    SELECT * FROM Customers
                    WHERE Username = @Username
                    AND PasswordHash = @PasswordHash
                    ";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@Username", request.Username);

                    cmd.Parameters.AddWithValue("@PasswordHash", request.PasswordHash);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        return Ok(new
                        {
                            message = "Login Successful",
                            customerId = reader["CustomerId"],
                            username = reader["Username"]
                        });
                    }

                    return Unauthorized("Invalid Username or Password");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}