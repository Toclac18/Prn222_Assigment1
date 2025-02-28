using NewsManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using NewsManagementSystem.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();

// Thêm Session vào dịch vụ
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthorization();

// Đăng ký Controllers với Views (FIX LỖI)
builder.Services.AddControllersWithViews();

// Đăng ký Controllers (Sửa lỗi)
builder.Services.AddControllers();

// ✅ 2️⃣ Đăng ký DbContext với SQL Server
builder.Services.AddDbContext<FunewsManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbContext"))); 
builder.Services.AddScoped<FunewsManagementContext>();

// ✅Cấu hình EmailSettings TRƯỚC khi builder.Build()
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

// ✅ 4️⃣ Tạo app sau khi hoàn tất cấu hình
var app = builder.Build();

// ✅ 5️⃣ Kiểm tra email settings (chỉ log, không cấu hình sau khi Build)
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();
if (emailSettings == null)
{
    throw new Exception("❌ EmailSettings không được cấu hình đúng hoặc không tồn tại trong appsettings.json!");
}

Console.WriteLine($"✅ SMTP Server: {emailSettings.SmtpServer}");
Console.WriteLine($"✅ Sender Email: {emailSettings.SenderEmail}");

// ✅ 6️⃣ Cấu hình Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
