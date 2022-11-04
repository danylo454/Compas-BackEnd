﻿using Compass.Data.Data.Context;
using Compass.Data.Data.Interfaces;
using Compass.Data.Data.Models;
using Compass.Data.Data.ViewModels.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compass.Data.Data.Classes
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public async Task<IdentityResult> DeleteUserAsync(AppUser user)
        {

            var result = await _userManager.DeleteAsync(user);
            return result;
        }
        public async Task<IdentityResult> DeleteUserRoleAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRoleAsync(user, roles[0]);
            return result;
        }
        public UserRepository(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<AppUser> FindByEmailAsync(LoginUserVM model)
        {
            var result = await _userManager.FindByEmailAsync(model.Email);
            return result;
        }

        public async Task<IdentityResult> RegisterUserAsync(AppUser model, string password)
        {
            var result = await _userManager.CreateAsync(model, password);
            return result;
        }

        public async Task<bool> ValidatePasswordAsync(LoginUserVM model, string password)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var result = await _userManager.CheckPasswordAsync(user, password);
            return result;
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(AppUser appUser)
        {
            var result = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            return result;
        }

        public async Task<AppUser> GetUserByIdAsync(string id)
        {
            var result = await _userManager.FindByIdAsync(id);
            return result;
        }

        public async Task<IdentityResult> ConfirmEmailAsync(AppUser model, string token)
        {
            var result = await _userManager.ConfirmEmailAsync(model, token);
            return result;
        }

        public async Task<AppUser> GetUserByEmailAsync(string email)
        {
            var result = await _userManager.FindByEmailAsync(email);
            return result;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(AppUser model)
        {
            var result = await _userManager.GeneratePasswordResetTokenAsync(model);
            return result;
        }

        public async Task<IdentityResult> ResetPasswordAsync(AppUser model, string token, string password)
        {
            var result = await _userManager.ResetPasswordAsync(model, token, password);
            return result;
        }

        public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            using (var _context = new AppDbContext())
            {
                await _context.RefreshTokens.AddAsync(refreshToken);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<RefreshToken> CheckRefreshTokenAsync(string refreshToken)
        {
            using(var _context = new AppDbContext())
            {
                var result = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);
                return result;
            }
        }

        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            using (var _context = new AppDbContext())
            {
                _context.RefreshTokens.Update(refreshToken);
                await _context.SaveChangesAsync();
            }
        }

        async Task<IList<string>> IUserRepository.GetRolesAsync(AppUser model)
        {
            var result = await _userManager.GetRolesAsync(model);
            return result;
        }

        public async Task<List<AppUser>> GetAllUsersAsync(int start, int end, bool isAll = false)
        {
            if (isAll)
            {
                return  await  _userManager.Users.ToListAsync();
            }
            return _userManager.Users.ToList().Take(new Range(new Index(start), new Index(end))).ToList();

        }

        public async Task AddToRoleAsync(AppUser model, string role)
        {
            await _userManager.AddToRoleAsync(model, role);
        }

        public async Task<SignInResult> LoginUserAsync(AppUser model, string password, bool rememberMe)
        {
            var result = await _signInManager.PasswordSignInAsync(model, password, rememberMe, lockoutOnFailure: true);
            return result;
        }

        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task DeleteUnussfullTokens(string userId)
        {
            using (var _context = new AppDbContext())
            {
                List<RefreshToken> tokens = await _context.RefreshTokens.Where(u => u.UserId  == userId).ToListAsync();
                foreach (RefreshToken token in tokens)
                {
                    _context.RefreshTokens.Remove(token);
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IdentityResult> ChangePasswordAsync(AppUser model, string oldPassword, string newPassword)
        {
            var result = await _userManager.ChangePasswordAsync(model, oldPassword, newPassword);
            return result;
        }

        public async Task<IdentityResult> EditUserAsunc(AppUser model)
        {
            var result = await _userManager.UpdateAsync(model);
            return result;
        }
    }
}
