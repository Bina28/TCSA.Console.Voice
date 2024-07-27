using Microsoft.CognitiveServices.Speech;
using Spectre.Console;
using System.Globalization;
using System.Text.RegularExpressions;

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
    var config = SpeechConfig.FromSubscription("cda77c05009148468a80833404e05ad0", "southeastasia");
    using var recognizer = new SpeechRecognizer(config);

    bool continueOrdering = true;

    while (continueOrdering)
    {
        Console.Clear();
        Showgrid();

        Console.WriteLine("\nWhich coffee would you like? If you'd like to finish your order, say 'finish'");
        bool speechRecognized = false;

        while (!speechRecognized)
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
                    decimal totalPrice = order.Sum(p => p.price);
                    Console.WriteLine($"\nAdded {selectedProduct.Name} to your order. Total: {totalPrice.ToString("C", CultureInfo.CurrentCulture)}");
                }
                else
                {
                    Console.WriteLine("Sorry, that product is not available or we can't understand you.");
                }
                speechRecognized = true;
            }
        }

        if (continueOrdering)
        {
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
    }

    Console.WriteLine($"Order Completed! Your coffee is coming soon!");
}

void Showgrid()
{
    var menuPanel = CreateMenuPanel();
    var orderPanel = CreateOrderPanel();

    var grid = new Grid();
    grid.AddColumn();
    grid.AddColumn();
    grid.AddRow(menuPanel, orderPanel);

    AnsiConsole.Write(grid);
}

Panel CreateMenuPanel()
{
    var table = new Table()
        .Centered();

    table.AddColumn("[bold yellow]Name[/]");
    table.AddColumn("[bold yellow]Price[/]");

    foreach (var product in products)
    {
        table.AddRow($"[green]{product.Name}[/]", $"[blue]{product.price.ToString("C")}[/]");
    }

    return new Panel(table)
    {
        Header = new PanelHeader("[underline cyan]Product List[/]"),
        Border = BoxBorder.Rounded,
        BorderStyle = new Style(Color.DeepSkyBlue2), 
        Padding = new Padding(1) 
    }; 
}

Panel CreateOrderPanel()
{
    var orderTable = new Table();
    orderTable.AddColumn("[bold yellow]Order Item[/]");
    orderTable.AddColumn("[bold yellow]Price[/]");

    foreach (var item in order)
    {
        orderTable.AddRow($"[green]{item.Name}[/]", $"[blue]{item.price.ToString("C")}[/]");
    }

    var totalPrice = order.Sum(item => item.price);

    orderTable.AddEmptyRow();
    orderTable.AddRow("[bold yellow]Total[/]", $"[bold red]{totalPrice:C}[/]");

    return new Panel(orderTable)
    {
        Header = new PanelHeader("[underline cyan]Current Order[/]"),
        Border = BoxBorder.Rounded,
        BorderStyle = new Style(Color.GreenYellow),
        Padding = new Padding(1)
    };
}

record Product(int id, string Name, decimal price);

