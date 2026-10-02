using System;
using System.Diagnostics.CodeAnalysis;

namespace Csp.Api.Services.Contracts
{
    public interface IAutoFocusApiRepository
    {
        AfApiKey GetKey(long supportAccountId, long userAccountId);
        AfApiKey PutStatusUpdate(long supportAccountId, bool enable, long userAccountId);
        AfApiKey PutLicenseKey(long supportAccountId, int useCount, DateTime expirationDateTime, long userAccountId);
        AfApiKey PostKeyCreation(long supportAccountId, long userAccountId);
        AfApiKey DeleteKey(long supportAccountId, long userAccountId);
    }

    public class AfApiKey
    {
        public string Key { get; set; }
        public AfApiKeyStatuses Status { get; set; }
        public string Message { get; set; }

        public bool IsErrorMessage
        {
            get
            {
                return !string.IsNullOrEmpty(Message);
            }
        }
    }

    public enum AfApiKeyStatuses
    {
        Enabled,
        Disabled,
        Deleted,
        Unknown,
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class JsonResults
    {
        public class GetKeyJson
        {
            public string key { get; set; }
            public string status { get; set; }

            public bool IsDeleted
            {
                get { return string.IsNullOrEmpty(key) && string.IsNullOrEmpty(status); }
            }
        }
    }
}

/*

Hi Michael, Gayathri,

Dummy end points are up on bd9.paloaltonetworks.local:5005

For example:
curl -XGET bd9.paloaltonetworks.local:5005/api/key/245

Please look at
https://intranet.paloaltonetworks.com/docs/DOC-16108
 for documentation on the end points.

Implementation should be ready by sometime tomorrow.

Both delete and disable will be supported, it is up to you guys if you want to use it.

 * 
 * 
 * 
 * 
 * 
Hi,
 
There can be the following three states:
1) No API key
2) Enabled API key
3) Disabled API key
 
And there can be the following transitions:
1 -> 2
2 -> 1, 3
3 -> 1, 2
 
End points:
To support the above, AF can provide the following four end points:
 
1) GET /api/key/:supportId
 
2) POST /api/key/:supportId
Request body = {
                numUsers: 2,
                expiration: '2015-08-20'
}
 
3) DELETE /api/key/:supportId
 
4) PUT /api/key/:supportId
Request body = {
                status: 'enabled',
                numUsers: 2,
                expiration: '2015-08-20'
}
 
Responses:
If there are no errors, the response will have a status of 200
GET, POST and PUT will return
{
                key: 'abc',
                status: 'enabled'
}
If other data about the key is needed, that can be returned too.
DELETE will return {}
 
If no key is set, GET will return {}
If no key is set, DELETE and PUT will return a status of 404
 
 
If there are errors in processing, the response will be:
Status: 500
{
                message: 'some error message'
}
 
Communication:
AF will need a way to authenticate requests from CSP.
How should we do that?
 
Questions:
How do we use autofocusLicenseAuthCode, and parttype
Is there is a way to determine supportId from autofocusLicenseAuthCode? 
Then supportId can be removed from the url-parameters
 
Thanks,
Tarun

*/