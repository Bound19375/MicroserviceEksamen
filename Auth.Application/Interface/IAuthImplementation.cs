using Crosscutting;

namespace Auth.Application.Interface
{
    public interface IAuthImplementation
    {
        Task<List<AuthModelDTO>> Auth(AuthModelDTO model);
    }
}
