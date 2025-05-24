using DataAccess;
using Interface.Components;
using Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AppDbContext>();
builder.Services.AddScoped<Login>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AdminSService>();
builder.Services.AddScoped<ResourceService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<AdminPService>();
builder.Services.AddScoped<MemberPService>();
builder.Services.AddScoped<CpmService>();
builder.Services.AddScoped<GanttService>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();