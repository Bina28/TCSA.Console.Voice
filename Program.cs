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
        if (order.Count > 0)
        {
            ShowOrderSummary(order);
        }

        ShowProducts();

        Console.WriteLine("Which coffee would you like?");
        bool speechRecognized = false;

        while (!speechRecognized)
        {
            var result = await recognizer.RecognizeOnceAsync();

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                string userChoice = Regex.Replace(result.Text.Trim().ToLower(), @"[^a-z0-9\s]", "");

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

        Console.WriteLine("Press any key to continue");
        Console.ReadKey();

        Console.Clear();
        Console.WriteLine("Do you want to order something else? (yes/no)");
        var continueResult = await recognizer.RecognizeOnceAsync();

        if (continueResult.Reason == ResultReason.RecognizedSpeech)
        {
            string continueChoice = continueResult.Text.Trim().ToLower();
            continueOrdering = continueChoice.StartsWith("y");
        }
        else
        {
            Console.WriteLine("Assuming you want to continue ordering.");
        }
    }

    ShowOrderSummary(order);

    decimal finalTotalPrice = order.Sum(p => p.price);
    Console.WriteLine($"Total price: {finalTotalPrice.ToString("C", CultureInfo.CurrentCulture)}");
}

void ShowProducts()
{
    var table = new Table();

    table.AddColumn("Name");
    table.AddColumn("Price");

    foreach (var product in products)
    {
        table.AddRow(product.Name, product.price.ToString("C"));
    }

    AnsiConsole.Write(table);
}

void ShowOrderSummary(List<Product> order)
{
    Console.WriteLine("\nYour order summary:");
    foreach (var item in order)
    {
        Console.WriteLine($"{item.Name} - {item.price:C}");
    }
    Console.WriteLine();
}

record Product(int id, string Name, decimal price);

