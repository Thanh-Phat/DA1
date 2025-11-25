using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using HTGMTMQ_QR.ViewModels.Systems.SanPham;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Authorize(Roles = "QuanLy")]
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        private readonly ApplicationDbcontext _context;
        public SanPhamController(ApplicationDbcontext context) 
        {
            _context = context;
        }

        // GET ALL + FILTER + PAGING
        [Authorize(Roles = "QuanLy,ThuNgan")]
        [HttpGet]
        public async Task<IActionResult> GetAllSanPham(string? filter = null, int pageIndex = 1, int pageSize = 10)
        {
            var query = _context.SanPhams.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(sp => sp.TenSP.Contains(filter) || sp.LoaiSP.Contains(filter));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(sp => sp.MaSP)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(sp => new SanPhamViewModels
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    DonGia = sp.DonGia,
                    LoaiSP = sp.LoaiSP,
                    TrangThai = sp.TrangThai
                })
                .ToListAsync();

            return Ok(new Pagination<SanPhamViewModels>
            {
                Items = items,
                TotalRecords = total,
                PageIndex = pageIndex,
                PageSize = pageSize
            });
        }

        // GET BY ID
        [Authorize(Roles = "QuanLy,ThuNgan")]
        [HttpGet("{id}")]
        public async Task<ActionResult<SanPhamViewModels>> GetSanPhamById(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null)
                return NotFound();

            return Ok(new SanPhamViewModels
            {
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,
                DonGia = sp.DonGia,
                LoaiSP = sp.LoaiSP,
                TrangThai = sp.TrangThai
            });
        }
        // POST - Tạo món
        [Authorize(Roles = "QuanLy")]
        [HttpPost]
        public async Task<ActionResult<SanPhamViewModels>> PostSanPham(SanPhamCreateVm model)
        {
            var sp = new SanPham
            {
                TenSP = model.TenSP,
                DonGia = model.DonGia,
                LoaiSP = model.LoaiSP,
                TrangThai = "Đang bán"
            };

            _context.SanPhams.Add(sp);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                return CreatedAtAction(nameof(GetSanPhamById), new { id = sp.MaSP }, model);

            return BadRequest("Không thể thêm sản phẩm.");
        }

        // PUT - Cập nhật món
        [Authorize(Roles = "QuanLy")]
        [HttpPut("{id}")]
        public async Task<ActionResult<SanPhamViewModels>> PutSanPham(int id, SanPhamUpdateVm model)
        {
            if (id != model.MaSP)
                return BadRequest("ID không khớp.");

            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null)
                return NotFound();

            sp.TenSP = model.TenSP;
            sp.DonGia = model.DonGia;
            sp.LoaiSP = model.LoaiSP;
            sp.TrangThai = model.TrangThai;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật sản phẩm thành công." });
        }

        // DELETE - Xóa món
        [Authorize(Roles = "QuanLy")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);

            if (sp == null)
                return NotFound();

            // Kiểm tra sản phẩm đã có trong hóa đơn chưa
            bool isUsed = await _context.ChiTietHoaDons.AnyAsync(c => c.MaSP == id);
            if (isUsed)
                return BadRequest("Không thể xóa món đã có trong hóa đơn.");

            _context.SanPhams.Remove(sp);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa sản phẩm thành công." });
        }

    }
}
