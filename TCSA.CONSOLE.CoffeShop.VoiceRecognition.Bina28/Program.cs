
using Microsoft.CognitiveServices.Speech;
using Spectre.Console;
using System.Text.RegularExpressions;
using System.Globalization;


var products = new List<Product>
{
	new Product(1, "Espresso", 2.50m),
	new Product(2, "Americano", 3.00m),
	new Product(3, "Cappuccino", 3.50m),
	new Product(4, "Latte", 4.00m),
	new Product(5, "Mocha", 4.50m),
	new Product(6, "Macchiato", 3.75m),
	new Product(7, "Flat White", 4.00m),
	new Product(8, "Cortado", 3.25m),
	new Product(9, "Affogato", 5.00m),
	new Product(10, "Cold Brew", 3.75m)
};
var order = new List<Product>();
await GetOrder();


async Task GetOrder()
{
	bool continueOrdering = true;
	var config = SpeechConfig.FromSubscription("a11dc0dbdfa94d98829efd494e52f169", "eastus");
	using var recognizer = new SpeechRecognizer(config);
	while (continueOrdering)
	{
		Console.Clear();
		ShowGrid();
		Console.WriteLine("Which coffee would you like? If you would like to finish your order, say 'finish'");

		bool productRecognized = false;
		while (!productRecognized)
		{
			var result = await recognizer.RecognizeOnceAsync();
			if (result.Reason == ResultReason.RecognizedSpeech)
			{
				string userChoice = Regex.Replace(result.Text.Trim().ToLower(), @"[^a-z0-9\s]", "");

				if (userChoice == "finish")
				{
					continueOrdering = false;
					break;
				}
				var selectedProduct = products.FirstOrDefault(p => p.Name.ToLower() == userChoice);

				if (selectedProduct != null)
				{
					order.Add(selectedProduct);
					decimal totalPrice = order.Sum(p => p.Price);
					Console.WriteLine($"Added {selectedProduct.Name} to your order. Total: {totalPrice.ToString("C", CultureInfo.CurrentCulture)}");

				}
				else
				{
					Console.WriteLine("Sorry, that product is not available or we can't understand you");

				}
				productRecognized = true;
			}
		}

		if (continueOrdering)
		{
			Console.WriteLine("Press any key to continue");
			Console.ReadKey();
		}
	}


	Console.WriteLine("Your order is complete! Your coffee come soon!");
}

void ShowGrid()
{
	var menuPanel = CreateMenuPanel();
	var orderPanel = CreateOrderPanel(order);
	var grid = new Grid();
	grid.AddColumn();
	grid.AddColumn();
	grid.AddRow(menuPanel, orderPanel);

	AnsiConsole.Write(grid);
}
Panel CreateMenuPanel()
{
	var table = new Table().Centered();
	table.AddColumn("[bold yellow]Name[/]");
	table.AddColumn("[bold yellow]Price[/]");

	foreach (var product in products)
	{
		table.AddRow($"[green]{product.Name}[/]", $"[blue]{product.Price.ToString("C")}[/]");
	}

	return new Panel(table)
	{
		Header = new PanelHeader("[underline cyan]Product list[/]"),
		Border = BoxBorder.Rounded,
		BorderStyle = new Style(Color.DeepSkyBlue2),
		Padding = new Padding(1)
	};
}

Panel CreateOrderPanel(List<Product> order)
{
	var table = new Table().Centered();
	table.AddColumn("[bold yellow]Name[/]");
	table.AddColumn("[bold yellow]Price[/]");

	foreach (var item in order)
	{
		table.AddRow($"[green]{item.Name}[/]", $"[blue]{item.Price.ToString("C")}[/]");
	}

	var totalPrice = order.Sum(item => item.Price);
	table.AddEmptyRow();
	table.AddRow("[bold yellow]Total[/]", $"[bold red]{totalPrice:C}[/]");

	return new Panel(table)
	{
		Header = new PanelHeader("[underline cyan]Current order[/]"),
		Border = BoxBorder.Rounded,
		BorderStyle = new Style(Color.DeepSkyBlue2),
		Padding = new Padding(1)
	};
}

record Product(int Id, string Name, decimal Price);