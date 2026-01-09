using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTGMTMQ_QR.ViewModels.Systems.Chitiethoadon
{
    public class ChiTietHoaDonUpdataVm
    {
        public int MaCTHD { get; set; }
        public int MaHD { get; set; }
        public int SoLuong { get; set; }
        public string TrangThaiMon { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime? LastModifiedDate { get; set; }
    }
}
