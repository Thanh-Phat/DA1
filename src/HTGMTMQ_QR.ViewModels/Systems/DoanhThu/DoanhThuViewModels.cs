using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTGMTMQ_QR.ViewModels.Systems.DoanhThu
{
    public class DoanhThuViewModels
    {
        public decimal TongDoanhThu { get; set; }
        public int SoHoaDon { get; set; }
        public List<ChiTietHoaDonItem> ChiTiet { get; set; }
    }

    public class ChiTietHoaDonItem
    {
        public int MaHD { get; set; }
        public DateTime? NgayTT { get; set; }
        public decimal SoTien { get; set; }
        public string HinhThuc { get; set; }
    }
}
