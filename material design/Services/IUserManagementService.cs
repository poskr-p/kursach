using System.Collections.Generic;

namespace material_design.Services
{
    public interface IUserManagementService
    {
        List<Autorization> GetAllUsers();
        void AddUser(string login, string password, byte accessLevel);
        void UpdateUser(Autorization user, string newPassword = null);
        void DeleteUser(int id);
        bool IsLastAdmin(int userId);
    }
}