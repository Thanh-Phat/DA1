using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.ViewModels.Systems.Ban;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Threading.Tasks;
using QRCode = HTGMTMQ_QR.BackendServer.Data.Entities.QRCode;


namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BanController : ControllerBase
    {
        private readonly ApplicationDbcontext _context;
        public BanController(ApplicationDbcontext context)
        {
            _context = context;
        }

        // URL GET: http://localhost:5001/api/ban/?filter={searchKeyword}&pageIndex=1&pageSize=10
        // Lấy danh sách (có filter + paging)
        [Authorize(Roles = "QuanLy,ThuNgan")]
        [HttpGet]
        public async Task<IActionResult> GetAllBan(string? filter = null, int pageIndex = 1, int pageSize = 10)
        {
            var query = _context.Bans.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(b => b.TrangThai.Contains(filter));
            }

            var TotalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(b => b.MaBan)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BanViewModels
                {
                    MaBan = b.MaBan,
                    SoBan = b.SoBan,
                    TrangThai = b.TrangThai,
                })
                .ToListAsync();
            var pagination = new Pagination<BanViewModels>
            {
                Items = items,
                TotalRecords = TotalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return Ok(pagination);
        }

        //URL Get: http://locahost:5001/api/ban/{id}
        //Lấy người dùng theo ID
        [Authorize(Roles = "QuanLy,ThuNgan")]
        [HttpGet("{id}")]
        public async Task<ActionResult<BanViewModels>> GetBanbyId(int id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null)
            {
                return NotFound();
            }
            var model = new BanViewModels
            {
                MaBan = ban.MaBan,
                SoBan = ban.SoBan,
                TrangThai = ban.TrangThai,
            };
            return Ok(model);
        }
        //URL Get: http://locahost:5001/api/ban/{id}/qr
        //Lấy ID QR theo bàn
        [Authorize(Roles = "QuanLy,ThuNgan")]
        [HttpGet("{id}/qr")]
        public async Task<IActionResult> GetQRTheoBan(int id)
        {
            var qr = await _context.QRCodes.FirstOrDefaultAsync(x => x.MaBan == id);

            if (qr == null)
            {
                return NotFound("Bàn này chưa có mã QR.");
            }
            return Ok(new
            {
                qr.MaQR,
                qr.MaBan,
                qr.NgayTao,
                qr.DuongDanQR
            });
        }


        //Url: http://locahost:7066/api/ban/{id}/them-ban
        //Thêm bàn 
        [Authorize(Roles = "QuanLy")]
        [HttpPost]
        public async Task<ActionResult<BanViewModels>> PostBan(BanCreateVm model)
        {
            // Kiểm tra số bàn trùng
            if (await _context.Bans.AnyAsync(x => x.SoBan == model.SoBan))
                return BadRequest("Số bàn này đã tồn tại.");

            var ban = new Ban
            {
                SoBan = model.SoBan,
                TrangThai = "Trống"
            };

            _context.Bans.Add(ban);
            var result = await _context.SaveChangesAsync();
            
            if (result > 0)
            {
                return CreatedAtAction(nameof(GetBanbyId), new { id = ban.MaBan }, model);
            }
            return BadRequest("Không thể thêm người dùng mới.");
        }
        //Url: http://locahost:7066/api/ban/{id}/tao-qr
        //Tạo mã QR cho bàn
        [Authorize(Roles = "QuanLy")]
        [HttpPost("{id}/tao-qr")]
        public async Task<IActionResult> PostQRCode(int id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null)
            {
                return NotFound("Không tìm thấy bàn.");
            }
            // URL khách sẽ truy cập 
            string url = $"{Request.Scheme}://{Request.Host}/Ban{id}";

            // Tạo mã QR
            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            byte[] qrBytes = qrCode.GetGraphic(20);
            // Lưu ảnh QR vào thư mục anhqr/qr
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "anhqr", "qr");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            string fileName = $"ban_{id}.png";
            string filePath = Path.Combine(folderPath, fileName);

            await System.IO.File.WriteAllBytesAsync(filePath, qrBytes);

            string fileUrl = "/qr/" + fileName; // Để lưu DB

            // ==== LƯU DB ====
            var qr = await _context.QRCodes.FirstOrDefaultAsync(x => x.MaBan == id);
            if (qr == null)
            {
                qr = new QRCode
                {
                    MaBan = id,
                    DuongDanQR = fileUrl,  
                    NgayTao = DateTime.Now
                };
                _context.QRCodes.Add(qr);
            }
            else
            {
                qr.DuongDanQR = fileUrl;
                qr.NgayTao = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            // Trả về base64 để xem trước nếu cần
            string base64Preview = Convert.ToBase64String(qrBytes);

            return Ok(new
            {
                message = "Tạo / cập nhật QR thành công.",
                url = url,
                qrImage = fileUrl, // ảnh thật để in
                previewBase64 = $"data:image/png;base64,{base64Preview}"
            });
        }

        //Url: http://locahost:7066/api/ban/{id}/cap-nhat-ban
        //Cập nhật thông tin bàn
        [Authorize(Roles = "QuanLy")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBan(int id,BanUpdateVm model)
        {
            // Kiểm tra ID
            if (id != model.MaBan)
            {
                return BadRequest("ID không khớp.");
            }    

            var ban = await _context.Bans.FindAsync(id);
            if (ban == null) 
                return NotFound();
            //Không cho đổi trạng thái nếu bàn đang phục vụ
            bool hasUnpaidHoadon= await _context.HoaDons.AnyAsync(h => h.MaBan == id && h.TrangThai == "Chưa thanh toán");
            if (hasUnpaidHoadon && model.TrangThai != "Đang phục vụ")
            {
                return BadRequest("Không thể thay đổi trạng thái bàn đang phục vụ.");
            }
            // Kiểm tra số bàn trùng
            if (await _context.Bans.AnyAsync(x => x.SoBan == model.SoBan && x.MaBan != id))
                return BadRequest("Số bàn này đã tồn tại.");
            {
                ban.SoBan = model.SoBan;
                ban.TrangThai = model.TrangThai;
            }

            _context.Entry(ban).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật bàn thành công." });
        }

        //Url: http://locahost:7066/api/ban/{id}/xoa-ban
        //Xóa bàn
        [Authorize(Roles = "QuanLy")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBan(int id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null)
            {
                return NotFound();
            }
            // Không xóa nếu còn hóa đơn chưa thanh toán
            bool hasUnpaid = await _context.HoaDons.AnyAsync(h => h.MaBan == id && h.TrangThai == "Chưa thanh toán");
            if (hasUnpaid)
            {
                return BadRequest("Không thể xóa bàn này vì còn hóa đơn chưa thanh toán.");
            }
            _context.Bans.Remove(ban);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa bàn thành công." });
        }

    }
}

