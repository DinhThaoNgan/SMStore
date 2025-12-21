namespace CuaHangBanSach.Services
{
    public class EmailSettings
    {
        public string Email { get; set; }      // ✅ tên khớp với config
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}