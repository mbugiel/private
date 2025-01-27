namespace ManagemateAPI.Controllers
{
    public class Api_Response
    {
        public string code { get; set; }
        public string message { get; set; }
        public object? responseData { get; set; }
    }

    public enum ResponseType
    {
        Success,
        NotFound
    }
}
