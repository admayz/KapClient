namespace KapClient.KapResponse
{
    public sealed class FundDetailResponse
    {
        public string NameTr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string PublishDateTime { get; set; } = string.Empty;
        public object Value { get; set; } = new object();
    }
}