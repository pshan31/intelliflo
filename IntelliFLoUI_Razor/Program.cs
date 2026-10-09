using IntelliFloCore_NetCore.Backgrounds;
using IntelliFloCore_NetCore.Ingestion;
using log4net;
using System.Reflection;

var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddRazorPages();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddHostedService<LogProcessingService>();
builder.Services.AddMemoryCache();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "services/{controller}"
);

//app.MapControllerRoute(
//    name: "ui",
//    pattern: "iui/{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
