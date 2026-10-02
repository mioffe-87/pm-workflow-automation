using System.ComponentModel;

namespace Csp.Api.Model.Enums
{
    public enum ManageUserAction 
    {
        [Description("create")]
        Create,

        [Description("get")]
        Get,

        [Description("update")]
        Update,

        [Description("delete")]
        Delete
    }
}
