namespace material_design.Services
{
    public interface IAuthService
    {
        Autorization Login(string login, string password);
    }
}