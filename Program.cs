using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/",()=>"API is running!");
var sampleTodos = new Todo[] {
	new(1, "Walk the dog"),
	new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
	new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
	new(4, "Clean the bathroom"),
	new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos");

todosApi.MapGet("/", () => sampleTodos);

todosApi.MapGet("/{id}", (int id) =>
	sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
		? Results.Ok(todo)
		: Results.NotFound());
app.MapPost("/chat", async (string message) =>
{
	var apiKey = "OPENAI_API_KEY";

	using var httpClient = new HttpClient();

	httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

	var requestBody = new
	{
		model = "gpt-4o-mini",
		messages = new[]
		{
			new { role = "user", content = message }
		}
	};

	var response = await httpClient.PostAsJsonAsync(
		"https://api.openai.com/v1/chat/completions",
		requestBody
	);

	var json = await response.Content.ReadFromJsonAsync<JsonElement>();

	var reply = json
		.GetProperty("choices")[0]
		.GetProperty("message")
		.GetProperty("content")
		.GetString();

	return Results.Ok(reply);
});

app.Run();

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);