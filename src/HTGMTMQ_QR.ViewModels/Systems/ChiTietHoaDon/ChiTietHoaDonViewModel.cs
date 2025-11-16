using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTGMTMQ_QR.ViewModels.Systems.Chitiethoadon
{
    internal class ChiTietHoaDonViewModel
    {
        public int MaCTHD { get; set; }
        public int MaHD { get; set; }
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public string TrangThaiMon { get; set; }
    }
}
