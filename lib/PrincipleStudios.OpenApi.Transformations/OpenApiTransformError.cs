namespace PrincipleStudios.OpenApi.Transformations
{
    public class OpenApiTransformError
    {
        public OpenApiTransformError(string context, string message)
        {
            Context = context;
            Message = message;
        }

        public string Context { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{Context}: {Message}";
        }
    }
}