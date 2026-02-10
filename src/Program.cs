using src;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AcomDb>(opt => opt.Use("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();
app.MapGet("/", () => "Hello World!");

app.Run();
