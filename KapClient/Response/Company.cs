namespace KapClient.Response
{
    public sealed class Company
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public List<Code> Codes { get; set; } = new List<Code>();

        public string Type { get; set; } = string.Empty;

        public List<Type> Types { get; set; } = new List<Type>();

        public string Url { get; set; } = string.Empty;
    }

    public sealed class Code
    {
        public string Name { get; set; } = string.Empty;
    }

    public sealed class Type
    {
        public string Name { get; set; } = string.Empty;
    }
}