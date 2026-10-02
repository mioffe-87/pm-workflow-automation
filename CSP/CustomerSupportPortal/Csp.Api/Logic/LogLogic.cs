using Csp.Api.EntityClasses;
using Csp.Api.Services.Contracts;
using PAN.Common.Core.Persistence;

namespace Csp.Api.Logic {
	public class LogLogic : BaseLogic, ILogLogic {
		private readonly IRepository _repository;

		public LogLogic(IRepository repository) {
			_repository = repository;
		}

		public void LogNewUser(Lognewuser logNewUser) {
			_repository.SaveAndCommit(logNewUser);
		}
	}
}
