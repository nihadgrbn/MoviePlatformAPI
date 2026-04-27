namespace MoviePlatformAPI.Exceptions;

public class NotFoundException : Exception
{
    public string ErrorCode { get; }

    public NotFoundException(string message = "The requested resource was not found.", string errorCode = "NOT_FOUND") 
        : base(message)
    {
        ErrorCode = errorCode;
    }
}