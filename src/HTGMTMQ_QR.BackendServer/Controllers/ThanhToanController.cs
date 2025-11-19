using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.ViewModels.Systems.Chitiethoadon;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using HTGMTMQ_QR.ViewModels.Systems.ThanhToan;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThanhToanController : ControllerBase
    {
        private readonly ApplicationDbcontext _context;
        public ThanhToanController(ApplicationDbcontext context)
        {
            _context = context;
        }
        // GET: danh sách thanh toán
        [HttpGet]
        public async Task<IActionResult> GetAllThanhToan(string? filter = null, int pageIndex = 1, int pageSize = 20)
        {
            var query = _context.ThanhToans.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(tt => tt.HinhThuc.Contains(filter));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(tt => tt.MaTT)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(tt => new ThanhToanViewModels
                {
                    MaTT = tt.MaTT,
                    MaHD = tt.MaHD,
                    HinhThuc = tt.HinhThuc,
                    SoTien = tt.SoTien,
                    NgayTT = tt.NgayTT
                })
                .ToListAsync();

            return Ok(new Pagination<ThanhToanViewModels>
            {
                Items = items,
                TotalRecords = total,
                PageIndex = pageIndex,
                PageSize = pageSize
            });
        }

        // GET theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ThanhToanViewModels>> GetThanhToanById(int id)
        {
            var tt = await _context.ThanhToans.FindAsync(id);

            if (tt == null)
                return NotFound();

            return Ok(new ThanhToanViewModels
            {
                MaTT = tt.MaTT,
                MaHD = tt.MaHD,
                HinhThuc = tt.HinhThuc,
                SoTien = tt.SoTien,
                NgayTT = tt.NgayTT
            });
        }

        //POST: tạo thanh toán
        [HttpPost]

        public async Task<IActionResult> PostThanhToan(ThanhToanCreateVm model)
        {
            var hd = await _context.HoaDons.FindAsync(model.MaHD);

            if (hd == null)
            {
                return NotFound("Không tìm thấy hóa đơn.");
            }
            if (hd.TrangThai == "Đã thanh toán")
            {
                return NotFound("Hóa đơn này đã thanh toán rồi.");
            }

            var tt = new ThanhToan
            {
                MaHD = model.MaHD,
                HinhThuc = model.HinhThuc,
                SoTien = model.SoTien,
                NgayTT = DateTime.Now,
            };

            _context.ThanhToans.Add(tt);
            //Update trang thái hóa đơn
            hd.TrangThai = "Đã thanh toán";

            //Update Trạng thái bàn
            var ban = await _context.Bans.FindAsync(hd.MaBan);
            if (ban != null)
                ban.TrangThai = "Trống";

            var result = await _context.SaveChangesAsync();

            if (result > 0)
                return CreatedAtAction(nameof(GetThanhToanById), new { id = tt.MaTT }, model);
            return BadRequest("Không thể tạo thanh toán.");
        }
        //Put: cập nhật thanh toán
        [HttpPut("{id}")]
        public async Task<ActionResult<ThanhToanViewModels>> PutThanhToan(int id, ThanhToanUpdateVm model)
        {
            if (id != model.MaTT)
                return BadRequest("ID không khớp.");

            var tt = await _context.ThanhToans.FindAsync(id);
            if (tt != null)
            {
                return NotFound("Không tìm thấy thông tin thanh toán.");
            }

            tt.HinhThuc = model.HinhThuc;
            tt.SoTien = model.SoTien;
            tt.NgayTT = DateTime.Now;
            
            _context.Entry(tt).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new {message = " Cập nhật thanh toán thành công."});
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteThanhToan(int id)
        {
            var tt = await _context.ThanhToans.FindAsync(id);
            if (tt == null)
                return NotFound("Không tìm thấy thông tin thanh toán.");
            // Lấy hóa đơn liên quan
            var hd = await _context.HoaDons.FindAsync(tt.MaHD);
            if (hd != null)
            {
                hd.TrangThai = "Chưa thanh toán";

                var ban = await _context.Bans.FindAsync(hd.MaBan);
                if (ban != null)
                    ban.TrangThai = "Đang phục vụ";
            }

            _context.ThanhToans.Remove(tt);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa thanh toán và khôi phục trạng thái hóa đơn/bàn." });
        }


    }
}
