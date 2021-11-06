using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddCors(options => options.AddPolicy("allowAny", o => o.AllowAnyOrigin()));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/volcanos/{name?}", Task<string>(string? name, HttpContext context) => {
    context.VerifyUserHasAnyAcceptedScope(new string[] { "access_as_user" });
    if (name is null)
    {
        return Task.FromResult("You're getting ALL the volcanos");
    }
    else
    {
        return Task.FromResult($"You're getting results for volcanos: {name}");
    }
}).RequireAuthorization();

app.MapPost("/volcano", ( HttpContext context) => {
    context.VerifyUserHasAnyAcceptedScope(new string[] { "access_as_user" });
    
    return "About to create a Volcano";
    
}).RequireAuthorization();

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