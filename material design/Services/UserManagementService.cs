using material_design.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace material_design.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IRepository<Autorization> _userRepo;

        public UserManagementService(IRepository<Autorization> userRepo)
        {
            _userRepo = userRepo;
        }

        public List<Autorization> GetAllUsers() => _userRepo.GetAll().ToList();

        public void AddUser(string login, string password, byte accessLevel)
        {
            if (_userRepo.GetAll().Any(u => u.Login == login))
                throw new InvalidOperationException("Логин уже существует");

            var (hash, salt) = PasswordHelper.GenerateHash(password);
            var user = new Autorization
            {
                Login = login,
                PasswordHash = hash,
                Salt = salt,
                accessLevel = accessLevel
            };
            _userRepo.Add(user);
            _userRepo.Save();
        }

        public void UpdateUser(Autorization user, string newPassword = null)
        {
            if (!string.IsNullOrEmpty(newPassword))
            {
                var (hash, salt) = PasswordHelper.GenerateHash(newPassword);
                user.PasswordHash = hash;
                user.Salt = salt;
            }
            _userRepo.Update(user);
            _userRepo.Save();
        }

        public void DeleteUser(int id)
        {
            var user = _userRepo.GetById(id);
            if (user != null)
            {
                _userRepo.Delete(user);
                _userRepo.Save();
            }
        }

        public bool IsLastAdmin(int userId)
        {
            var user = _userRepo.GetById(userId);
            if (user == null || user.accessLevel != 5) return false;
            return _userRepo.GetAll().Count(u => u.accessLevel == 5) <= 1;
        }
    }
}