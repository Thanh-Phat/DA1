using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HTGMTMQ_QR.BackendServer.Data.Entities
{
    [Table("QRCode")]
    public class QRCode
    {
        [Key]
        public int MaQR { get; set; }

        [ForeignKey("Ban")]
        public int MaBan { get; set; }

        [Required, StringLength(255)]
        public string DuongDanQR { get; set; }

        public DateTime NgayTao { get; set; }

        public Ban Ban { get; set; }
    }
}