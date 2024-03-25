namespace Grapio.Provider;

public class GrapioConfiguration
{
    public string Requester { get; set; } = string.Empty;
    public Uri ServerUri { get; set; } = new("http://localhost:3278");
    public string ConnectionString { get; set; } = "Data Source=grapio.db;Mode=ReadWriteCreate";
    public bool Offline { get; set; }
}
