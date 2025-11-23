using HTGMTMQ_QR.BackendServer.Data.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HTGMTMQ_QR.BackendServer.Data.Entities
{
    [Table("SanPham")]
    public class SanPham : IDateTracking
    {
        [Key]
        public int MaSP { get; set; }

        [Required, StringLength(100)]
        public string TenSP { get; set; }

        [Required]
        public decimal DonGia { get; set; }

        [StringLength(50)]
        public string LoaiSP { get; set; }

        [Required, StringLength(20)]
        public string TrangThai { get; set; } = "Đang bán";

        public ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime? LastModifiedDate { get; set; }
    }
}
