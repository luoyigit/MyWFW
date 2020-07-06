using System.Threading.Tasks;

namespace User.Identity.Services
{
    public class TestAuthCodeService:IAuthCodeService
    {
        public async Task<bool> Validate(string phone, string authCode)
        {
            return await Task.FromResult(true);
        }
    }
}