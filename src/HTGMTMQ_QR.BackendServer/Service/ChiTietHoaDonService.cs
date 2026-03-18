using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.ViewModels.Systems.Chitiethoadon;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HTGMTMQ_QR.BackendServer.Service;
namespace HTGMTMQ_QR.BackendServer.Service
{
    public class ChiTietHoaDonService
    {
        private readonly ApplicationDbcontext _context;

        public   ChiTietHoaDonService(ApplicationDbcontext context)
        {
            _context = context;
        }
        // URL GET: http://localhost:5001/api/chitiethoadon/?filter={searchKeyword}&pageIndex=1&pageSize=20
        // GET ALL CTHD theo MaHD
        public async Task<Pagination<ChiTietHoaDonViewModels>>GetAllChiTietHoaDon(string? filter , int pageIndex, int pageSize)
        {
            var query = _context.ChiTietHoaDons
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(cthd => cthd.TrangThaiMon.Contains(filter));
            }

            var TotalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(cthd => cthd.MaCTHD)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(cthd => new ChiTietHoaDonViewModels
                {
                    MaCTHD = cthd.MaCTHD,
                    MaHD = cthd.MaHD,
                    MaSP = cthd.MaSP,
                    SoLuong = cthd.SoLuong,
                    DonGia = cthd.DonGia,
                    ThanhTien = cthd.ThanhTien,
                    TrangThaiMon = cthd.TrangThaiMon
                })
                .ToListAsync();

            return new Pagination<ChiTietHoaDonViewModels>
            {
                Items = items,
                TotalRecords = TotalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
}

        // url: http://localhost:5001/api/chitiethoadon/{id}/xem-chi-tiet
        //xem chi tiết
        public async Task<ChiTietHoaDonViewModels?> GetCTHDById(int id)
        {
            var cthd = await _context.ChiTietHoaDons.FindAsync(id);

            if (cthd == null)
            {
                return null;
            }
            var model = new ChiTietHoaDonViewModels
            {
                MaCTHD = cthd.MaCTHD,
                MaHD = cthd.MaHD,
                MaSP = cthd.MaSP,
                SoLuong = cthd.SoLuong,
                DonGia = cthd.DonGia,
                ThanhTien = cthd.ThanhTien,
                TrangThaiMon = cthd.TrangThaiMon
            };
            return model;   
        }

        //url: http://localhost:5001/api/chitiethoadon/them-mon
        // POST: Thêm món vào hóa đơn
        // KHÁCH GỌI MÓN (không cần đăng nhập)
        public async Task<(bool, string)> PostCTHD(ChiTietHoaDonCreateVm model)
        {

            var hd = await _context.HoaDons.FindAsync(model.MaHD);

            if (hd == null)
            {
                return (false, "Hóa đơn không tồn tại.");
            }

            var sp = await _context.SanPhams.FindAsync(model.MaSP);
            if (sp == null || sp.TrangThai == "Hết hàng")
            {
                return (false, "Sản phẩm không tồn tại hoặc đã hết hàng.");
            }

            // Kiểm tra nếu đã có món này trong hóa đơn thì cộng dồn số lượng, ngược lại tạo mới
            var item = await _context.ChiTietHoaDons
                .FirstOrDefaultAsync(x => x.MaHD == model.MaHD && x.MaSP == model.MaSP);
            if (item != null)
            {
                item.SoLuong += model.SoLuong;
                item.ThanhTien = item.SoLuong * item.DonGia;
            }
            else
            {
                item = new ChiTietHoaDon
                {
                    MaHD = model.MaHD,
                    MaSP = model.MaSP,
                    SoLuong = model.SoLuong,
                    DonGia = sp.DonGia,
                    ThanhTien = model.SoLuong * sp.DonGia,
                    TrangThaiMon = "Đang nấu"
                };
                _context.ChiTietHoaDons.Add(item);
            }
            var result = await _context.SaveChangesAsync();
            await HoaDonService.CapNhatTongTien(_context, model.MaHD);
            await _context.SaveChangesAsync();
            return (true, "Thêm món vào hóa đơn thành công.");
        }
        // url: http://localhost:5001/api/chitiethoadon/{id}/capnhat-trangthai
        // PUT: Cập nhật trạng thái món
        // BẾP cập nhật trạng thái món
        public async Task<(bool, string)> PutCTHD(int id, ChiTietHoaDonUpdataVm model)
        {
            if (id != model.MaCTHD)
                return (false, "ID không khớp.");

            var cthd = await _context.ChiTietHoaDons.FindAsync(id);
            if (cthd == null)
            {
                return (false, "Chi tiết hóa đơn không tồn tại.");
            }
            cthd.SoLuong = model.SoLuong;
            cthd.ThanhTien = model.SoLuong * cthd.DonGia;
            cthd.TrangThaiMon = model.TrangThaiMon;

            _context.Entry(cthd).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await HoaDonService.CapNhatTongTien(_context, cthd.MaHD);
            return (true, "Cập nhật trạng thái món thành công.");
        }


        //url:http://localhost:5001/api/chitiethoadon/{id}/Xoa-mon
        // DELETE: Xóa món
        [Authorize(Roles = "QuanLy")]
        [HttpDelete(("{id}/Xoa-mon"))]
        public async Task<(bool,string)> DeleteCTHD(int id)
        {
            var cthd = await _context.ChiTietHoaDons.FindAsync(id);
            if (cthd == null)
                return (false, "Chi tiết hóa đơn không tồn tại.");

            if (cthd.TrangThaiMon != "Đang nấu")
            {
                return (false, "Chỉ có thể xóa món đang ở trạng thái 'Đang nấu'.");
            }
            int maHD = cthd.MaHD;

            _context.ChiTietHoaDons.Remove(cthd);
            await _context.SaveChangesAsync();
            await HoaDonService.CapNhatTongTien(_context, maHD);
            return (true, "Xóa món thành công.");
        }
    }
}
}
