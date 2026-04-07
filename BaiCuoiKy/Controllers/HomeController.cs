using BaiCuoiKy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace BaiCuoiKy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager; 
        }


        // ==================== TRANG CHỦ ====================
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 🔔 Notifications
            if (userId != null)
            {
                ViewBag.Notifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                ViewBag.NotiCount = await _context.Notifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead);
            }

            // 📂 Load danh mục cho dropdown
            ViewBag.Categories = await _context.Categories
                .Where(c => c.TrangThai == true)
                .ToListAsync();

            // 📍 Load danh sách Khu Vực cho thanh tìm kiếm (Lấy các khu vực độc nhất từ các phòng đang trống)
            ViewBag.KhuVucs = await _context.Tros
                .Where(t => t.TrangThai == TrangThaiPhong.DangTrong && !t.IsDeleted)
                .Select(t => t.KhuVuc)
                .Distinct()
                .OrderBy(k => k)
                .ToListAsync();

            try
            {
                var danhSachTro = await _context.Tros
                    .Include(t => t.AnhPhongs)
                    .Where(t => (t.TrangThai == TrangThaiPhong.DangTrong
                             || t.TrangThai == TrangThaiPhong.DangXuLy)
                             && !t.IsDeleted) // Lọc thêm điều kiện chưa bị xóa
                    .OrderByDescending(t => t.NgayDang)
                    .Take(8)
                    .ToListAsync();

                return View(danhSachTro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load home index data");
                return View(new List<Tro>());
            }
        }

        // ==================== TÌM KIẾM ====================
        [HttpGet]
        // ✅ Cập nhật tham số để khớp với các name="" trong form HTML tìm kiếm mới
        public async Task<IActionResult> Search(string tukhoa, string khuvuc, int? categoryId, string gia)
        {
            // 📂 Load danh mục cho dropdown (quan trọng để header/navbar không lỗi)
            ViewBag.Categories = await _context.Categories
                .Where(c => c.TrangThai == true)
                .ToListAsync();

            // Load lại Khu Vực để lỡ họ có tìm kiếm thì Dropdown vẫn hiển thị
            ViewBag.KhuVucs = await _context.Tros
                .Where(t => t.TrangThai == TrangThaiPhong.DangTrong && !t.IsDeleted)
                .Select(t => t.KhuVuc)
                .Distinct()
                .OrderBy(k => k)
                .ToListAsync();

            var query = _context.Tros
                .Include(t => t.AnhPhongs)
                .Include(t => t.Category)
                .Where(t => (t.TrangThai == TrangThaiPhong.DangTrong
                         || t.TrangThai == TrangThaiPhong.DangXuLy)
                         && !t.IsDeleted);

            // 🔍 Tìm theo từ khóa (tukhoa)
            if (!string.IsNullOrEmpty(tukhoa))
            {
                query = query.Where(t => t.TieuDe.Contains(tukhoa) || t.MoTa.Contains(tukhoa));
            }

            // 📍 Lọc khu vực (Bây giờ đã có trường KhuVuc riêng xác thực)
            if (!string.IsNullOrEmpty(khuvuc))
            {
                query = query.Where(t => t.KhuVuc == khuvuc);
            }

            // 📂 Lọc theo danh mục (Sử dụng categoryId từ select)
            if (categoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            // 💰 Lọc theo khoảng giá (Phân tích cú pháp min-max từ HTML)
            if (!string.IsNullOrEmpty(gia))
            {
                if (gia == "4000000-max")
                {
                    query = query.Where(t => t.Gia >= 4000000);
                }
                else
                {
                    var priceRange = gia.Split('-');
                    if (priceRange.Length == 2 &&
                        decimal.TryParse(priceRange[0], out decimal minPrice) &&
                        decimal.TryParse(priceRange[1], out decimal maxPrice))
                    {
                        query = query.Where(t => t.Gia >= minPrice && t.Gia <= maxPrice);
                    }
                }
            }

            var results = await query
                .OrderByDescending(t => t.NgayDang)
                .ToListAsync();

            // 🔁 Giữ lại giá trị filter để gán lại vào view nếu cần
            ViewBag.TuKhoa = tukhoa;
            ViewBag.KhuVucDaChon = khuvuc;
            ViewBag.CategoryIdDaChon = categoryId;
            ViewBag.GiaDaChon = gia;

            return View(results);
        }

        // ==================== CHI TIẾT ====================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Thêm đoạn này để Navbar có dữ liệu hiển thị
            ViewBag.Categories = await _context.Categories
                .Where(c => c.TrangThai == true)
                .ToListAsync();

            var tro = await _context.Tros
                .Include(t => t.User)
                .Include(t => t.AnhPhongs)
                .Include(t => t.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            if (tro == null) return NotFound();

            return View("~/Views/Tro/Details.cshtml", tro);
        }

        // ==================== ERROR ====================
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }



        // ==================== ĐÁNH GIÁ (REVIEW) ====================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostReview(int TroId, int Rating, string Comment)
        {
            // 1. Lấy ID của người dùng đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2. Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(Comment))
            {
                // Có thể dùng TempData để thông báo lỗi nếu cần
                return RedirectToAction("Details", new { id = TroId });
            }

            // 3. Tạo một đánh giá mới
            var review = new Review
            {
                TroId = TroId,
                UserId = userId,
                Rating = Rating,
                Comment = Comment,
                NgayDanhGia = DateTime.Now,
                IsHidden = false
            };

            // 4. Lưu vào Database
            _context.Add(review);
            await _context.SaveChangesAsync();

            // 5. Load lại trang Chi tiết phòng để hiển thị đánh giá mới
            return RedirectToAction("Details", new { id = TroId });
        }
        // ==================== CHÍNH SÁCH BẢO MẬT ====================
        public IActionResult Privacy()
        {
            return View();
        }
        // ==================== TRANG HỖ TRỢ ====================
        [HttpGet]
        public IActionResult Support()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSupport(string Name, string Email, string Phone, string Message)
        {
            try
            {
                // 1. Đóng gói dữ liệu từ Form vào Model
                var ticket = new SupportTicket
                {
                    Name = Name,
                    Email = Email,
                    Phone = Phone,
                    Message = Message,
                    CreatedAt = DateTime.Now,
                    IsResolved = false // Mặc định là chưa giải quyết
                };

                // Lưu vào cơ sở dữ liệu
                _context.SupportTickets.Add(ticket);
                await _context.SaveChangesAsync(); // Lưu để sinh ra ID (nếu cần)

                // 2. THÔNG BÁO TỚI TÀI KHOẢN ADMIN (In-app Notification)
                // Lấy danh sách các user có quyền "Admin"
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                foreach (var admin in admins)
                {
                    var notification = new Notification
                    {
                        UserId = admin.Id,
                        CreatedAt = DateTime.Now,
                        IsRead = false,
                        Message = $"Khách hàng {Name} vừa gửi một yêu cầu hỗ trợ mới.",
                        Url = "/Admin/Dashboard?section=support" 
                    };
                    _context.Notifications.Add(notification);
                }
                await _context.SaveChangesAsync();

                // 3. GỬI THÔNG BÁO TỚI GMAIL ADMIN
               
                string fromEmail = "tamv5771@gmail.com"; 
                string emailPassword = "okfz qiob zlym somn";      
                string adminGmail = "tamv5771@gmail.com";              
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, emailPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "House88 Support System"),
                    Subject = $"[House88] Yêu cầu hỗ trợ mới từ {Name}",
                    Body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2 style='color: #002641;'>Có yêu cầu hỗ trợ mới trên hệ thống</h2>
                    <p><strong>Người gửi:</strong> {Name}</p>
                    <p><strong>Số điện thoại:</strong> {Phone}</p>
                    <p><strong>Email khách:</strong> {Email}</p>
                    <hr/>
                    <p><strong>Nội dung hỗ trợ:</strong></p>
                    <div style='background: #f8fafc; padding: 15px; border-left: 4px solid #ED005A;'>
                        {Message.Replace("\n", "<br/>")}
                    </div>
                </div>",
                    IsBodyHtml = true, 
                };

                mailMessage.To.Add(adminGmail);

               
                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (Exception mailEx)
                {
                   
                    Console.WriteLine("Lỗi gửi mail: " + mailEx.Message);
                }

              
                TempData["SuccessMessage"] = "Cảm ơn bạn! Yêu cầu hỗ trợ đã được gửi thành công. Chúng tôi sẽ liên hệ lại sớm nhất.";
            }
            catch (Exception ex)
            {
                
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu dữ liệu. Vui lòng thử lại sau!";
            }

            return RedirectToAction("Support");
        }
    }
}