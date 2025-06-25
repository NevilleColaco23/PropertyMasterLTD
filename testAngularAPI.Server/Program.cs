using MyWarehouse.Infrastructure;
using System.Reflection;
using testAngularAPI.Server.Logging;
using testAngularAPI.Server.Mongo;

Console.WriteLine("Starting application...");
var builder = WebApplication.CreateBuilder(args);

builder.Host
          .AddMySerilogLogging() // Notice: Logging overrides.
          .ConfigureAppConfiguration((context, config) =>
          {
              // ConfigureWebHostDefaults only adds secrets if environment is Develop.
              // This ensures they're always added, for local testing of Production setting.
              config.AddUserSecrets(Assembly.GetEntryAssembly(), optional: true);

              // Notice: Infrastructure hook.
              config.AddMyInfrastructureConfiguration(context);
          });

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MongoDbContext>();

var startup = new Startup(builder.Configuration, builder.Environment);
startup.ConfigureServices(builder.Services);


var app = builder.Build();
startup.Configure(app);
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("Application started.");
});

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
