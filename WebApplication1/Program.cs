using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.SemanticKernel;
using WebApplication1.Controllers.N8n;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IProductDetailRecommendationService, N8nService>();
builder.Services.AddScoped<IValidator<CreateProductRequest>, CreateProductRequestValidator>();

// 註冊 Kernel
// https://learn.microsoft.com/en-us/semantic-kernel/concepts/kernel?pivots=programming-language-csharp
// https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp
builder.Services.AddOpenAIChatCompletion(
    modelId: "gpt-4.1",
    apiKey: builder.Configuration["OpenAiApiKey"]
);
builder.Services.AddTransient<Kernel>(serviceProvider =>
{
    return new Kernel(serviceProvider);
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
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();