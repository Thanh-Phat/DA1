using HTGMTMQ_QR.BackendServer.Data;
using Microsoft.AspNetCore.Mvc;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.ViewModels.Systems.Ban;
using Microsoft.EntityFrameworkCore;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using System.Threading.Tasks;


namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BanController : ControllerBase
    {
        private readonly ApplicationDbcontext _context;
        public BanController(ApplicationDbcontext context)
        {
            _context = context;
        }

        // URL GET: http://localhost:5001/api/ban/?filter={searchKeyword}&pageIndex=1&pageSize=10
        // Lấy danh sách (có filter + paging)

        [HttpGet]
        public async Task<IActionResult> GetAllBan(string? filter = null, int pageIndex = 1, int pageSize = 10)
        {
            var query = _context.Bans.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(b => b.TrangThai.Contains(filter));
            }

            var TotalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(b => b.MaBan)
                .Skip((pageSize - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BanViewModels
                {
                    MaBan = b.MaBan,
                    SoBan = b.SoBan,
                    TrangThai = b.TrangThai,
                })
                .ToListAsync();
            var pagination = new Pagination<BanViewModels>
            {
                Items = items,
                TotalRecords = TotalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return Ok(pagination);
        }

        //URL Get: http://locahost:5001/api/ban/{id}
        //Lấy người dùng theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<BanViewModels>> GetBanbyId(int id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null)
            {
                return NotFound();
            }
            var model = new BanViewModels
            {
                MaBan = ban.MaBan,
                SoBan = ban.SoBan,
                TrangThai = ban.TrangThai,
            };
            return Ok(model);
        }
        //Url: http://locahost:7066/api/ban/{id}
        //Thêm bàn 
        [HttpPost]
        public async Task<ActionResult<BanViewModels>> PostBan(BanCreateVm model)
        {
            var ban = new Ban
            {
                SoBan = model.SoBan,
                TrangThai = "Trống"
            };
            _context.Bans.Add(ban);
            await _context.SaveChangesAsync();
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return CreatedAtAction(nameof(GetBanbyId), new { id = ban.MaBan }, model);
            }
            return BadRequest("Không thể thêm người dùng mới.");
        }

        //Url: http://locahost:7066/api/ban/{id}
        //Cập nhật thông tin bàn
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBan(int id,BanUpdateVm model)
        {
            if (id != model.MaBan)
            {
                return BadRequest("ID không khớp.");
            }    
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null) return NotFound();
            {
                ban.SoBan = model.Soban;
                ban.TrangThai = model.TrangThai;
            }

            _context.Entry(ban).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật bàn thành công." });
        }

        //Url: http://locahost:7066/api/ban/{id}
        //Xóa bàn
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBan(int id)
        {
            var ban = await _context.Bans.FindAsync();
            if (ban == null) return NotFound();

            _context.Bans.Remove(ban);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa bàn thành công." });
        }

    }
}

