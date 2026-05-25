using Anviz.SDK;
using Anviz.SDK.Responses;
using AnvizWebSDK.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace AnvizWebSDK.Controllers
{
    [ApiController]
    [Route("devices")]
    public class DevicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DevicesController(AppDbContext context)
        {
            _context = context;
        }

        public class ConnectDevicesRequest
        {
            public List<int> Id { get; set; } = new();
            public List<string> Ips { get; set; } = new();
        }

        public List<AnvizDevice> _devices = new List<AnvizDevice>();

        private async Task<AnvizDevice> ConnectToDevice(string ip)
        {
            AnvizManager manager = new AnvizManager();
            return await manager.Connect(ip);
        }

        // GET /devices
        [HttpGet]
        public async Task<IActionResult> GetDevices()
        {
            var devices = _context.Devices.ToList();
            for (int i = 0; i < devices.Count; i++)
            {
                var device = ConnectToDevice(devices[i].IpAddress).Result;
                if (device != null)
                {
                    _devices.Add(device);
                }
            }

            return Ok(_devices);
        }

        // GET /devices/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var devices = await _context.Devices.ToListAsync();

            var tasks = devices.Select(async d =>
            {
                var device = await ConnectToDevice(d.IpAddress);

                if (device == null)
                    return null;

                var stats = await device.GetDownloadInformation();

                return new DeviceWithStats
                {
                    Id = d.Id,
                    IpAddress = d.IpAddress,
                    DeviceBiometricType = device.DeviceBiometricType,
                    UserAmount = stats.UserAmount,
                    FingerPrintAmount = stats.FingerPrintAmount,
                    PasswordAmount = stats.PasswordAmount,
                    CardAmount = stats.CardAmount,
                    AllRecordAmount = stats.AllRecordAmount,
                    NewRecordAmount = stats.NewRecordAmount
                };
            });

            var result = await Task.WhenAll(tasks);

            return Ok(result.Where(x => x != null));
        }

        // GET /devices/{id}/users
        [HttpGet("{id}/users")]
        public async Task<IActionResult> GetUsers(int id)
        {
            AnvizDevice _device = null;

            var devices = _context.Devices.ToList();
            for (int i = 0; i < devices.Count; i++)
            {
                var device = ConnectToDevice(devices[i].IpAddress).Result;
                if (device != null)
                {
                    if (devices[i].Id == id)
                    {
                        _device = device;
                        break;
                    }
                }
            }

            List<UserInfo> users = await _device.GetEmployeesData();
            return Ok(users);
        }

        // GET /devices/{id}/records
        [HttpGet("{id}/records")]
        public async Task<IActionResult> GetRecords(int id)
        {
            AnvizDevice _device = null;

            var devices = _context.Devices.ToList();
            for (int i = 0; i < devices.Count; i++)
            {
                var device = ConnectToDevice(devices[i].IpAddress).Result;
                if (device != null)
                {
                    if (devices[i].Id == id)
                    {
                        _device = device;
                        break;
                    }
                }
            }

            List<Record> records = await _device.DownloadRecords();
            return Ok(records);
        }

        // GET /devices/{id}/statistics
        [HttpGet("{id}/statistics")]
        public async Task<IActionResult> GetStatistics(int id)
        {
            var devices = _context.Devices.ToList();
            for (int i = 0; i < devices.Count; i++)
            {
                var device = ConnectToDevice(devices[i].IpAddress).Result;
                if (device != null)
                {
                    if (devices[i].Id == id)
                    {
                        Statistic statistics = await device.GetDownloadInformation();
                        return Ok(statistics);
                    }
                }
            }
            
            return NotFound();
        }

        // GET /devices/{id}/users/{userId}
        [HttpGet("{id}/users/{userId}")]
        public async Task<IActionResult> GetUser(int id, int userId)
        {
            AnvizDevice _device = null;

            var devices = _context.Devices.ToList();
            for (int i = 0; i < devices.Count; i++)
            {
                var device = ConnectToDevice(devices[i].IpAddress).Result;
                if (device != null)
                {
                    if (devices[i].Id == id)
                    {
                        _device = device;
                        break;
                    }
                }
            }

            List<UserInfo> users = await _device.GetEmployeesData();
            UserInfo _user = null;

            foreach (UserInfo user in users)
            {
                if(user.Id == (ulong)userId)
                {
                    _user = user;
                    break;
                }
            }

            if (_user == null) return NotFound();
            return Ok(_user);
        }
    }
}