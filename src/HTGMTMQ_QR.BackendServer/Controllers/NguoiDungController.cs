using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.BackendServer.Helpers;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using HTGMTMQ_QR.ViewModels.Systems.NguoiDung;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Authorize(Roles = "QuanLy")]
    [Route("api/[controller]")]
    [ApiController]
    public class NguoiDungController : ControllerBase
    {
        private readonly ApplicationDbcontext _context;
        public NguoiDungController(ApplicationDbcontext context)
        {
            _context = context;
        }
        // URL GET: http://localhost:5001/api/nguoidung/?filter={searchKeyword}&pageIndex=1&pageSize=10
        // Lấy danh sách (có filter + paging)
        [HttpGet]
        public async Task<IActionResult> GetAllNguoiDung(string? filter= null, int pageIndex= 1, int pageSize = 10)
        {
            var query = _context.NguoiDungs.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x =>
                    x.TenDangNhap.Contains(filter) ||
                    x.HoTen.Contains(filter) ||
                    x.VaiTro.Contains(filter));
            }
            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.MaND)
                .Skip((pageIndex -1) * pageSize)
                .Take(pageSize)
                .Select(u => new NguoiDungViewModels()
                { 
                    MaND=u.MaND,
                    TenDangNhap=u.TenDangNhap,
                    HoTen=u.HoTen,
                    VaiTro=u.VaiTro,
                    TrangThai=u.TrangThai
                })
                .ToListAsync();

            // Trả về object phân trang
            var pagination = new Pagination<NguoiDungViewModels>
            {
                Items = items,
                TotalRecords = totalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return Ok(pagination);
        }

        //URL Get: http://locahost:5001/api/nguoidung/{id}
        //Lấy người dùng theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<NguoiDungViewModels>> GetNguoiDungbyId(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null)
                return NotFound();
            var model = new NguoiDungViewModels
            {
                MaND = user.MaND,
                TenDangNhap = user.TenDangNhap,
                HoTen = user.HoTen,
                VaiTro = user.VaiTro,
                TrangThai = user.TrangThai,
            };
            return Ok(model);
        }
        //URL POST: http://locahost:5001/api/nguoidung
        //Thêm người dùng
        [HttpPost]
        public async Task<ActionResult<NguoiDungViewModels>> PostNguoiDung(NguoiDungCreateVm model)
        {
            var nd = new NguoiDung
            {
                TenDangNhap = model.TenDangNhap,
                MatKhau = PasswordHelper.HashPassword(model.MatKhau),
                HoTen = model.HoTen,
                VaiTro = model.VaiTro,
                TrangThai = true,
                NgayCapNhatMK = DateTime.Now,
            };
            _context.NguoiDungs.Add(nd);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return CreatedAtAction(nameof(GetNguoiDungbyId), new { id = nd.MaND }, model);
            }
            return BadRequest("Không thể thêm người dùng mới.");
        }

        //URL Put: http://locahost:5001/api/nguoidung/{id}
        // Cập nhật thông tin người dùng
        [HttpPut("{id}")]
        public async Task<ActionResult<NguoiDungViewModels>> PutNguoiDung(int id, NguoiDungUpdataVm model)
        {
            if (id != model.MaND)
            {
                return BadRequest("ID không khớp giữa URL và dữ liệu gửi lên.");
            };
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null)
                return NotFound();
            {
                // Cập nhật thông tin
                user.HoTen = model.HoTen;
                user.VaiTro = model.VaiTro;
                user.TrangThai = model.TrangThai;
                user.NgayCapNhatMK = DateTime.Now;
                user.LastModifiedDate = DateTime.Now;
            };
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thông tin người dùng thành công." });
        }
        //URL Delete http://locahost:5001/api/nguoidung/{id}
        //Xóa người dùng
        [HttpDelete("{id}")]
        public async Task<ActionResult<NguoiDungViewModels>> DeleteNguoiDung(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            };
            _context.NguoiDungs.Remove(user);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Đã xóa người dùng thành công." });
        }
        //URL Put http://locahost:5001/api/nguoidung/doimatkhau/{id}
        //Đổi mật khẩu
        [HttpPut("doimatkhau/{id}")]
        public async Task<ActionResult<NguoiDungViewModels>> DoiMatKhauNguoiDung(int id, DoiMatKhauVm model)
        {
            if(id != model.MaND)
            {
                return BadRequest("ID không khớp.");
            }; 

            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            };
            if (!PasswordHelper.VerifyPassword(model.MatKhauCu, user.MatKhau))
            {
                return BadRequest("Mật Khẩu cũ không chính xác");
            };

            user.MatKhau = PasswordHelper.HashPassword(model.MatKhauMoi);
            user.NgayCapNhatMK = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công." });
        }


    }
}
