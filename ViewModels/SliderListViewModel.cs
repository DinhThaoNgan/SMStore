using CuaHangBanSach.Models;

namespace CuaHangBanSach.ViewModels
{
    public class SliderListViewModel : Paginate<Slider>
    {
        public string? SearchTerm { get; set; }
    }
}