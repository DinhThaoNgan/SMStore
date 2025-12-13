using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CuaHangBanSach.Models;
using CuaHangBanSach.Repository;
using CuaHangBanSach.ViewModels;

namespace CuaHangBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CouponController : Controller
    {
        private readonly ICouponRepository _couponRepository;

        public CouponController(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 10;
            var model = await _couponRepository.GetPagedCouponsAsync(search, page, pageSize);
            return View(model);
        }

        public async Task<IActionResult> Display(int id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
                return NotFound();

            return View(coupon);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (coupon.EndDate <= coupon.StartDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu.");
            }

            var existing = await _couponRepository.GetByCodeAsync(coupon.Code.Trim());
            if (existing != null)
            {
                ModelState.AddModelError("Code", "Mã giảm giá đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                await _couponRepository.AddAsync(coupon);
                TempData["Success"] = "Tạo mã giảm giá thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(coupon);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
                return NotFound();

            return View(coupon);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Coupon updatedCoupon)
        {
            if (id != updatedCoupon.Id)
                return NotFound();

            if (updatedCoupon.EndDate <= updatedCoupon.StartDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu.");
            }

            var existing = await _couponRepository.GetByCodeAsync(updatedCoupon.Code.Trim());
            if (existing != null && existing.Id != id)
            {
                ModelState.AddModelError("Code", "Mã giảm giá đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                var couponInDb = await _couponRepository.GetByIdAsync(id);
                if (couponInDb == null) return NotFound();

                // Cập nhật thủ công để tránh lỗi tracking
                couponInDb.Code = updatedCoupon.Code;
                couponInDb.StartDate = updatedCoupon.StartDate;
                couponInDb.EndDate = updatedCoupon.EndDate;
                couponInDb.DiscountAmount = updatedCoupon.DiscountAmount;
                couponInDb.IsPercentage = updatedCoupon.IsPercentage;

                await _couponRepository.UpdateAsync(couponInDb);
                TempData["Success"] = "Cập nhật mã giảm giá thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(updatedCoupon);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
                return NotFound();

            return View(coupon);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _couponRepository.DeleteAsync(id);
            TempData["Success"] = "Xoá mã giảm giá thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ValidateCoupon(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Json(new { success = false });

            var coupon = await _couponRepository.GetByCodeAsync(code.Trim());
            if (coupon == null || !coupon.IsActive)
                return Json(new { success = false });

            var now = DateTime.Now;
            if (coupon.StartDate > now || coupon.EndDate < now)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                isPercentage = coupon.IsPercentage,
                discount = coupon.DiscountAmount
            });
        }
    }
}