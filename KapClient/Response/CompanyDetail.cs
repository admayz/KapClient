namespace KapClient.Response
{
    public sealed class CompanyDetail
    {
        public Name Name { get; set; } = new Name();

        public string Key { get; set; } = string.Empty;

        public DateTime? PublishDateTime { get; set; } = null;

        public object? Value { get; set; } = new object();
    }

    public sealed class Name
    {
        public string Tr { get; set; } = string.Empty;

        public string En { get; set; } = string.Empty;
    }
}