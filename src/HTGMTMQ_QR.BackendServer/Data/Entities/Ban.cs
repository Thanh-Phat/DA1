using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HTGMTMQ_QR.BackendServer.Data.Entities
{
    public class Ban
    {
        [Table("Ban")]
        public class Ban
        {
            [Key]
            public int MaBan { get; set; }

            [Required]
            public int SoBan { get; set; }

            [Required, StringLength(20)]
            public string TrangThai { get; set; } = "Trống";

            public QRCode QRCode { get; set; }

            public ICollection<HoaDon> HoaDons { get; set; }
        }
    }
}
