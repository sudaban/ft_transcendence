
namespace Backend.Application.Exceptions
{
    public class BadRequestException : BaseException
    {
        public BadRequestException()
            : base("Bad Request", 400, "Incomplete or extra data was entered")
        { }

        public BadRequestException(string message)
            : base(message, 400, "Incomplete or extra data was entered")
        {}
    }
}
