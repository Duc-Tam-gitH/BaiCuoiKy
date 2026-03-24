using BaiCuoiKy.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; // Thêm dòng này

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. THÊM VÀO ĐÂY: Đăng ký dịch vụ Identity để dùng được UserManager và SignInManager
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    // Bạn có thể cấu hình mật khẩu tại đây nếu muốn
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. THÊM VÀO ĐÂY: Kích hoạt xác thực (Authentication)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();