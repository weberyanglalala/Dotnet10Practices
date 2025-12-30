namespace WebApplication1.Common;

public class OperationResult<T>
{
    public bool IsSuccess { get; }
    public T Data { get; }
    public string ErrorMessage { get; }
    public int Code { get; }

    private OperationResult(bool isSuccess, T data, string errorMessage, int code)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        Code = code;
    }

    public static OperationResult<T> Success(T data, int statusCode = 200)
    {
        return new OperationResult<T>(true, data, null, statusCode);
    }

    public static OperationResult<T> Failure(string errorMessage = "Operation Failed.", int statusCode = 400)
    {
        return new OperationResult<T>(false, default, errorMessage, statusCode);
    }
}