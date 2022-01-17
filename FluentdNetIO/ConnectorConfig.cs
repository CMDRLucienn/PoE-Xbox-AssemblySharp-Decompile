namespace FluentdNetIO;

public class ConnectorConfig
{
	public string TableTag { get; set; }

	public string HostName { get; set; }

	public string Host { get; set; }

	public string CertificatePath { get; set; }

	public int Port { get; set; }

	public int MaxPacketCount { get; set; }

	public int MaxSendPoolCount { get; set; }

	public ConnectorConfig()
	{
		TableTag = string.Empty;
		HostName = string.Empty;
		Host = string.Empty;
		CertificatePath = string.Empty;
		Port = 0;
		MaxPacketCount = 50;
		MaxSendPoolCount = 3;
	}
}
