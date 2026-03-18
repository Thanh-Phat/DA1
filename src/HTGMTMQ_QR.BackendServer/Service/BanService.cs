using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.ViewModels.Systems.Ban;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QRCode = HTGMTMQ_QR.BackendServer.Data.Entities.QRCode;

namespace HTGMTMQ_QR.BackendServer.Service
{
    public class BanService
    {
        private readonly ApplicationDbcontext _context;
        public BanService(ApplicationDbcontext context)
        {
            _context = context;
        }
        public async Task<Pagination<BanViewModels>> GetAllBan(string? filter = null, int pageIndex = 1, int pageSize = 10)
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
            return new Pagination<BanViewModels>
            {
                Items = items,
                TotalRecords = TotalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<BanViewModels?> GetBanbyId(int id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null)
            {
                return null;
            }
            return new BanViewModels
            {
                MaBan = ban.MaBan,
                SoBan = ban.SoBan,
                TrangThai = ban.TrangThai,
            };
        }
        public async Task<object?> GetQRTheoBan(int id)
        {
            var qr = await _context.QRCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaBan == id);

            if (qr == null)
            {
                return null;
            }
            return new
            {
                qr.MaQR,
                qr.MaBan,
                qr.NgayTao,
                qr.DuongDanQR
            };
        }

        public async Task<bool> PostBan(BanCreateVm model)
        {
            // Kiểm tra số bàn trùng
            if (await _context.Bans.AnyAsync(x => x.SoBan == model.SoBan))
                return false;

            var ban = new Ban
            {
                SoBan = model.SoBan,
                TrangThai = "Trống"
            };

            _context.Bans.Add(ban);
            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<object?> PostQRCode(int id, string host)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null)
            {
                return null;
                // URL khách sẽ truy cập 
            }
            string url = $"{host}/api/qr/{id}";

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

            await File.WriteAllBytesAsync(filePath, qrBytes);

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

            return new
            {
                message = "Tạo / cập nhật QR thành công.",
                url = url,
                qrImage = fileUrl, // ảnh thật để in
                previewBase64 = $"data:image/png;base64,{base64Preview}"
            };
        }

        public async Task<bool> PutBan(int id, BanUpdateVm model)
        {
            // Kiểm tra ID
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null)
                return false;
            //Không cho đổi trạng thái nếu bàn đang phục vụ
            bool hasUnpaidHoadon = await _context.HoaDons
                .AnyAsync(h => h.MaBan == id && h.TrangThai == "Chưa thanh toán");

            if (hasUnpaidHoadon && model.TrangThai != "Đang phục vụ")
            {
                return false;
            }
            // Kiểm tra số bàn trùng
            if (await _context.Bans.AnyAsync(x => x.SoBan == model.SoBan && x.MaBan != id))
                return false;
            {
                ban.SoBan = model.SoBan;
                ban.TrangThai = model.TrangThai;
            }

            _context.Entry(ban).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteBan(int id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null)
            {
                return false;
            }
            // Không xóa nếu còn hóa đơn chưa thanh toán
            bool hasUnpaid = await _context.HoaDons.AnyAsync(h => h.MaBan == id && h.TrangThai == "Chưa thanh toán");
            if (hasUnpaid)
            {
                return false;
            }
            _context.Bans.Remove(ban);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

