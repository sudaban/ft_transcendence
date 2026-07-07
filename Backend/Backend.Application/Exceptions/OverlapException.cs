
namespace Backend.Application.Exceptions
{
    public class OverlapException : BaseException
    {
        public OverlapException()
            : base("Overlap detected", 400, "The same data was found")
        {}

        public OverlapException(string message)
            : base(message, 400, "The same data was found")
        {}
    }
}
