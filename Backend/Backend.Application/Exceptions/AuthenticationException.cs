namespace Backend.Application.Exceptions;

public class AuthenticationException : BaseException
{
    public AuthenticationException() 
        : base("User is not authenticated.", 401, "Authentication Error")
    {
    }

    public AuthenticationException(string message) 
        : base(message, 401, "Authentication Error")
    {
    }
}
