using Controllers;
using DataAccess;
using Interface.Components;
using Microsoft.EntityFrameworkCore;
using Service;
using Service.Converter;
using Service.Converters;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<LeaderPService>();

builder.Services.AddScoped<LoginController>();
builder.Services.AddScoped<SignUpController>();
builder.Services.AddScoped<ResourceController>();
builder.Services.AddScoped<AdminSystemController>();
builder.Services.AddScoped<UserController>();
builder.Services.AddScoped<TaskController>();
builder.Services.AddScoped<MemberProjectController>();
builder.Services.AddScoped<AdminProjectController>();
builder.Services.AddScoped<GanttController>();
builder.Services.AddScoped<LeaderProjectController>();
builder.Services.AddScoped<ResourceAdminController>();

builder.Services.AddScoped<IRepositoryManager, RepositoryManager>();
builder.Services.AddScoped<RolConverter>();
builder.Services.AddScoped<ResourceConverter>();
builder.Services.AddScoped<TaskConverter>();
builder.Services.AddScoped<UserConverter>();
builder.Services.AddScoped<ProjectConverter>();
builder.Services.AddScoped<NotificationConverter>();

builder.Services.AddScoped<IExporter, CSVExporter>();
builder.Services.AddScoped<IExporter, JSONExporter>();


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    providerOptions => providerOptions.EnableRetryOnFailure())
);
/*
builder.Services.AddDbContextFactory<AppDbContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        providerOptions => providerOptions.EnableRetryOnFailure())
);*/

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