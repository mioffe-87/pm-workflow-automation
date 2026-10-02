namespace Csp.Api.Model
{
    public class CornerstoneUserEmailUpdateResponse
    {
        public string Success { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public CornerstoneAccountData Data { get; set; }
    }
}
