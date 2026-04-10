using Microsoft.AspNetCore.Mvc;
using AuthenticationService.Services;
using AuthenticationService.Models;

namespace AuthenticationService.Controllers
{
    /// <summary>
    /// Authentication Controller - Handles HTTP requests for login, logout, and token validation.
    /// Organization: 95bd4e80-e002-4fe5-ab71-fa85aad9fec8
    /// Project: 6ac6b43c-29aa-4b94-abde-18897563e8e1
    /// Service ID: AUTH-1
    /// Service Name: Authentication Service
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;

        public AuthController(ITokenService tokenService, IUserRepository userRepository)
        {
            _tokenService = tokenService;
            _userRepository = userRepository;
        }

        /// <summary>
        /// POST /login - Accepts user credentials, returns token on success.
        /// </summary>
        /// <param name="request">Login request containing username and password.</param>
        /// <returns>Authentication token on success, error on failure.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ErrorResponse { Error = "Invalid request. Username and password are required." });
            }

            // Verify credentials against user data store
            var isValid = await _userRepository.ValidateCredentialsAsync(request.Username, request.Password);

            if (!isValid)
            {
                return Unauthorized(new ErrorResponse { Error = "Invalid credentials." });
            }

            // Generate and return token
            var token = await _tokenService.GenerateTokenAsync(request.Username);

            return Ok(new LoginResponse { Token = token.Value, ExpiresAt = token.ExpiresAt });
        }

        /// <summary>
        /// POST /logout - Invalidates the current session/token.
        /// </summary>
        /// <param name="request">Logout request containing the token to invalidate.</param>
        /// <returns>Confirmation of logout.</returns>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest(new ErrorResponse { Error = "Token is required." });
            }

            var result = await _tokenService.InvalidateTokenAsync(request.Token);

            if (!result)
            {
                return BadRequest(new ErrorResponse { Error = "Token not found or already invalidated." });
            }

            return Ok(new LogoutResponse { Message = "Logout successful. Token invalidated." });
        }

        /// <summary>
        /// POST /validate - Validates a provided token.
        /// </summary>
        /// <param name="request">Validation request containing the token to validate.</param>
        /// <returns>Validity status of the token.</returns>
        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest(new ErrorResponse { Error = "Token is required." });
            }

            var validationResult = await _tokenService.ValidateTokenAsync(request.Token);

            if (!validationResult.IsValid)
            {
                return Unauthorized(new ValidateResponse 
                { 
                    IsValid = false, 
                    Error = validationResult.ErrorMessage ?? "Token is invalid or expired." 
                });
            }

            return Ok(new ValidateResponse 
            { 
                IsValid = true, 
                Username = validationResult.Username,
                ExpiresAt = validationResult.ExpiresAt 
            });
        }
    }
}
