using Xunit;

namespace JsonAnalyzerApiTest;

public class ServiceTests
{
    [Theory]
    [InlineData("Test0_valid0.json", "validObjects0.json")]
    [InlineData("Test1_valid3.json", "validObjects1.json")]
    [InlineData("Test2_valid2from3.json", "validObjects2.json")]
    public async Task ProcessJsonFile_Success(string inputFileName, string outputFileName)
    {
        var parts = Environment.ProcessPath!.Split(["bin"], StringSplitOptions.None);
        var inputFilePath = Path.Combine(parts[0], $"TestData\\{inputFileName}");
        var outputFilePath = Path.Combine(parts[0], outputFileName);
        using var inputStream = File.OpenRead(inputFilePath);
        var (result, _) = await JsonAnalyzerApi.Services.Orchestrator.ExtractValidObjectsAsync(inputStream, outputFilePath, TestContext.Current.CancellationToken);

        Assert.True(result);
    }
}
