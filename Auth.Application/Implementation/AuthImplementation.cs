using Auth.Application.Interface;
using Crosscutting;

namespace Auth.Application.Implementation
{
    public class AuthImplementation : IAuthImplementation
    {
        private readonly IAuthRepository _authRepository;
        public AuthImplementation(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        async Task<List<AuthModelDTO>> IAuthImplementation.Auth(AuthModelDTO model)
        {
            try
            {
                return await _authRepository.Auth(model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
