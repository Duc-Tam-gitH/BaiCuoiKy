using Azure.Core;
using BaiCuoiKy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class FavoriteController : Controller
{
    private readonly AppDbContext _context;
    public FavoriteController(AppDbContext context) => _context = context;

    [HttpPost]
    public async Task<IActionResult> ToggleFavorite(int troId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var fav = await _context.Favorites.FirstOrDefaultAsync(f => f.TroId == troId && f.UserId == userId);

        if (fav != null) _context.Favorites.Remove(fav);
        else _context.Favorites.Add(new Favorite { TroId = troId, UserId = userId });

        await _context.SaveChangesAsync();

        // Sau khi xóa xong, quay lại trang trước đó (Trang Favorites)
        return Redirect(Request.Headers["Referer"].ToString());
    }
}