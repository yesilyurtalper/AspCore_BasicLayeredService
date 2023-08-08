
using System.Text.Json.Serialization;
using BasicLayeredAPI.API.Filters;
using BasicLayeredAPI.API.Extensions;
using BasicLayeredAPI.API.DBContext;
using BasicLayeredAPI.API.Services.Persistence;
using BasicLayeredAPI.API.Constants;
using BasicLayeredAPI.API.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers(options => {
    options.Filters.Add<ValidationFilter>();
}).
    ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    }).
    AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

services.AddScoped<ResultLoggerAttribute>();

//services.AddLogging(loggingBuilder =>
//{
//    loggingBuilder.ClearProviders();
//    loggingBuilder.AddSerilog();
//});

builder.Host.AddCustomSerilog();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
if (builder.Environment.IsDevelopment())
    services.AddSwaggerServices();

//string conStr = "server=localhost;port=3306;database=ECommerceItemAPI;user=root;password=pass;";
//string conStr = configuration.GetConnectionString("DefaultConnection");
string conStr = Environment.GetEnvironmentVariable("DB_CON_STR");
services.AddDbContext<PostingAPIDbContext>(options => options.UseMySQL(conStr));

services.AddScoped<IPostRepo, DBPostRepo>();

services.AddAuthServices();
services.AddHttpClients();

var app = builder.Build();

//Migrate latest database changes during startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PostingAPIDbContext>();
    dbContext.Database.Migrate();
}

//custom global exception handling middleware
app.UseExceptionMiddleware();
app.UseErrorMiddleware();

//app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//var elasticAPMSettings = new Dictionary<string, string>();
//elasticAPMSettings.Add("ElasticApm:SecretToken","");
//elasticAPMSettings.Add("ElasticApm:ServerUrl", Environment.GetEnvironmentVariable("ELK_APMURL"));
//elasticAPMSettings.Add("ElasticApm:ServiceName", "ECommerce.ItemService");
//app.UseAllElasticApm(new ConfigurationBuilder().AddInMemoryCollection(elasticAPMSettings).Build());

app.UseCors("all");
//app.UseCors(builder =>
//    builder.WithOrigins(Environment.GetEnvironmentVariable("ALLOWED_ORIGINS").Split(","))
//    .AllowAnyMethod().
//    AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization(APIConstants.BasicLayeredAPIWebClient);

app.Run();