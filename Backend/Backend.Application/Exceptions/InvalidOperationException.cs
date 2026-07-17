namespace Backend.Application.Exceptions;

public class InvalidOperationException : BaseException
{
    public InvalidOperationException() 
        : base("An invalid operation occurred.", 400, "Invalid Operation")
    {
    }

    public InvalidOperationException(string message) 
        : base(message, 400, "Invalid Operation")
    {
    }
}
