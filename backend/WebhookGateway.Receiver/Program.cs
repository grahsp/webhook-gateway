namespace WebhookGateway.Receiver;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var app = builder.Build();

		app.MapPost("/success", () =>
		{
			Console.WriteLine("Responded with success");
			return Results.StatusCode(200);
		});

		app.MapPost("/failure", () =>
		{
			Console.WriteLine("Responded with failure");
			return Results.StatusCode(500);
		});

		app.MapPost("/slow", async () =>
		{
			await Task.Delay(TimeSpan.FromSeconds(5));
			Console.WriteLine("Responded with delayed success");
			return Results.Ok();
		});

		app.Run();
	}
}