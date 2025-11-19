using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using HTGMTMQ_QR.ViewModels.Systems.HoaDon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    public class HoaDonController : Controller
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
                .Skip((pageIndex - 1) * pageSize)
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

            if (hd ==null)
                    return NotFound();
            var model = new HoaDonViewModels
            {
                MaHD =  hd.MaND,
                MaBan = hd.MaBan,
                MaND = hd.MaND,
                NgayTao= hd.NgayTao,
                TongTien = hd.TongTien,
                TrangThai = hd.TrangThai,
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
                TongTien= 0,
                TrangThai = "Chưa Thanh Toán"
            };

            _context.HoaDons.Add(hd);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return CreatedAtAction(nameof(GetHDById), new { id = hd.MaHD }, model);
            }    
            return BadRequest("Không thể tạo hóa đơn.");
        }
    }
}
