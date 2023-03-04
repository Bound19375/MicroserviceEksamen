using Auth.Application.Interface;
using Crosscut;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
