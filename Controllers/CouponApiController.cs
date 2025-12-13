using Microsoft.AspNetCore.Mvc;
using CuaHangBanSach.Repository;

namespace CuaHangBanSach.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponApiController : ControllerBase
    {
        private readonly ICouponRepository _couponRepository;

        public CouponApiController(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        [HttpGet("Validate")]
        public async Task<IActionResult> Validate(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Ok(new { success = false });

            var coupon = await _couponRepository.GetByCodeAsync(code.Trim());
            if (coupon == null || !coupon.IsActive)
                return Ok(new { success = false });

            var now = DateTime.Now;
            if (coupon.StartDate > now || coupon.EndDate < now)
                return Ok(new { success = false });

            return Ok(new
            {
                success = true,
                isPercentage = coupon.IsPercentage,
                discount = coupon.DiscountAmount
            });
        }
    }
}