namespace Backend.Application.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException()
            : base("Not Found", 404, "Resource not found")
        {}

        public NotFoundException(string message)
            : base(message, 404, "Resource not found")
        {}
    }
}
