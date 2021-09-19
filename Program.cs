using Newtonsoft.Json.Serialization;
using vault;

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls($"http://*.*.*.*:{port}");

builder.Services.AddControllers().AddNewtonsoftJson(x =>
{
    x.SerializerSettings.ContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new SnakeCaseNamingStrategy()
    };
});
builder.Services.AddScoped<IFireStoreAdapter, FireStoreAdapter>();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ShieldMiddleware>();
app.Run();
