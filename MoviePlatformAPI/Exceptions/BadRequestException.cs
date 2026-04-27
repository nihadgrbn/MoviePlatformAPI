namespace MoviePlatformAPI.Exceptions;

public class BadRequestException : Exception
{
    public string ErrorCode { get; }

    public BadRequestException(string message = "Bad request. Please check your input.", string errorCode = "BAD_REQUEST") 
        : base(message)
    {
        ErrorCode = errorCode;
    }
}