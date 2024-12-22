namespace Varadhi.Services;

using SocketIOClient;
public class SocketIoService
{
	private readonly SocketIO _socket;

	public SocketIoService()
	{
		// Replace with your Socket.IO server URL
		_socket = new SocketIO("http://localhost:8009");
		_socket.ConnectAsync().Wait();
	}

	public async Task EmitEventAsync(string eventName, object data)
	{
		if (_socket.Connected)
		{
			await _socket.EmitAsync(eventName, data);
		}
	}
}
