using JsonAnalyzerApi.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Xunit;


namespace JsonAnalyzerApiTest;

public class ControllerTests() : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private HttpClient? _client;

    public ValueTask InitializeAsync()
    {
        _client = _factory.CreateClient();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _client?.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    [Theory]
    [InlineData("Test0_valid0.json", MediaTypeNames.Application.ProblemJson)]
    [InlineData("Test1_valid3.json", MediaTypeNames.Application.Octet)]
    [InlineData("Test2_valid2from3.json", MediaTypeNames.Application.Json)]
    public async Task AttachedFile_Success(string fileName, string contentType)
    {
        var postUri = new Uri(_client!.BaseAddress!, File_Controller.RouteName);
        var parts = Environment.ProcessPath!.Split(["bin"], StringSplitOptions.None);
        var inputFilePath = Path.Combine(parts[0], $"TestData\\{fileName}");
        using var fileStream = File.OpenRead(inputFilePath);
        using var formData = new MultipartFormDataContent();

        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        // Add file to form data
        formData.Add(streamContent, "file", fileName);

        var postResponse = await _client.PostAsync(postUri, formData, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

        // here we should check output from processing
    }

    [Theory]
    [InlineData("Test0_valid0.json")]
    [InlineData("Test1_valid3.json")]
    [InlineData("Test2_valid2from3.json")]
    public async Task JsonData_Success(string fileName)
    {
        var postUri = new Uri(_client!.BaseAddress!, File_Controller.RouteName);
        var parts = Environment.ProcessPath!.Split(["bin"], StringSplitOptions.None);
        var inputFilePath = Path.Combine(parts[0], $"TestData\\{fileName}");
        var jsonString = File.ReadAllText(inputFilePath);

        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

        var postResponse = await _client.PostAsync(postUri, content, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

        // here we should check output from processing
    }
}