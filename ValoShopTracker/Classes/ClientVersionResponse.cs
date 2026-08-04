namespace ValoShopTracker.Classes;

public class ClientVersionResponse
{
    public int status { get; set; }
    public Data data { get; set; }
}

public class Data
{
    public string riotClientVersion { get; set; }
}