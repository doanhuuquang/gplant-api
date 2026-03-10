using System.Text.Json.Serialization;

namespace Gplant.Domain.enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BannerGroup
    {
        Carousel = 0,
        HomePopup = 1
    }
}
