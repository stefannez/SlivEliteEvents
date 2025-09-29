using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SlivEliteEvents.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure SQLite for Identity and application data
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.ReturnUrlParameter = "returnUrl";
    options.SlidingExpiration = true;
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Seed a default manager user
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Create Manager role
    if (!await roleManager.RoleExistsAsync("Manager"))
    {
        await roleManager.CreateAsync(new IdentityRole("Manager"));
    }
    
    // test user
    var testUser = new IdentityUser { UserName = "test@slivevents.com", Email = "test@slivevents.com" };
    await userManager.CreateAsync(testUser, "Test123!");

    // Create default manager user
    var managerEmail = "manager@slivevents.com";
    var managerPassword = "Manager123!";
    if (await userManager.FindByEmailAsync(managerEmail) == null)
    {
        var user = new IdentityUser { UserName = managerEmail, Email = managerEmail };
        var result = await userManager.CreateAsync(user, managerPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Manager");
        }
    }
}

app.Run();