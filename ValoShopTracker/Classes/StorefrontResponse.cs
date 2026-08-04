using System.Text.Json.Serialization;

namespace ValoShopTracker.Classes;

public class StorefrontResponse
{
    public SkinsPanelLayout SkinsPanelLayout { get; set; }
}


public class SkinsPanelLayout
{
    public List<string> SingleItemOffers { get; set; }
    public int SingleItemOffersRemainingDurationInSeconds { get; set; }
}