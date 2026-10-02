using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Web.Http.Filters;

namespace Csp.Web.Attributes
{
    public class GzipCompressionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionContext)
        {
            var content = actionContext.Response.Content;
            var bytes = content == null ? null : content.ReadAsByteArrayAsync().Result;
            var zlibbedContent = bytes == null ? new byte[0] :
                CompressionHelper.GzipByte(bytes);
            actionContext.Response.Content = new ByteArrayContent(zlibbedContent);
            actionContext.Response.Content.Headers.Remove("Content-Type");
            actionContext.Response.Content.Headers.Add("Content-encoding", "gzip");
            actionContext.Response.Content.Headers.Add("Content-Type", "application/json");
            base.OnActionExecuted(actionContext);
        }
    }

    public class CompressionHelper
    {
        public static byte[] GzipByte(byte[] str)
        {
            if (str == null)
            {
                return null;
            }

            using (var output = new MemoryStream())
            {
                using (var compressor = new GZipStream(output, CompressionMode.Compress))
                {
                    compressor.Write(str, 0, str.Length);
                }
                return output.ToArray();
            }
        }
    }
}