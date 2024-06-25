using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using SSSCalBlazor.Data;
using SSSCalBlazor.Models;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
//increase to 65535 for chunking
builder.Services.AddServerSideBlazor().AddHubOptions(options => options.MaximumReceiveMessageSize = 640 * 1024);
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddHttpClient();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
//builder.Services.AddAuthorizationCore();

//builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMYServices();// builder.Environment.BaseAddress);



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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
