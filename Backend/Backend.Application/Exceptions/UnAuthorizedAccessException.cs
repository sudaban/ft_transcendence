
namespace Backend.Application.Exceptions
{
    public class UnAuthorizedAccessException : BaseException
    {
        public UnAuthorizedAccessException()
            : base("You do not have permission to perform this operation.", 403, "The permission level is insufficient")
        {}

        public UnAuthorizedAccessException(string message)
            : base(message, 403, "You do not have permission to perform this operation.")
        {}
    }
}
