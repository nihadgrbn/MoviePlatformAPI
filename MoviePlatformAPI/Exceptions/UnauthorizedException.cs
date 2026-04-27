namespace MoviePlatformAPI.Exceptions;

public class UnauthorizedException : Exception
{
    public string ErrorCode { get; }

    public UnauthorizedException(string message = "You are not authorized to perform this action.", string errorCode = "UNAUTHORIZED") 
        : base(message)
    {
        ErrorCode = errorCode;
    }
}