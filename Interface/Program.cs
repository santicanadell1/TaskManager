using DataAccess;
using Interface.Components;
using Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<InMemoryDatabase>();
builder.Services.AddScoped<Login>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AdminSService>();
builder.Services.AddScoped<ResourceService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<AdminPService>();
builder.Services.AddScoped<MemberPService>();
builder.Services.AddScoped<CpmService>();


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();