using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTGMTMQ_QR.ViewModels.Systems.SanPham
{
    public class SanPhamCreateVm
    {
        public string TenSP { get; set; }
        public decimal DonGia { get; set; }
        public string? LoaiSP { get; set; }
        public string ThanhPhan { get; set; }

        public string MoTa { get; set; }

        public string HinhAnh { get; set; } = string.Empty;
    }
}
