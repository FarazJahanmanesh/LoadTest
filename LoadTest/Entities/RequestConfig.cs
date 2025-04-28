using System.Text;

namespace LoadTest.Entities;

public class RequestConfig
{
    public string Url { get; set; }
    public string? ContentType { get; set; }
    public Dictionary<string, string>? Payload { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public HttpMethod HttpMethodType { get; set; }
    public Encoding? EncodingType { get; set; }
}
