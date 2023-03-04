using Crosscutting;

namespace Auth.Application.Interface
{
    public interface IAuthRepository
    {
        Task<List<AuthModelDTO>> Auth(AuthModelDTO model);
    }
}
