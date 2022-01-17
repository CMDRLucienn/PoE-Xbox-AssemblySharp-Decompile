using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using MsgPack.Serialization;

namespace FluentdNetIO;

public class FluentdConnector
{
	public delegate void LogHandler(string message);

	private DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	private FluentdMessage[] messagePool;

	private TcpCallbackPayload[] payloadPool;

	private int msgPoolIndex;

	public ConnectorConfig Config { get; private set; }

	public event LogHandler Log;

	public FluentdConnector(ConnectorConfig config)
	{
		Config = config;
		X509Store x509Store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
		x509Store.Open(OpenFlags.ReadWrite);
		x509Store.Add(new X509Certificate2(Config.CertificatePath));
		x509Store.Close();
		SetupMessagePool();
	}

	private void SetupMessagePool()
	{
		if (messagePool == null)
		{
			messagePool = new FluentdMessage[Config.MaxSendPoolCount];
			payloadPool = new TcpCallbackPayload[messagePool.Length];
			msgPoolIndex = 0;
			for (int i = 0; i < messagePool.Length; i++)
			{
				payloadPool[i] = new TcpCallbackPayload();
				messagePool[i] = new FluentdMessage(Config.TableTag, Config.MaxPacketCount);
			}
		}
	}

	public void SendMessage(TcpCallbackPayload payload, FluentdMessage message)
	{
		try
		{
			TcpClient tcpClient = (payload.Client = new TcpClient());
			payload.Message = message;
			tcpClient.BeginConnect(Config.Host, Config.Port, OnTcpConnect, payload);
		}
		catch (Exception ex)
		{
			if (this.Log != null)
			{
				this.Log("ANALYTICS: TCP Connection Failure! " + ex.ToString());
			}
		}
	}

	private void OnTcpConnect(IAsyncResult ar)
	{
		try
		{
			TcpCallbackPayload tcpCallbackPayload = (TcpCallbackPayload)ar.AsyncState;
			TcpClient client = tcpCallbackPayload.Client;
			client.EndConnect(ar);
			(tcpCallbackPayload.SslStream = new SslStream(client.GetStream())).BeginAuthenticateAsClient(Config.HostName, OnAuthenticated, tcpCallbackPayload);
		}
		catch (Exception ex)
		{
			if (this.Log != null)
			{
				this.Log("ANALYTICS: TCP Connection Failure! " + ex.ToString());
			}
		}
	}

	private void OnAuthenticated(IAsyncResult ar)
	{
		TcpCallbackPayload tcpCallbackPayload = (TcpCallbackPayload)ar.AsyncState;
		SslStream sslStream = tcpCallbackPayload.SslStream;
		try
		{
			sslStream.EndAuthenticateAsClient(ar);
			byte[] array = MessagePackSerializer.Get<FluentdMessage>().PackSingleObject(tcpCallbackPayload.Message);
			sslStream.Write(array, 0, array.Length);
			sslStream.Flush();
			tcpCallbackPayload.Clear();
		}
		catch (Exception ex)
		{
			Shutdown();
			if (this.Log != null)
			{
				this.Log("ANALYTICS: Authentication Failure! " + ex.ToString());
			}
		}
	}

	private int GetPoolIndex()
	{
		if (messagePool == null)
		{
			SetupMessagePool();
		}
		if (messagePool[msgPoolIndex].IsFull)
		{
			for (int i = 0; i < messagePool.Length; i++)
			{
				if (!messagePool[i].IsFull)
				{
					msgPoolIndex = i;
					return i;
				}
			}
			return -1;
		}
		return msgPoolIndex;
	}

	public void Shutdown()
	{
		FlushMessages();
	}

	public void FlushMessages()
	{
		for (int i = 0; i < messagePool.Length; i++)
		{
			if (!messagePool[i].IsEmpty)
			{
				SendMessage(payloadPool[i], messagePool[i]);
			}
		}
	}

	public void SendMessage(TeleMsg message)
	{
		int poolIndex = GetPoolIndex();
		if (poolIndex >= 0)
		{
			messagePool[poolIndex].AddEntry(message, (int)(DateTime.UtcNow - epochStart).TotalSeconds);
			if (messagePool[poolIndex].IsFull)
			{
				SendMessage(payloadPool[poolIndex], messagePool[poolIndex]);
			}
		}
	}
}
