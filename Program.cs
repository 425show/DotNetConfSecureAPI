using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddCors(options => options.AddPolicy("allowAny", o => o.AllowAnyOrigin()));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


app.MapGet("/volcanos/{name?}", async (string? name) => {
    if (name is null)
    {
        return $"Hello, World!. You're getting ALL the volcanos";
    }
    else
    {
        return $"Hello,You'll get data for Volcano: {name}!";

    }
}).RequireAuthorization();

app.MapPost("/volcano", (Volcano volcano) => {
    return $"Hello, this call will create a new volcano: {volcano.VolcanoName}!";
});//.RequireAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

class Location
{
    public string? type { get; set; }
    public List<double>? coordinates { get; set; }
}

class Volcano
{
    public string? VolcanoName { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public Location? Location { get; set; }
    public int Elevation { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? LastKnownEruption { get; set; }
    public string? id { get; set; }
}
