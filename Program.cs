using CollegeChatbot.Services;
using GCTConnect.Interfaces;
using GCTConnect.Models;
using GCTConnect.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
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
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<GeminiChatService>();
builder.Services.AddSingleton<OpenAIService>();
builder.Services.AddHttpClient<OpenAIService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSignalR();
builder.Services.AddHttpClient<QdrantVectorStore>();
builder.Services.AddSingleton<GeminiLlmClient>();
builder.Services.AddSingleton<OpenAiLlmClient>();
builder.Services.AddScoped<RagService>();

builder.Services.AddDbContext<GctConnectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("gctconnectdbcs")));
builder.Services.AddTransient<EmailService>();

// Configure request size limits BEFORE building the app
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100_000_000; // 100MB
});

// For Kestrel - configure through builder.WebHost
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 100_000_000; // 100MB
});

// NOW build the application
var app = builder.Build();

// Error handling
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

app.UseHttpsRedirection();
app.UseStaticFiles(); // serves wwwroot/*

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "react")),
    RequestPath = "/react"
});

// React SPA fallback
app.MapFallbackToFile("/react/{*path}", "react/index.html");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// SignalR hub
app.MapHub<ChatHub>("/Services/ChatHub");

app.Run();