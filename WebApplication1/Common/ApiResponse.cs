namespace WebApplication1.Common;

public record ApiResponse<T>
{
    public T Data { get; set; }
    public string Message { get; set; }
    public int Code { get; set; }
}