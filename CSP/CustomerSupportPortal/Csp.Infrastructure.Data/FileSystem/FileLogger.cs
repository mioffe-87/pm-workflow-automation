using System.IO;
using Csp.Api.Services.Contracts;

namespace Csp.Infrastructure.Data.FileSystem {
	public class FileLogger : IFileLogger {

		private readonly IApplicationSettings _applicationSettings;

		public FileLogger(IApplicationSettings applicationSettings) {
			_applicationSettings = applicationSettings;
		}

		public void AppendLine(string line) {
			using (TextWriter tw = new StreamWriter(_applicationSettings.LoggerPath, append: true)) {
				tw.WriteLine(line);
			}
		}
	}
}
