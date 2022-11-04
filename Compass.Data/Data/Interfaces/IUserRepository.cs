
using Compass.Data.Data.Models;
using Compass.Data.Data.ViewModels.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compass.Data.Data.Interfaces
{
    public interface IUserRepository
    {
        Task<IdentityResult> RegisterUserAsync(AppUser model, string password);
        Task<AppUser> FindByEmailAsync(LoginUserVM model);
        Task<SignInResult> LoginUserAsync(AppUser model, string password, bool rememberMe);
        Task<bool> ValidatePasswordAsync(LoginUserVM model, string password);
        Task<string> GenerateEmailConfirmationTokenAsync(AppUser appUser);
        Task<AppUser> GetUserByIdAsync(string id);
        Task<IList<string>> GetRolesAsync(AppUser model);
        Task<AppUser> GetUserByEmailAsync(string email);
        Task<IdentityResult> ConfirmEmailAsync(AppUser model, string token);
        Task<string> GeneratePasswordResetTokenAsync(AppUser model);
        Task<IdentityResult> ResetPasswordAsync(AppUser model, string token, string password);
        Task SaveRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken> CheckRefreshTokenAsync(string refreshToken);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
        Task<List<AppUser>> GetAllUsersAsync(int start, int end, bool isAll = false);
        Task AddToRoleAsync(AppUser model, string role);
        Task LogoutUserAsync();
        Task DeleteUnussfullTokens(string userId);
        Task<IdentityResult>ChangePasswordAsync(AppUser model, string currentPassword, string newPassword);
        Task<IdentityResult> EditUserAsunc(AppUser model);
        Task<IdentityResult> DeleteUserAsync(AppUser user);
        Task<IdentityResult> DeleteUserRoleAsync(AppUser user);
    }
}
