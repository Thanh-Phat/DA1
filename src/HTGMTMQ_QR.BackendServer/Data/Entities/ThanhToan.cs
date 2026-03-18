using HTGMTMQ_QR.BackendServer.Data.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HTGMTMQ_QR.BackendServer.Data.Entities
{
    [Table("ThanhToan")]
    public class ThanhToan : IDateTracking
    {
        [Key]
        public int MaTT { get; set; }

        [ForeignKey(nameof(MaHD))]
        public int MaHD { get; set; }

        [Required, StringLength(50)]
        public string HinhThuc { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SoTien { get; set; }

        public DateTime? NgayTT { get; set; }

        public HoaDon HoaDon { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedDate { get; set; }
    }
}