using Anviz.SDK;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace AnvizWebSDK.Models
{
    public class DeviceRegistry : IHostedService
    {
        private readonly ConcurrentDictionary<string, AnvizDevice> _devices = new();
        private UdpClient? _listener;

        public IReadOnlyDictionary<string, AnvizDevice> Devices => _devices;

        public async Task StartAsync(CancellationToken ct)
        {
            _listener = new UdpClient(5010); // Anviz default UDP port
            _ = Task.Run(() => ListenForPings(ct), ct);
        }

        private async Task ListenForPings(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var result = await _listener!.ReceiveAsync(ct);
                var ip = result.RemoteEndPoint.Address.ToString();

                if (!_devices.ContainsKey(ip))
                {
                    try
                    {
                        var manager = new AnvizManager();
                        var device = await manager.Connect(ip);
                        _devices[ip] = device;
                    }
                    catch { /* device unreachable, skip */ }
                }
            }
        }

        public Task StopAsync(CancellationToken ct)
        {
            _listener?.Close();
            return Task.CompletedTask;
        }
    }
}
