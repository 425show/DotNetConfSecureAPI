using Microsoft.Azure.Cosmos;
using Azure.Identity;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddCors(options => options.AddPolicy("allowAny", o => o.AllowAnyOrigin()));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var tokenCredential = new ChainedTokenCredential
(
    new AzureCliCredential(),
    new ManagedIdentityCredential()
);

var endpointUrl = "https://cm-cosmos-demo.documents.azure.com";
CosmosClient cosmosClient = new CosmosClient(endpointUrl, tokenCredential);

app.MapGet("/volcanos/{name?}", async Task<List<Volcano>> (string? name, HttpContext context) => {
    context.VerifyUserHasAnyAcceptedScope(new string[] { "access_as_user" });
    if (name is null)
    {
        return await GetVolcanos();
    }
    else
    {
        return new List<Volcano> { await GetVolcano(name)};
    }
}).RequireAuthorization();

app.MapPost("/volcano", async Task<IResult> (Volcano volcano, HttpContext context) => {
    context.VerifyUserHasAnyAcceptedScope(new string[] { "access_as_user" });
    
    try
    {
        await CreateVolcano(volcano);
        return Results.Ok();
    }
    catch(CosmosException ex)
    {
        if (ex.StatusCode is System.Net.HttpStatusCode.Forbidden)
        {
            return Results.Forbid();
        }
        else
        {
            return Results.BadRequest();
        }
    }
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


async Task<List<Volcano>> GetVolcanos()
{
    QueryRequestOptions options = new QueryRequestOptions() { MaxBufferedItemCount = 100 };
    var volcanos = new List<Volcano>();
    var database = cosmosClient.GetDatabase("VolcanoList");
    var container = database.GetContainer("Volcano");
    var queryText = "SELECT * FROM Volcano";
    using (FeedIterator<Volcano> query = container.GetItemQueryIterator<Volcano>(
        queryText,
        requestOptions: options))
    {
        while (query.HasMoreResults)
        {
            foreach (var volcano in await query.ReadNextAsync())
            {
                volcanos.Add(volcano);
            }
        }
    }

    return volcanos;
}

async Task<Volcano> GetVolcano(string name)
{
    QueryDefinition query = new QueryDefinition(
        "SELECT * FROM Volcano s WHERE s.VolcanoName = @volcanoName")
        .WithParameter("@volcanoName",name);
    var database = cosmosClient.GetDatabase("VolcanoList");
    var container = database.GetContainer("Volcano");
    var volcanos = new List<Volcano>();
    using (FeedIterator<Volcano> resultSet = container.GetItemQueryIterator<Volcano>(query))
    {
        while (resultSet.HasMoreResults)
        {
            FeedResponse<Volcano> response = await resultSet.ReadNextAsync();
            volcanos.AddRange(response);
        }
    }
    return volcanos.FirstOrDefault();
}

async Task<ItemResponse<Volcano>> CreateVolcano(Volcano volcano)
{
    var database = cosmosClient.GetDatabase("VolcanoList");
    var container = database.GetContainer("Volcano");
    var response = await container.UpsertItemAsync<Volcano>(
        volcano, 
        new PartitionKey(Guid.NewGuid().ToString()));
    return response;
}

public class Location
{
    public string? type { get; set; }
    public List<double>? coordinates { get; set; }
}

public class Volcano
{
    public string? VolcanoName { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public Location? Location { get; set; }
    public int? Elevation { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? LastKnownEruption { get; set; }
    public string? id { get; set; }
    public string? _rid { get; set; }
    public string? _self { get; set; }
    public string? _etag { get; set; }
    public string? _attachments { get; set; }
    public int _ts { get; set; }
}


