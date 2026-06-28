namespace Backend.Application.Exceptions
{
    public class BaseException : Exception
    {
        public int StatusCode { get; set; }

        public string Title { get; set; }

        public BaseException(string message
            , int statusCode, string title) : base(message)
        {
            StatusCode = statusCode;
            Title = title;
        }
    }
}
