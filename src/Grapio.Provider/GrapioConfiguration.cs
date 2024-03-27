namespace Grapio.Provider;

public class GrapioConfiguration
{
    public string Requester { get; set; } = string.Empty;
    public string ServerUri { get; set; } = "http://localhost:3278";
    public string ConnectionString { get; set; } = "Data Source=grapio.db;Mode=ReadWriteCreate";
    public bool Offline { get; set; }
    public int RefreshInterval { get; set; } = 300;
}
