namespace Csp.Api.Services.Contracts {
	public interface IEncryptionProvider {
		bool IsKeyValid(string value, string key, string privateKey);
		string GetMd5Hash(string input);
        string GetSha256(string input);
	}
}
