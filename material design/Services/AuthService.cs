using System.Linq;
using material_design.Repositories;

namespace material_design.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<Autorization> _userRepo;

        public AuthService(IRepository<Autorization> userRepo)
        {
            _userRepo = userRepo;
        }

        public Autorization Login(string login, string password)
        {
            var user = _userRepo.GetAll().FirstOrDefault(u => u.Login == login);
            if (user == null) return null;
            return PasswordHelper.VerifyPassword(password, user.PasswordHash, user.Salt) ? user : null;
        }
    }
}