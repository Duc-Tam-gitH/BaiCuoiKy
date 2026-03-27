using BaiCuoiKy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
public class NotificationController : Controller
{
    private readonly AppDbContext _context;
    public NotificationController(AppDbContext context) => _context = context;

    // Trang xem tất cả thông báo
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return View(notifications);
    }

    // (Tùy chọn) Hàm đánh dấu đã đọc tất cả
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var unreadNotis = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead).ToListAsync();

        foreach (var noti in unreadNotis) noti.IsRead = true;

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}