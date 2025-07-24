using System.ComponentModel;

namespace Core.Enums
{
    public enum StatusType
    {
        [Description("On-going")]
        Ongoing,

        [Description("Resolved")]
        Resolved,

        [Description("Under Investigation")]
        Investigation,

        [Description("Critical")]
        Critical
    }
}
