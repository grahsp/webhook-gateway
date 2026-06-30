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

		app.MapPost("/random", async () =>
		{
			var random = new Random();
			// await Task.Delay(TimeSpan.FromSeconds(random.Next(1, 5)));

			var result = random.Next(0, 100);
			
			Console.WriteLine($"Responded with {result}");
			return result switch
			{
				< 50 => Results.Ok(),
				< 80 => Results.StatusCode(500),
				_ => Results.StatusCode(300)
			};
		});

		app.Run();
	}
}