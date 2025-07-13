using CollegeChatbot.Services;
using GCTConnect.Interfaces;
using GCTConnect.Models;
using GCTConnect.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login"; // Path to redirect unauthenticated users
        options.AccessDeniedPath = "/Home/AccessDenied"; // Path to redirect unauthorized users
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<IChatBotService, ChatBotService>();
builder.Services.AddSingleton<OpenAIService>();
//builder.Services.AddHttpClient<MistralService>();
//builder.Services.AddHostedService<FullSiteScraperBackgroundService>();

builder.Services.AddHttpClient<OpenAIService>();
//builder.Services.AddScoped<TextProcessingService>();  // Keep your service
//builder.Services.AddHostedService<ScrapedDataWorker>();  // Runs automatically


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout duration
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddDbContext<GctConnectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("gctconnectdbcs")));
builder.Services.AddTransient<EmailService>();

var app = builder.Build();
var dataFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "collegeData.json");
if (!System.IO.File.Exists(dataFile))
{
    try
    {
        var sourceFile = "[YOUR_SOURCE_PATH]/collegeData.json";
        if (System.IO.File.Exists(sourceFile))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dataFile));
            System.IO.File.Copy(sourceFile, dataFile);
            Console.WriteLine("collegeData.json copied to wwwroot folder.");
        }
        else
        {
            Console.WriteLine("Source collegeData.json file not found.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error copying collegeData.json: {ex.Message}");
    }
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{

    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}");
app.UseCors();
app.MapHub<ChatHub>("/Services/ChatHub");
app.Run();






