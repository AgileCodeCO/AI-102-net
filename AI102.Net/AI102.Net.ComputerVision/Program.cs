using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace AI102.Net.ComputerVision;

class Program
{
    private static string subscriptionKey = string.Empty;
    private static string endpoint = string.Empty;

    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Cognitive service endpoint and key should be passed as arguments");
            Console.ReadKey();
            return;
        }

        endpoint = args[0];
        subscriptionKey = args[1];

        ComputerVisionClient client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey))
        {
            Endpoint = endpoint
        };

        var documentAnalysisClient = new DocumentAnalysisClient(new Uri(endpoint), new Azure.AzureKeyCredential(subscriptionKey));

        IList<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>
        {
            VisualFeatureTypes.Adult,
            VisualFeatureTypes.Brands,
            VisualFeatureTypes.Categories,
            VisualFeatureTypes.Color,
            VisualFeatureTypes.Description,
            VisualFeatureTypes.Faces,
            VisualFeatureTypes.ImageType,
            VisualFeatureTypes.Objects,
            VisualFeatureTypes.Tags
        };

        Task.Run(async () =>
        {
            await AnalyzeImage(client, features, "images/eiffel-tower.jpeg");
            await AnalyzeImageLandmarks(client, "images/eiffel-tower.jpeg");
            await ReadHandwrittenText(client, "images/handwritten-text.png");
            await ReadPrintedText(client, "images/printed_text.jpg");
            await ReadFormText(documentAnalysisClient, "docs/invoice.pdf");
        });

        Console.ReadKey();
    }

    static async Task AnalyzeImage(ComputerVisionClient client,
        IList<VisualFeatureTypes?> visualFeatures,
        string imagePath)
    {
        using StreamReader imageStream = new StreamReader(imagePath);

        var result = await client.AnalyzeImageInStreamAsync(imageStream.BaseStream, visualFeatures);

        Console.WriteLine();
        Console.WriteLine("Adult:");
        Console.WriteLine($"Is adult content: {result.Adult.IsAdultContent}");
        Console.WriteLine($"Is racy content: {result.Adult.IsRacyContent}");
        Console.WriteLine($"Is gory content: {result.Adult.IsGoryContent}");

        Console.WriteLine();
        Console.WriteLine("Brands:");
        if (!result.Brands.Any()) Console.WriteLine("Not found");
        foreach (var brand in result.Brands)
        {
            Console.WriteLine($"{brand.Name} with confidence {brand.Confidence}");
        }

        Console.WriteLine();
        Console.WriteLine("Categories:");
        if (!result.Categories.Any()) Console.WriteLine("Not found");
        foreach (var category in result.Categories)
        {
            Console.WriteLine($"{category.Name} with confidence {category.Score}");
        }

        Console.WriteLine();
        Console.WriteLine("Description:");
        if (!result.Description.Captions.Any()) Console.WriteLine("Not found");
        foreach (var caption in result.Description.Captions)
        {
            Console.WriteLine($"{caption.Text} with confidence {caption.Confidence}");
        }
        foreach (var tag in result.Description.Tags)
        {
            Console.WriteLine($"Tag->{tag}");
        }

        Console.WriteLine();
        Console.WriteLine("Objects:");
        if (!result.Objects.Any()) Console.WriteLine("Not found");
        foreach (var obj in result.Objects)
        {
            Console.WriteLine($"{obj.ObjectProperty} with confidence {obj.Confidence}");
        }

        Console.WriteLine();
        Console.WriteLine("Tags:");
        if (!result.Tags.Any()) Console.WriteLine("Not found");
        foreach (var tag in result.Tags)
        {
            Console.WriteLine($"{tag.Name} with confidence {tag.Confidence}");
        }
    }

    static async Task AnalyzeImageLandmarks(ComputerVisionClient client,
        string imagePath)
    {
        using StreamReader imageStream = new StreamReader(imagePath);

        var result = await client.AnalyzeImageByDomainInStreamAsync("landmarks", imageStream.BaseStream);

        Console.WriteLine();
        Console.WriteLine("Landmarks:");
        Console.WriteLine(result.Result);
    }

    static async Task ReadHandwrittenText(ComputerVisionClient client,
        string imagePath)
    {
        Console.WriteLine("Reading handwritten text...");
        using StreamReader imageStream = new StreamReader(imagePath);

        var result = await client.ReadInStreamAsync(imageStream.BaseStream);

        var operationId = result.OperationLocation.Split('/').Last();

        ReadOperationResult readOperationResult;
        do
        {
            readOperationResult = await client.GetReadResultAsync(Guid.Parse(operationId));
        } while (readOperationResult.Status == OperationStatusCodes.Running ||
                readOperationResult.Status == OperationStatusCodes.NotStarted);

        foreach(var page in readOperationResult.AnalyzeResult.ReadResults)
        {
            foreach(var line in page.Lines)
            {
                Console.WriteLine(line.Text);
            }
        }

        Console.WriteLine();
    }

    static async Task ReadPrintedText(ComputerVisionClient client,
        string imagePath)
    {
        Console.WriteLine("Reading printed text...");
        using StreamReader imageStream = new StreamReader(imagePath);

        var result = await client.RecognizePrintedTextInStreamAsync(true, imageStream.BaseStream);

        foreach (var region in result.Regions)
            foreach (var line in region.Lines)
                foreach (var word in line.Words)
                    Console.WriteLine($"{word.Text} ({word.BoundingBox})");

        Console.WriteLine();
    }

    static async Task ReadFormText(DocumentAnalysisClient client,
        string documentPath)
    {
        Console.WriteLine("Reading form text...");
        using StreamReader documentStream = new StreamReader(documentPath);

        var result = await client.AnalyzeDocumentAsync(Azure.WaitUntil.Completed, "prebuilt-layout", documentStream.BaseStream);

        foreach (var page in result.Value.Pages)
            foreach (var line in page.Lines)
                    Console.WriteLine($"{line.Content}");

        Console.WriteLine();
    }
}

