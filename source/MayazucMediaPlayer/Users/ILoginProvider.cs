using System.Threading.Tasks;

namespace MayazucMediaPlayer.Users
{
    public interface ILoginProvider
    {
        bool LoggedIn { get; }
        Task<bool> LoginAsync();
        Task LogoutAsync();
    }
}