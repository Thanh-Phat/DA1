using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTGMTMQ_QR.ViewModels.Systems.NguoiDung
{
    public class DoiMatKhauVm
    {
        public int MaND { get; set; }
        public string MatKhauCu { get; set; } = string.Empty;
        public string MatKhauMoi { get; set; } = string.Empty ;

        public string XacNhanMatKhauMoi { get; set ; } = string.Empty ;
    }
}
