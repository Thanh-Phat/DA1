using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HTGMTMQ_QR.BackendServer.Data.Entities
{
    public class CaLamViec
    {
        [Key]
        public int MaCa { get; set; }
        public int MaND { get; set; }  // Thu ngân
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int TongSoHoaDon { get; set; }

        // Quan hệ với bảng Người Dùng
        public NguoiDung NguoiDung { get; set; }
    }

}
