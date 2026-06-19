using KMCEventSphereWeb.Services;

var builder = WebApplication.CreateBuilder(args);

//  Razor Pages services
builder.Services.AddRazorPages();

//  Session support
builder.Services.AddSession();

// Register IHttpContextAccessor here (important for X-UserID in services)
builder.Services.AddHttpContextAccessor();

// Register API client services BEFORE building the app
builder.Services.AddHttpClient<EventService>();
builder.Services.AddHttpClient<OrganizerService>();
builder.Services.AddHttpClient<UserService>();
builder.Services.AddHttpClient<RegistrationService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // Default HSTS 30 days
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Map static assets and Razor Pages
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();