//using SignalR.Services;
//using SignalR.Hubs;
//using SignalR.Services;

//var builder = WebApplication.CreateBuilder(args);

//// MVC + SignalR
//builder.Services.AddControllersWithViews();
//builder.Services.AddSignalR();

//// Register background watcher that polls DB
//builder.Services.AddHostedService<DatabaseWatcher>();

//var app = builder.Build();

//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}
//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();
//app.UseAuthorization();

//// MVC default route
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//// SignalR hub endpoint
//app.MapHub<NotificationHub>("/notifyHub");

//app.Run();

using SignalR.Hubs;
using SignalR.Services;
using SignalR.Hubs;
using SignalR.Services;

var builder = WebApplication.CreateBuilder(args);

// SignalR
builder.Services.AddSignalR();
builder.Services.AddControllers();

// Background polling service
builder.Services.AddHostedService<MessagePollingService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<MessageHub>("/messageHub");

app.Run();
