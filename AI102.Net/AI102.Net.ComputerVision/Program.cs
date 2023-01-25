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

        string imagePath = "images/eiffel-tower.jpeg";

        Task.Run(async () =>
        {
            await AnalyzeImage(client, features, imagePath);
            await AnalyzeImageLandmarks(client, imagePath);
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
}

