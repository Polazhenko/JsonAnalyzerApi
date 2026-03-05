using JsonAnalyzerApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;


namespace JsonAnalyzerApi.Controllers;

[ApiController]
[Route(FileController.RouteName)]
public class FileController() : ControllerBase
{
    private const string Multipart = "multipart/form-data";
    public const string RouteName = "CreateFile";

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json, Multipart, MediaTypeNames.Application.Octet)]
    public async Task<IActionResult> Create_A_FileAsync()
    {
        try
        {
            if (!TryGetContent(out var input))
            {
                return BadRequest("Wrong input data (neither atached json file or json body message).");
            }

            var fileName = $"validObjects-{Guid.NewGuid()}.json";
            var (result, error) = await Orchestrator.ExtractValidObjectsAsync(input, fileName);
            if (result)
            {
                return Ok($"Objects saved to {fileName}.");
            }

            return BadRequest(error);
        }
        catch (Exception ex)
        {
            return BadRequest("Internal error: " + ex.Message);
        }
    }

    private bool TryGetContent(out Stream x)
    {
        x = Stream.Null;
        if (Request is { HasFormContentType: true, Form.Files.Count: 1 })
        {
            x = Request.Form.Files[0].OpenReadStream();
            return true;
        }

        if (Request.ContentType?.Contains(MediaTypeNames.Application.Json) != false)
        {
            x = Request.Body;
            return true;
        }

        return false;
    }
    
    [HttpGet]
    public IActionResult HelloWorld()
    {
        return Ok("Hello World!");
    }
}