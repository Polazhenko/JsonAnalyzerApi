using System.Buffers;
using System.IO.Pipelines;
using System.Text.Json;

namespace JsonAnalyzerApi.Services;

public static class Orchestrator
{
    public const int FileBufferSize = 10 * 1024 * 1024; // 10 MB

    public static async Task<(bool Result, string Error)> ExtractValidObjectsAsync(
        Stream input,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var output = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, FileBufferSize, useAsync: true);
            var reader = PipeReader.Create(input);
            var writer = new Utf8JsonWriter(output, new JsonWriterOptions
            {
                Indented = false,
                SkipValidation = true
            });

            writer.WriteStartArray();

            int braceDepth = 0;
            bool insideString = false;
            bool escape = false;

            var objectBuffer = new ArrayBufferWriter<byte>();

            while (true)
            {
                var result = await reader.ReadAsync(cancellationToken);
                var buffer = result.Buffer;

                foreach (var segment in buffer)
                {
                    var span = segment.Span;

                    for (int i = 0; i < span.Length; i++)
                    {
                        byte b = span[i];

                        if (braceDepth == 0)
                        {
                            if (b == (byte)'{')
                            {
                                braceDepth = 1;
                                objectBuffer.Clear();
                                objectBuffer.Write([b]);
                            }
                            continue;
                        }

                        objectBuffer.Write([b]);

                        if (insideString)
                        {
                            if (escape)
                                escape = false;
                            else if (b == (byte)'\\')
                                escape = true;
                            else if (b == (byte)'"')
                                insideString = false;

                            continue;
                        }

                        if (b == (byte)'"')
                            insideString = true;
                        else if (b == (byte)'{')
                            braceDepth++;
                        else if (b == (byte)'}')
                        {
                            braceDepth--;

                            if (braceDepth == 0)
                            {
                                ProcessCandidate(objectBuffer.WrittenSpan, writer);
                            }
                        }
                    }
                }

                reader.AdvanceTo(buffer.End);

                if (result.IsCompleted)
                    break;
            }

            writer.WriteEndArray();
            await writer.FlushAsync(cancellationToken);
            await reader.CompleteAsync();
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
           return (false, ex.Message);
        }
    }

    private static void ProcessCandidate(
        ReadOnlySpan<byte> candidate,
        Utf8JsonWriter writer)
    {
        var jsonReader = new Utf8JsonReader(candidate, new JsonReaderOptions
        {
            CommentHandling = JsonCommentHandling.Disallow,
            AllowTrailingCommas = false
        });

        try
        {
            while (jsonReader.Read()) { }

            writer.WriteRawValue(candidate, skipInputValidation: true);
        }
        catch (JsonException)
        {
            // truncated / invalid object → ignored
        }
    }
}