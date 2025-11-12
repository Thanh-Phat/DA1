using HTGMTMQ_QR.BackendServer.Data.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HTGMTMQ_QR.BackendServer.Data.Entities
{
    [Table("NguoiDung")]
    public class NguoiDung : IDateTracking
    {
        [Key]
        public int MaND { get; set; }

        [Required, StringLength(50)]
        public string TenDangNhap { get; set; }

        [Required, StringLength(128)]
        public string MatKhau { get; set; }

        [Required, StringLength(100)]
        public string HoTen { get; set; }

        [Required, StringLength(20)]
        public string VaiTro { get; set; }

        public bool TrangThai { get; set; } = true;

        public DateTime NgayCapNhatMK { get; set; }

        public ICollection<HoaDon> HoaDons { get; set; }
        public DateTime CreateDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime LastModifiedDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
