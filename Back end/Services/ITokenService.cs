using Jumia.Jumia.Domain.Models;

namespace Jumia.Services
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}