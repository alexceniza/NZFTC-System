using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NZFTC_Portal.Interfaces;
using NZFTC_Portal.Models;
using NZFTC_Portal.Services;

var builder = WebApplication.CreateBuilder(args);

// Gets the MySQL connection string from appsettings.
// This will be used by AppDbContext for database connectivity.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Adds MVC controller and view support.
builder.Services.AddControllersWithViews();

// Adds in-memory storage for session data.
builder.Services.AddDistributedMemoryCache();

// Registers the Entity Framework database context.
// This allows the project to connect to the MySQL database.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Registers HttpContext access for services that may need session/request data.
builder.Services.AddHttpContextAccessor();

// Adds session support for lightweight login state.
builder.Services.AddSession(options =>
{
    // Keeps the session active for the sprint demo flow.
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    // Prevents JavaScript from reading the session cookie.
    options.Cookie.HttpOnly = true;

    // Marks the cookie as essential for the app to function.
    options.Cookie.IsEssential = true;
});

// Registers backend services used during Sprint 1 integration.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IHolidayService, HolidayService>();
// Registers backend services used during Sprint 2 integration.
builder.Services.AddScoped<ILeaveService, LeaveService>();
// Registers backend services used during Sprint 3 integration.
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<ICaseService, CaseService>();

var app = builder.Build();

// Configures the production error pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enables session before controller actions run.
app.UseSession();

app.UseAuthorization();

// Sets the login page as the startup route.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();