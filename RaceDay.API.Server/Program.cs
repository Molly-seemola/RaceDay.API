using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

using RaceDay.API.Data;
using RaceDay.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. DATABASE
// ============================================
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================
// 2. CONTROLLERS
// ============================================
builder.Services.AddControllers();

// ============================================
// 3. SWAGGER UI
// ============================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "RESTful API for the RaceDay event management system"
    });
});

// ============================================
// 4. SESSION
// ============================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ============================================
// 5. CORS
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("RaceDayCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// ============================================
// 6. DEPENDENCY INJECTION
// ============================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IEnrolmentService, EnrolmentService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();

// ============================================
// 7. BUILD
// ============================================
var app = builder.Build();

// ============================================
// 8. MIDDLEWARE PIPELINE
// ============================================
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("RaceDayCors");
app.UseSession();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    application = "RaceDay API",
    version = "1.0",
    status = "Running",
    documentation = "/swagger"
}));

app.Run();