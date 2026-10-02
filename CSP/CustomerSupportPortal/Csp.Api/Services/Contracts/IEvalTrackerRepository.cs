namespace Csp.Api.Services.Contracts {
	public interface IEvalTrackerRepository {
		EvalTrackerData GetSfAccountDataByEvalId(string evalId);
	}

	public class EvalTrackerData {
		public string SfAccountId { get; set; }
		public string[] DeviceCodes { get; set; }
	}
}
