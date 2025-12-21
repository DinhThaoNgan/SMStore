namespace CuaHangBanSach.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Age { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public List<string> AllRoles { get; set; } = new();
    }
}