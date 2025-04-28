using System.Text;

namespace LoadTest.Entities;

public class RequestConfig
{
    public string Url { get; set; }
    public string? Token { get; set; }
    public object? Payload { get; set; }
    public string? Authorization { get; set; }
    public HttpMethod HttpMethodType { get; set; }
    public Encoding? EncodingType { get; set; }
}
