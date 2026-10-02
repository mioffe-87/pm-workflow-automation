
using System.Runtime.Serialization;

namespace Csp.Api.Exceptions {
	/// <summary>
	/// Class to create custom Csp exceptions
	/// </summary>
	public class CspException : System.Exception, ISerializable {
		public CspException()
			: base() {
			// Add implementation (if required)
		}

		public CspException(string message)
			: base(message) {
			// Add Implementation (if required)
		}

		public CspException(string message, System.Exception inner)
			: base(message, inner) {
			// Add implementation (if required)
		}

		protected CspException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
			// Add implementation (if required)
		}
	}
}
