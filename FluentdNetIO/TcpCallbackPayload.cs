using System.Net.Security;
using System.Net.Sockets;

namespace FluentdNetIO;

public class TcpCallbackPayload
{
	public TcpClient Client;

	public SslStream SslStream;

	public FluentdMessage Message;

	public void Clear()
	{
		if (Client != null)
		{
			Client.Close();
		}
		if (SslStream != null)
		{
			SslStream.Close();
		}
		if (Message != null)
		{
			Message.Clear();
		}
		Client = null;
		SslStream = null;
		Message = null;
	}
}
