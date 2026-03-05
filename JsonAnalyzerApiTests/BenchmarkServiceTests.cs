using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using Xunit;

namespace JsonAnalyzerApiTest;

public class BenchmarkServiceTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void Run_Benchmarks()
    {
        var logger = new AccumulationLogger();

        var config = ManualConfig.Create(DefaultConfig.Instance)
            .AddLogger(logger)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator);

        BenchmarkRunner.Run<MemoryBenchmarkService>(config);

        // write benchmark summary
        output.WriteLine(logger.GetLog());
    }
}

[MemoryDiagnoser]
public class MemoryBenchmarkService
{
    private MemoryStream? _resultStream = null;
    private string _outputFilePath = string.Empty;
    private const int RepeatCount = 100;

    [GlobalSetup]
    public void Setup()
    {
        var inputFilePath = "C:\\Users\\spola\\Documents\\repos\\JsonAnalyzerApi\\JsonAnalyzerApiTests\\TestData\\Test2_valid2from3.json";
        _outputFilePath = "C:\\Users\\spola\\Documents\\repos\\JsonAnalyzerApi\\JsonAnalyzerApiTests\\benchmarkout.json";

        byte[] originalBytes = File.ReadAllBytes(inputFilePath);
        long totalSize = (long)originalBytes.Length * RepeatCount;
        _resultStream = new MemoryStream((int)totalSize);

        for (int i = 0; i < RepeatCount; i++)
        {
            _resultStream.Write(originalBytes, 0, originalBytes.Length);
        }

        // Reset position for reading
        _resultStream.Position = 0;
    }

    [Benchmark]
    public async Task ProcessRequest_Benchmark()
    {
        try
        {
            await JsonAnalyzerApi.Services.Orchestrator.ExtractValidObjectsAsync(
                _resultStream!, _outputFilePath, TestContext.Current.CancellationToken);
        }
        finally
        {
            _resultStream?.Dispose();
        }
    }
}

