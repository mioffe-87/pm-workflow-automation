using System.Runtime.Serialization;

namespace Csp.Api.Exceptions {

	public class CspNotificationException : CspException {

		/// <summary>
		/// This exception should be thrown if the error should be reported to the user
		/// </summary>
		public CspNotificationException() {

		}

		/// <summary>
		/// Contructor for the notification exception. Throw this exception if the user should be notified of the error message in
		/// a notification.
		/// </summary>
		/// <param name="message">The message for the exception.</param>
		public CspNotificationException(string message)
			: base(message) {

		}

		/// <summary>
		/// Overloaded contructor for the notification exception. Throw this exception if the user should be notified of the error message in
		/// a notification.
		/// </summary>
		/// <param name="message">The message for the exception.</param>
		/// <param name="inner">the inner exception</param>
		public CspNotificationException(string message, System.Exception inner)
			: base(message, inner) {

		}

		/// <summary>
		/// Overloaded contructor for the notification exception. Throw this exception if the user should be notified of the error message in
		/// a notification.
		/// <param name="info">The serialization information</param>
		/// <param name="context">Streaming context object</param>
		protected CspNotificationException(SerializationInfo info, StreamingContext context)
			: base(info, context) {

		}




	}

}
