namespace Generators;

public static class NameGenerator
{
    private static readonly Random rand = new Random();

    private static readonly string directoryPath =
        Path.Combine(Directory.GetCurrentDirectory(), "Resources");

    private static readonly List<string> firstNames = LoadNames("FirstNames.txt");
    private static readonly List<string> lastNames = LoadNames("LastNames.txt");

    private static List<string> LoadNames(string fileName)
    {
        var names = new List<string>(File.ReadAllLines(Path.Combine(directoryPath, fileName)));
        names.RemoveAll(string.IsNullOrWhiteSpace);
        return names;
    }

    public static string GenerateName()
    {
        if (firstNames.Count == 0 || lastNames.Count == 0)
            return $"Error Name";

        string firstName = firstNames[rand.Next(firstNames.Count)];
        string lastName = lastNames[rand.Next(lastNames.Count)];

        return $"{firstName} {lastName}";
    }
}