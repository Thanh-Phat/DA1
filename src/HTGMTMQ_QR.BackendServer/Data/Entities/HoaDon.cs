using HTGMTMQ_QR.BackendServer.Data.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HTGMTMQ_QR.BackendServer.Data.Entities
{
    [Table("HoaDon")]
    public class HoaDon : IDateTracking
    {
        [Key]
        public int MaHD { get; set; }

        [ForeignKey("Ban")]
        public int MaBan { get; set; }

        [ForeignKey("NguoiDung")]
        public int MaND { get; set; }

        public DateTime NgayTao { get; set; }

        public decimal TongTien { get; set; }

        [Required, StringLength(20)]
        public string TrangThai { get; set; } = "Chưa thanh toán";

        public Ban Ban { get; set; }
        public NguoiDung NguoiDung { get; set; }
        public ICollection<ThanhToan> ThanhToans { get; set; }

        public ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public DateTime CreateDate { get ; set ; } = DateTime.Now;
        public DateTime? LastModifiedDate { get ; set ; }
    }
}

