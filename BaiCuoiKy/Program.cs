using BaiCuoiKy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =========================
// 1. DATABASE
// =========================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =========================
// 2. IDENTITY
// =========================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password config
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// =========================
// 3. COOKIE CONFIG (QUAN TRỌNG)
// =========================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";

    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);

    options.SlidingExpiration = true;
});

// =========================
// 4. MVC
// =========================
builder.Services.AddControllersWithViews();

var app = builder.Build();

// =========================
// 5. PIPELINE
// =========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ⚠️ PHẢI CÓ THỨ TỰ NÀY
app.UseAuthentication();
app.UseAuthorization();

// =========================
// 6. ROUTE
// =========================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// =========================
// 7. SEED ROLE (AUTO CREATE)
// =========================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Tạo role
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    if (!await roleManager.RoleExistsAsync("Customer"))
        await roleManager.CreateAsync(new IdentityRole("Customer"));

    // Tạo admin account
    var adminEmail = "admin@gmail.com";
    var admin = await userManager.FindByEmailAsync(adminEmail);

    if (admin == null)
    {
        var newAdmin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Admin"
        };

        await userManager.CreateAsync(newAdmin, "Admin@123");
        await userManager.AddToRoleAsync(newAdmin, "Admin");
    }
}

app.Run();
