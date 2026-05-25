using Anviz.SDK.Utils;

namespace AnvizWebSDK.Models
{
    public class DeviceWithStats
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public BiometricType DeviceBiometricType { get; set; }
        public ulong UserAmount { get; set; }
        public ulong FingerPrintAmount { get; set; }
        public ulong PasswordAmount { get; set; }
        public ulong CardAmount { get; set; }
        public ulong AllRecordAmount { get; set; }
        public ulong NewRecordAmount { get; set; }
    }
}
