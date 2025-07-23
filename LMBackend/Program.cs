using LMBackend;
using LMBackend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Asp.Versioning.Conventions;

var builder = WebApplication.CreateBuilder(args);

// Add my dependencies
builder.Services.AddSingleton<IDockerHelper, DockerHelper>();
builder.Services.AddSingleton<ILlmService, LlmClient>();
builder.Services.AddSingleton<ISerpService, LMBackend.RAG.SerpService>();
builder.Services.AddSingleton<IVectorStoreService, LMBackend.RAG.ChromaVectorStoreService>();
builder.Services.AddHttpClient<ISerpService, LMBackend.RAG.SerpService>(httpClient =>
{
    httpClient.BaseAddress = new Uri("https://serpapi.com/search");
});

// Add services to the container.
// Fix navigation property cycle in JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
//builder.Services.AddDbContext<ChatContext>(opt => opt.UseInMemoryDatabase("Chat"));
//builder.Services.AddDbContext<ChatContext>(opt => opt.UseSqlite("Data Source=chat.db"));
string connection = $"Host={Constants.PGSQL_IP};Port={Constants.PGSQL_PORT};Database=lmdb;Username=lmbackend;Password=sql_pass";
builder.Services.AddDbContext<ChatContext>(opt => opt.UseNpgsql(connection));

// Add API version
// see:
// https://github.com/dotnet/aspnet-api-versioning/wiki/New-Services-Quick-Start
// https://medium.com/@dipendupaul/documenting-a-versioned-net-web-api-using-swagger-eec0fe7aa010
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddMvc(options =>
{
    // Automatically applies an api version based on the name of the defining controller's namespace
    options.Conventions.Add(new VersionByNamespaceConvention());
}).AddApiExplorer(setup =>
{
    // Call AddApiExplorer(), so swagger will automatically add API version 
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Add swagger and bearer JWT token input
builder.Services.AddSwaggerGen(config =>
{
    config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
        "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    config.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
    // Add swagger comment from XML
    string xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    config.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Add JWT user authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,  // must match ValidIssuer
        ValidateAudience = true,  // must match ValidAudience
        ValidateLifetime = true,  // must check expired time
        ValidateIssuerSigningKey = true,  // must match IssuerSigningKey
        ValidIssuer = "lmbackend",
        ValidAudience = "lmbackend",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("a very long text over 128 bits to prevent IDX10603"))
    };
});

// Allow cross-origin
const string policyName = "myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: policyName,
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();  // we don't need HTTPS in docker

// CORS must before authentication and authorization
app.UseCors(policyName);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Add websocket support for audio streaming
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(1)
};
app.UseWebSockets(webSocketOptions);

app.Run();
