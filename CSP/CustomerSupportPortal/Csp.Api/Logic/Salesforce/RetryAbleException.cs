using System;
using System.Runtime.Serialization;

namespace Csp.Api.Logic.Salesforce {
	/// <summary>
	/// This is an exception wrapper to identify salesforce exceptions which can be re-tried
	/// Basically any salesforce generated exception ban be re-tried up to 10 times
	/// 
	/// Why create all these exception constructors?
	/// See here: http://stackoverflow.com/questions/2999298/difference-between-throw-and-throw-new-exception
	/// or here: https://msdn.microsoft.com/en-us/library/ms182151.aspx
	/// </summary>
	[Serializable()]
	public class RetryAbleException : Exception {

		#region Default Exceptoin Constructors (not used but required)
		public RetryAbleException() {

		}

		public RetryAbleException(string message)
			: base(message) {

		}

		public RetryAbleException(string message, Exception innerException) :
			base(message, innerException) {

		}

		protected RetryAbleException(SerializationInfo info, StreamingContext context)
			: base(info, context) {

		}
		#endregion

		/// <summary>
		/// This is the only constructor that will be used
		/// the rest are required as a best practice
		/// </summary>
		/// <param name="innerException"></param>
		public RetryAbleException(Exception innerException)
			: base("RetryAble Exception", innerException) {

		}
	}
}