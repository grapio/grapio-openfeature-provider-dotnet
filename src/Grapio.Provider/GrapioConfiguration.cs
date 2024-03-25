namespace Grapio.Provider;

public class GrapioConfiguration
{
    public string Requester { get; init; } = string.Empty;
    public Uri ServerUri { get; set; } = new("http://localhost:3278");
    public string ConnectionString { get; init; } = "Data Source=grapio.db;Mode=ReadWriteCreate";
    public bool Offline { get; init; }
}
