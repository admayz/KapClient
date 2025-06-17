namespace KapClient.Response
{
    public sealed class Fund
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Class { get; set; } = string.Empty;

        public string Expiry { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string UmbMemberType { get; set; } = string.Empty;

        public List<MemberType> UmbMemberTypes { get; set; } = new List<MemberType>();

        public string MemberType { get; set; } = string.Empty;

        public List<MemberType> MemberTypes { get; set; } = new List<MemberType>();

        public string Url { get; set; } = string.Empty;

        public int NonInactiveCount { get; set; }

        public int CompanyId { get; set; }

        public string CompanyTitle { get; set; } = string.Empty;
    }

    public sealed class MemberType
    {
        public string Name { get; set; }
    }
}