using MandarinBid.Data;
using MandarinBid.Services;
using MandarinBid.Services.Background;
using MandarinBid.Services.Implementations;
using MandarinBid.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.RequireUniqueEmail = true;
});

//времнно скипаем логин
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();


//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    //.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddHostedService<MandarinGeneratorService>();
builder.Services.AddHostedService<MandarinCleanupService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<EmailBackgroundService>();
 
builder.Services.AddScoped<IAuctionService, AuctionService>(); 

builder.Services.AddHttpClient<IEmailService, MailtrapEmailService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!db.Mandarins.Any())
    {
        db.Mandarins.Add(new MandarinBid.Models.Mandarin
        {
            Name = "🍊Мандаринка #1",
            CurrentPrice = 100, 
            ExpirationDate = DateTimeOffset.UtcNow.AddMinutes(1)
        });

        db.Mandarins.Add(new MandarinBid.Models.Mandarin
        {
            Name = "🍊Мандаринка #2",
            CurrentPrice = 150,
            ExpirationDate = DateTimeOffset.UtcNow.AddMinutes(60)
        });

        db.SaveChanges();
    }
}
app.Run();


