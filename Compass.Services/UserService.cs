using AutoMapper;
using Compass.Data.Data.Interfaces;
using Compass.Data.Data.Models;
using Compass.Data.Data.ViewModels.Users;
using Compass.Services.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;


namespace Compass.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private IConfiguration _configuration;
        private EmailService _emailService;
        private JwtService _jwtService;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, JwtService jwtService, IConfiguration configuration, EmailService emailService, IMapper mapper, IOptionsMonitor<JwtConfig> optionsMonitor, TokenValidationParameters tokenValidationParameters)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _emailService = emailService;
            _jwtService = jwtService;
            _mapper = mapper;
        }
        public async Task<ServiceResponse> RegisterUserAsync(RegisterUserVM model)
        {
        
            if (model.Password != model.ConfirmPassword)
            {
                return new ServiceResponse
                {
                    Message = "Confirm pssword do not match",
                    IsSuccess = false
                };
            }

            var newUser = _mapper.Map<RegisterUserVM, AppUser>(model);

            var result = await _userRepository.RegisterUserAsync(newUser, model.Password);
            if (result.Succeeded)
            {
                await _userRepository.AddToRoleAsync(newUser, model.Role);
                var token = await _userRepository.GenerateEmailConfirmationTokenAsync(newUser);

                var encodedEmailToken = Encoding.UTF8.GetBytes(token);
                var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

                string url = $"{_configuration["HostSettings:URL"]}/api/User/confirmemail?userid={newUser.Id}&token={validEmailToken}";

                string emailBody = $"<h1>Confirm your email</h1> <a href='{url}'>Confirm now</a>";
                await _emailService.SendEmailAsync(newUser.Email, "Email confirmation.", emailBody);

                var tokens = await _jwtService.GenerateJwtTokenAsync(newUser);

                return new ServiceResponse
                {
                    Message = "User successfully created.",
                    IsSuccess = true
                };
            }
            else
            {
                return new ServiceResponse
                {
                    Message = "Error user not created.",
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description)
                };
            }
        }

        public async Task<ServiceResponse> LoginUserAsync(LoginUserVM model)
        {
            var user = await _userRepository.FindByEmailAsync(model);

            if (user == null)
            {
                return new ServiceResponse
                {
                    Message = "Login or password incorrect.",
                    IsSuccess = false
                };
            }

            var signInResult = await _userRepository.LoginUserAsync(user, model.Password, model.RememberMe);
            if (signInResult.Succeeded)
            {

                var tokens = await _jwtService.GenerateJwtTokenAsync(user);

                return new ServiceResponse
                {
                    AccessToken = tokens.token,
                    RefreshToken = tokens.refreshToken.Token,
                    Message = "Logged in successfully",
                    IsSuccess = true,
                };
            }
            if (signInResult.IsNotAllowed)
            {
                return new ServiceResponse
                {
                    Message = "User cannot sign in without a confirmed email.",
                    IsSuccess = false,
                };

            }
            if (signInResult.IsLockedOut)
            {
                return new ServiceResponse
                {
                    Message = "User is blocked",
                    IsSuccess = false
                };
            }

            return new ServiceResponse
            {
                Message = "Login or password incorrect.",
                IsSuccess = false,
            };

        }

        public async Task<ServiceResponse> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "User not found"
                };

            var decodedToken = WebEncoders.Base64UrlDecode(token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userRepository.ConfirmEmailAsync(user, normalToken);

            if (result.Succeeded)
                return new ServiceResponse
                {
                    Message = "Email confirmed successfully!",
                    IsSuccess = true,
                };

            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Email did not confirm",
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        public async Task<ServiceResponse> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return new ServiceResponse
                {
                    Message = "No user associated with email",
                    IsSuccess = false
                };
            }

            var token = await _userRepository.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Encoding.UTF8.GetBytes(token);
            var validToken = WebEncoders.Base64UrlEncode(encodedToken);

            string url = $"{_configuration["HostSettings:URL"]}/ResetPassword?email={email}&token={validToken}";
            string emailBody = "<h1>Follow the instructions to reset your password</h1>" + $"<p>To reset your password <a href='{url}'>Click here</a></p>";
            await _emailService.SendEmailAsync(email, "Fogot password", emailBody);

            return new ServiceResponse
            {
                IsSuccess = true,
                Message = $"Reset password for {_configuration["HostSettings:URL"]} has been sent to the email successfully!"
            };
        }

        public async Task<ServiceResponse> ResetPasswordAsync(ResetPasswordVM model)
        {
            var user = await _userRepository.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "No user associated with email",
                };
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Password doesn't match its confirmation",
                };
            }

            var decodedToken = WebEncoders.Base64UrlDecode(model.Token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userRepository.ResetPasswordAsync(user, normalToken, model.NewPassword);
            if (result.Succeeded)
            {
                return new ServiceResponse
                {
                    Message = "Password has been reset successfully!",
                    IsSuccess = true,
                };
            }
            return new ServiceResponse
            {
                Message = "Something went wrong",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description),
            };
        }

        public async Task<ServiceResponse> RefreshTokenAsync(TokenRequestVM model)
        {
            var result = await _jwtService.VerifyTokenAsync(model);
            return result;
        }

        public async Task<ServiceResponse> GetAllUsersAsync(int start, int end, bool isAll = false)
        {
            List<AppUser> users = await _userRepository.GetAllUsersAsync(start, end, isAll);
            List<AllUsersVM> mappedUsers = users.Select(u => _mapper.Map<AppUser, AllUsersVM>(u)).ToList();

            for (int i = 0; i < users.Count; i++)
            {
                mappedUsers[i].Role = (await _userRepository.GetRolesAsync(users[i])).FirstOrDefault();
            }

            return new ServiceResponse()
            {
                IsSuccess = true,
                Payload = mappedUsers,
                Message = "All users loaded."
            };
        }

        public async Task<ServiceResponse> LogoutUserAsync(string userId)
        {
            await _userRepository.DeleteUnussfullTokens(userId);
            await _userRepository.LogoutUserAsync();
            return new ServiceResponse()
            {
                IsSuccess = true,
                Message = "User successfully logged out."
            };
        }

        public async Task<ServiceResponse> ChangePasswordAsync(ChangePasswordVM model)
        {
            var user = await _userRepository.GetUserByIdAsync(model.UserId);
            if(user == null)
            {
                return new ServiceResponse
                {
                    Message = "User not found.",
                    IsSuccess = false
                };
            }

            if(model.NewPassword != model.ConfirmPassword)
            {
                return new ServiceResponse
                {
                    Message = "Password do not match.",
                    IsSuccess = false
                };
            }


            var result = await _userRepository.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return new ServiceResponse
                {
                    Message = "Password successfully updated.",
                    IsSuccess = true,
                };
            }
            else
            {
                return new ServiceResponse
                {
                    Message = "Password not updated.",
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description),
                };
            }
           
        }

        public async Task<ServiceResponse> UpdateProfileAsync(UpdateProfileVM model)
        {
            var user = await _userRepository.GetUserByEmailAsync(model.Email);
            if(user == null)
            {
                return new ServiceResponse
                {
                    Message = "User not found.",
                    IsSuccess = false
                };
            }
            else
            {
                var updatedUser = _mapper.Map<AppUser>(user);
                return new ServiceResponse
                {
                    Message = "User not found.",
                    IsSuccess = false
                };

            }
        }
        public async Task<ServiceResponse> EditUserAsync(EditUserVM model)
        {

            var user = await _userRepository.GetUserByIdAsync(model.Id);
            if (user == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = $"User with {model.Id} is not exist !!!"
                };
            }
            var newUser = _mapper.Map<EditUserVM, AppUser>(model, user);
            var result = await _userRepository.EditUserAsunc(newUser);

            if (result.Succeeded)
            {
                return new ServiceResponse
                {
                    Message = "User has been updated successfully!",
                    IsSuccess = true,
                };
            }
            return new ServiceResponse
            {
                Message = "Something went wrong",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description),
            };
        }
        public async Task<ServiceResponse> DeleteUser(string id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "No user found!!!",
                };
            }
            var resultRoles = await _userRepository.DeleteUserRoleAsync(user);
            if (!resultRoles.Succeeded)
            {
                return new ServiceResponse
                {
                    Message = "Something went width roles",
                    IsSuccess = true,
                };
            }
            var result = await _userRepository.DeleteUserAsync(user);

            if (result.Succeeded)
            {
                return new ServiceResponse
                {
                    Message = "User has been deleted successfully!",
                    IsSuccess = true,
                };
            }
            return new ServiceResponse
            {
                Message = "Something went wrong",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description),
            };
        }
    }
}
