using Csp.Api.Model;

namespace Csp.Api.Services.Contracts 
{
	public interface IVmRegistrationService 
    {
        void CorrectVmRegistrationData(EVmType vmType);
    }
}
