using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using HTGMTMQ_QR.ViewModels.Systems.HoaDon;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HoaDonController : ControllerBase
    {
        private readonly ApplicationDbcontext _context;

        public HoaDonController(ApplicationDbcontext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllHoaDon(string? filter = null, int pageIndex = 1, int pageSize = 2)
        {
            var query = _context.HoaDons.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(hd => hd.TrangThai.Contains(filter));
            }

            var TotalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(hd => hd.MaHD)
                .Skip((pageSize - 1) * pageSize)
                .Take(pageSize)
                .Select(hd => new HoaDonViewModels
                {
                    MaHD = hd.MaHD,
                    MaBan = hd.MaBan,
                    MaND = hd.MaND,
                    NgayTao = hd.NgayTao,
                    TongTien = hd.TongTien,
                    TrangThai = hd.TrangThai,
                })
                .ToListAsync();

            var pagination = new Pagination<HoaDonViewModels>
            {
                Items = items,
                TotalRecords = TotalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return Ok(pagination);
        }


        [HttpGet("{id}")]

        public async Task<ActionResult<HoaDonViewModels>> GetHDById(int id)
        {
            var hd = await _context.HoaDons.FindAsync(id);

            if (hd == null)
                return NotFound();
            var model = new HoaDonViewModels
            {
                MaHD = hd.MaHD,
                MaBan = hd.MaBan,
                MaND = hd.MaND,
                NgayTao = hd.NgayTao,
                TongTien = hd.TongTien,
                TrangThai = hd.TrangThai,
            };
            return Ok(model);
        }
        // GET: api/hoadon/TheoNgay?date=2025-01-01
        [HttpGet("TheoNgay")]

        public async Task<IActionResult> GetHDTheoNgay(DateTime date)
        {
            var query = await _context.HoaDons
            .Where(hd => hd.NgayTao.Date == date.Date)
            .Select(hd => new HoaDonViewModels
            {
                MaHD = hd.MaHD,
                MaBan = hd.MaBan,
                MaND = hd.MaND,
                NgayTao = hd.NgayTao,
                TongTien = hd.TongTien,
                TrangThai = hd.TrangThai
            })
            .ToListAsync();
            return Ok(query);
        }

        // GET: /api/hoadon/dangmo/{maban}
        [HttpGet("dangmo/{maban}")]
        public async Task<IActionResult> GetHoaDonDangMoTheoBan(int maban)
        {
            var hd = await _context.HoaDons
                .Where(h => h.MaBan == maban && h.TrangThai == "Chưa thanh toán")
                .OrderByDescending(h => h.NgayTao)
                .FirstOrDefaultAsync();

            if (hd == null)
                return NotFound("Bàn này không có hóa đơn đang mở.");

            var model = new HoaDonViewModels
            {
                MaHD = hd.MaHD,
                MaBan = hd.MaBan,
                MaND = hd.MaND,
                NgayTao = hd.NgayTao,
                TongTien = hd.TongTien,
                TrangThai = hd.TrangThai
            };

            return Ok(model);
        }

        //Post = Thêm hóa đơn

        [HttpPost]
        public async Task<ActionResult<HoaDonViewModels>> PostHD(HoaDonCreateVm model)
        {
            var hd = new HoaDon
            {
                MaBan = model.MaBan,
                MaND = model.MaND,
                NgayTao = DateTime.Now,
                TongTien = 0,
                TrangThai = "Chưa thanh toán"
            };

            _context.HoaDons.Add(hd);
            // Cập nhật trạng thái bàn

            var ban = await _context.Bans.FindAsync(model.MaBan);
            if (ban != null)
            {
                ban.TrangThai = "Đang phục vụ";
            }

            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return CreatedAtAction(nameof(GetHDById), new { id = hd.MaHD }, model);
            }
            return BadRequest("Không thể tạo hóa đơn.");
        }

        // PUT: api/hoadon/{id}
        // Cập nhật thông tin hóa đơn
        [HttpPut("{id}")]
        public async Task<ActionResult<HoaDonViewModels>> PutHD(int id, HoaDonUpdateVm model)
        {
            if (id != model.MaHD)
                return BadRequest("ID không khớp.");

            var hd = await _context.HoaDons.FindAsync(id);
            if (hd == null)
                return NotFound("Không tìm thấy hóa đơn.");

            // Nếu đổi bàn → cập nhật trạng thái 2 bàn
            if (hd.MaBan != model.MaBan)
            {
                // Bàn cũ → Trống
                var banCu = await _context.Bans.FindAsync(hd.MaBan);
                if (banCu != null)
                    banCu.TrangThai = "Trống";

                // Bàn mới → Đang phục vụ
                var banMoi = await _context.Bans.FindAsync(model.MaBan);
                if (banMoi != null)
                    banMoi.TrangThai = "Đang phục vụ";
            }

            // Cập nhật dữ liệu hóa đơn
            hd.MaBan = model.MaBan;
            hd.MaND = model.MaND;
            hd.TrangThai = model.TrangThai;
            hd.LastModifiedDate = DateTime.Now;

            // Không cho sửa tổng tiền — auto tính
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật hóa đơn thành công." });
        }


        //Delete: api/hoadon/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHD(int id)
        {
            var hoadon = await _context.HoaDons.FindAsync(id);
            if (hoadon == null)
                return NotFound("Không tìm thấy hóa đơn.");
            // Không cho xoá hóa đơn đã thanh toán
            if (hoadon.TrangThai == "Đã thanh toán")
                return BadRequest("Không thể xóa hóa đơn đã thanh toán.");
            //Xóa chi tiết hóa đơn liên quan
            var cthds = _context.ChiTietHoaDons.Where(c => c.MaHD == id);
            _context.ChiTietHoaDons.RemoveRange(cthds);
            //Đặt trạng thái bàn về Trống
            var ban = await _context.Bans.FindAsync(hoadon.MaBan);
            if (ban != null)
                ban.TrangThai = "Trống";
            //Xóa hóa đơn
            _context.HoaDons.Remove(hoadon);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa hóa đơn và giải phóng bàn." });
        }
    }
}
