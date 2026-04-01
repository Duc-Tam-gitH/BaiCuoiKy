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
    [Authorize]
    public async Task<IActionResult> Read(int id)
    {
        // 1. Tìm thông báo trong Database dựa vào ID
        var notification = await _context.Notifications.FindAsync(id);

        if (notification != null)
        {
            // 2. Nếu thông báo chưa đọc (Mới) thì đổi thành Đã đọc
            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync(); // Lưu thay đổi vào DB
            }

            // 3. Chuyển hướng đến bài viết (Dựa vào đường link đã lưu ở cột Url)
            if (!string.IsNullOrEmpty(notification.Url))
            {
                return Redirect(notification.Url);
            }
        }

        // Nếu thông báo bị lỗi hoặc không có link, cho quay lại trang danh sách thông báo
        return RedirectToAction("Index");
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        // 1. Lấy ID người dùng hiện tại
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 2. Tìm đúng thông báo của người đó
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }

        // 3. Quay lại trang danh sách thông báo
        return RedirectToAction("Index");
    }
}