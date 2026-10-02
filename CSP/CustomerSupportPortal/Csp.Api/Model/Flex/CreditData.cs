using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class CreditData
    {
        [JsonProperty("tokens")]
        public List<CreditDetail> Credits { get; set; }
    }
}

public class CreditQuantityUsed
{
    public List<Usagetransactionsv1> usageTransactionsV1 { get; set; }
}

public class Usagetransactionsv1
{
    public long tokenId { get; set; }
    public string productId { get; set; }
    public float quantityConsumed { get; set; }
    public Xrefcdata xrefCData { get; set; }
}

public class Xrefcdata
{
    public string serialNumber { get; set; }
}


public class LoginDetails
{
    public string serialNumber { get; set; }
    public string softwareVersion { get; set; }
    public string supportAccountId { get; set; }
    public DateTime lastLoginDate { get; set; }
}

public class Firewallsummary
{
    [JsonProperty("firewallSummary")]
    public List<LoginDetails> firewallSummary { get; set; }
}