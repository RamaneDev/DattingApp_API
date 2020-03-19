using System.Threading.Tasks;
using DattingApp.API.Models;

namespace DattingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        public AuthRepository(DataContext context)
        {
            this._context = context;
        }

        private DataContext _context { get; set; }

        public Task<User> Login(string username, string password)
        {
            throw new System.NotImplementedException();
        }

        public Task<User> Register(User user, string password)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> UserExists(string username)
        {
            throw new System.NotImplementedException();
        }
    }
}