namespace KapClient.KapResponse
{
    public sealed class FundReponse
    {
        public int FundId { get; set; }

        public string FundName { get; set; } = string.Empty;

        public string FundCode { get; set; } = string.Empty;

        public string FundType { get; set; } = string.Empty;

        public string FundClass { get; set; } = string.Empty;

        public string FundExpiry { get; set; } = string.Empty;

        public string FundState { get; set; } = string.Empty;

        public string UmbMemberTypes { get; set; } = string.Empty;

        public string FundMemberTypes { get; set; } = string.Empty;

        public string KapUrl { get; set; } = string.Empty;

        public int NonInactiveCount { get; set; }

        public string FundCompanyId { get; set; } = string.Empty;

        public string FundCompanyTitle { get; set; } = string.Empty;
    }
}