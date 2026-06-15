using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using FeedbackAPI.Models;
using System.Security.Cryptography;

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

                string checkQuery = "SELECT COUNT(1) FROM Customers WHERE Email = @Email OR Username = @Username";
                using var checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@Email", customer.Email);
                checkCmd.Parameters.AddWithValue("@Username", customer.Username);

                int existingCount = (int)checkCmd.ExecuteScalar();

                if (existingCount > 0)
                {
                    return Conflict("This email is already registered.");
                }

                string query = @"
                INSERT INTO Customers (FullName, Email, Username, PasswordHash)
                OUTPUT INSERTED.CustomerId
                VALUES (@FullName, @Email, @Username, @PasswordHash)";

                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@FullName", customer.FullName);
                cmd.Parameters.AddWithValue("@Email", customer.Email);
                cmd.Parameters.AddWithValue("@Username", customer.Username);
                cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(customer.PasswordHash));

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

                string query = "SELECT * FROM Customers WHERE Username = @Username";

                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Username", request.Username);

                using var reader = cmd.ExecuteReader();

                if (reader.Read() && VerifyPassword(request.PasswordHash, reader["PasswordHash"].ToString() ?? ""))
                {
                    int customerId = Convert.ToInt32(reader["CustomerId"]);
                    string username = reader["Username"].ToString() ?? "";
                    string fullName = reader["FullName"].ToString() ?? "";
                    string storedPassword = reader["PasswordHash"].ToString() ?? "";

                    if (!IsHashedPassword(storedPassword))
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

        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);

            return $"PBKDF2$100000${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string password, string storedPassword)
        {
            if (!IsHashedPassword(storedPassword))
            {
                return storedPassword == password;
            }

            string[] parts = storedPassword.Split('$');
            if (parts.Length != 4 || !int.TryParse(parts[1], out int iterations))
            {
                return false;
            }

            try
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expectedHash = Convert.FromBase64String(parts[3]);
                byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool IsHashedPassword(string storedPassword)
        {
            return storedPassword.StartsWith("PBKDF2$", StringComparison.Ordinal);
        }

        private static void UpdatePasswordHash(SqlConnection con, int customerId, string password)
        {
            using var cmd = new SqlCommand("UPDATE Customers SET PasswordHash = @PasswordHash WHERE CustomerId = @CustomerId", con);
            cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(password));
            cmd.Parameters.AddWithValue("@CustomerId", customerId);
            cmd.ExecuteNonQuery();
        }
    }
}
