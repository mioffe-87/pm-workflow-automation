using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums
{
    public enum DeploymentProfileRequestType
    {
        [Description("get_deployment_profile")]
        GETDEPLOYMENTPROFILE,

        [Description("edit_deployment_profile")]
        EDITDEPLOYMENTPROFILE,

        [Description("delete_deployment_profile")]
        DELETDEPLOYMENTPROFILE,

        [Description("get_cnx_deployment_profile")]
        GETCNXDEPLOYMENTPROFILE,

        [Description("edit_cnx_deployment_profile")]
        EDITCNXDEPLOYMENTPROFILE,

        [Description("delete_cnx_deployment_profile")]
        DELETCNXDEPLOYMENTPROFILE,

        [Description("renew_deployment_profile")]
        RENEWDEPLOYMENTPROFILE,

        [Description("renew_cnx_deployment_profile")]
        RENEWCNXDEPLOYMENTPROFILE

    }
}
