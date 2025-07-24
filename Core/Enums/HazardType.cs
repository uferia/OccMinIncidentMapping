using System.ComponentModel;

namespace Core.Enums
{
    public enum HazardType
    {
        [Description("Flood")]
        Flood,

        [Description("Landslide")]
        Landslide,

        [Description("Power Outage")]
        PowerOutage,

        [Description("Impassable Road")]
        ImpassableRoad
    }
}
