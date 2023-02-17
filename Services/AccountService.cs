﻿using DoctorWebApi.Helpers;
using DoctorWebApi.Interfaces;
using DoctorWebApi.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DoctorWebApi.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _db;
        public AccountService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PagedList<User>> GetUsersAsync(UserParams userParams)
        {
            var query = (from user in _db.Users
                             join userRoles in _db.UserRoles on user.Id equals userRoles.UserId
                             join roles in _db.Roles.Where(x => x.Name == Roles.Doctor) on userRoles.RoleId equals roles.Id
                             select new User { 
                                    Id = user.Id,
                                    UserName = user.UserName,    
                                    Email = user.Email,
                                    EmailConfirmed = user.EmailConfirmed,
                                    PasswordHash = user.PasswordHash,
                                    SecurityStamp = user.SecurityStamp,
                                    PhoneNumber = user.PhoneNumber,
                                    Name = user.Name,
                                    Introduction = user.Introduction,  
                                    LastActive = user.LastActive,    
                                    RegisteredOn = user.RegisteredOn,
                                    Speciality = user.Speciality
                             }
                          );


            if(userParams.SearchName != null && userParams.SearchName != "")
            {
                query = query.Where(u => u.Name.Contains(userParams.SearchName));
            }

            if (userParams.Speciality != null && (userParams.Speciality != "" || userParams.Speciality != "Any"))
            {
                query = query.Where(u => u.Speciality == userParams.Speciality);
            }

            return await PagedList<User>.CreateAsync(query, userParams.PageNumber, userParams.PageSize, query.Count());
        }
    }
}