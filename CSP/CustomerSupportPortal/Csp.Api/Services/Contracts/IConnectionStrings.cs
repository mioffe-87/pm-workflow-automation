
namespace Csp.Api.Services.Contracts {
	public interface IConnectionStrings {
		string CspDatabase { get; }
		string PANDatabase { get; }
		string PANLoginDatabase { get; }
        string MarketplaceDatabase { get; }
        string TempDatabase { get; }
	}
}
