namespace KapClient.Response
{
    public sealed class CompanySecurities
    {
        public Data Data { get; set; } = new Data();
    }

    public sealed class Data
    {
        public int Id { get; set; }

        public string MemberType { get; set; } = string.Empty;

        public string CapitalSystem { get; set; } = string.Empty;

        public double RegisteredCapitalCeiling { get; set; }

        public DateTime? KstExpiryDate { get; set; }

        public string CompanyTitle { get; set; } = string.Empty;

        public string MksMbrId { get; set; } = string.Empty;

        public List<Item> Items { get; set; } = new List<Item>();
    }

    public sealed class Item
    {
        public string Isin { get; set; } = string.Empty;

        public string IsinDescription { get; set; } = string.Empty;

        public string ExchangeCode { get; set; } = string.Empty;

        public string SwapCode { get; set; } = string.Empty;

        public string TertipGroup { get; set; } = string.Empty;

        public double Capital { get; set; }

        public double CurrentCapital { get; set; }

        public string GroupCode { get; set; } = string.Empty;

        public string GroupCodeDescription { get; set; } = string.Empty;

        public bool ExchangeTradingOpen { get; set; }
    }
}