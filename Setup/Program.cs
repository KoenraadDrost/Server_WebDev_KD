using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Setup.DAL;
using Setup.Models;

var KdrAllowedOrigins = "_kdrAllowedOrigins";

var builder = WebApplication.CreateBuilder(args);

// TODO: Might want to change cors routing to safer method later
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: KdrAllowedOrigins,
        policy =>
        {
            policy.WithOrigins("http://127.0.0.1:5500",
                                "http://127.0.0.1:5501",
                                "http://localhost:5500",
                                "http://localhost:5501",
                                "http://localhost:3000")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
        });
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("ServerDb");
builder.Services.AddDbContext<ServerContext>(x => x.UseSqlite(connectionString));

// Identity Options for User Accounts
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password Requirements
    options.Password.RequiredLength = 8;

    // Lockout Settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// TODO: Might want to change cors routing to safer method later
app.UseCors(KdrAllowedOrigins);

app.UseAuthorization();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
