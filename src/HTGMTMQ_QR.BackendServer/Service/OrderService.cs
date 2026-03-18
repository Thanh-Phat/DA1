using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.ViewModels.Systems.SanPham;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Service
{
    public class OrderService
    {
        private readonly ApplicationDbcontext _context;

        public OrderService(ApplicationDbcontext context)
        {
            _context = context;
        }

        public async Task<bool> AddItem(AddItemVm model)
        {
            var hoadon = await _context.HoaDons.FindAsync(model.MaHD);
            if (hoadon == null)
                return false;

            var sanpham = await _context.SanPhams.FindAsync(model.MaSP);
            if (sanpham == null)
                return false;

            // kiểm tra món đã tồn tại chưa
            var item = await _context.ChiTietHoaDons
                .FirstOrDefaultAsync(x => x.MaHD == model.MaHD && x.MaSP == model.MaSP);

            if (item != null)
            {
                item.SoLuong += model.SoLuong;
            }
            else
            {
                item = new ChiTietHoaDon
                {
                    MaHD = model.MaHD,
                    MaSP = model.MaSP,
                    SoLuong = model.SoLuong,
                    DonGia = sanpham.DonGia,
                    TrangThaiMon = "Đang nấu"
                };

                _context.ChiTietHoaDons.Add(item);
            }

            await _context.SaveChangesAsync();

            // cập nhật tổng tiền
            hoadon.TongTien = await _context.ChiTietHoaDons
                .Where(x => x.MaHD == model.MaHD)
                .SumAsync(x => x.SoLuong * x.DonGia);

            await _context.SaveChangesAsync();

            return true;
        }

        // GET: api/order/{mahd}
        public async Task<List<object>> GetOrder(int mahd)
        {
            return await _context.ChiTietHoaDons
                .Where(x => x.MaHD == mahd)
                .Select(x => new
                {
                    x.MaCTHD,
                    x.MaSP,
                    x.SoLuong,
                    x.DonGia,
                    x.TrangThaiMon
                })
                .ToListAsync<object>();
        }
    }
}