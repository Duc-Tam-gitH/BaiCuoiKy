using Azure.Core;
using BaiCuoiKy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using BaiCuoiKy.Models.ViewModel;

[Authorize]
public class FavoriteController : Controller
{
    private readonly AppDbContext _context;
    public FavoriteController(AppDbContext context) => _context = context;

    [HttpPost]
    public async Task<IActionResult> ToggleFavorite(int troId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Tìm bài trọ để lấy thông tin chủ bài đăng
        var tro = await _context.Tros.FirstOrDefaultAsync(t => t.Id == troId);
        if (tro == null) return NotFound();

        var fav = await _context.Favorites.FirstOrDefaultAsync(f => f.TroId == troId && f.UserId == userId);

        if (fav != null)
        {
            // Hành động: Bỏ thích (Không cần gửi thông báo)
            _context.Favorites.Remove(fav);
        }
        else
        {
            // Hành động: Thích bài
            _context.Favorites.Add(new Favorite { TroId = troId, UserId = userId });

            // CHÈN THÔNG BÁO: Chỉ gửi nếu người thích không phải là chính chủ bài đăng
            if (tro.UserId != userId)
            {
                var notification = new Notification
                {
                    UserId = tro.UserId, // Thông báo cho chủ trọ
                    Message = $"❤️ Có người vừa yêu thích bài đăng '{tro.TieuDe}' của bạn!",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
            }
        }

        await _context.SaveChangesAsync();

        // Quay lại trang trước đó
        return Redirect(Request.Headers["Referer"].ToString());
    }
}