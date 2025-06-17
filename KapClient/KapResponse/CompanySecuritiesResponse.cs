namespace KapClient.KapResponse
{
    public sealed class CompanySecuritiesResponse
    {
        public Member Member { get; set; } = new Member();

        public List<Security> Securities { get; set; } = new List<Security>();
    }

    public sealed class Member
    {
        public string Id { get; set; } = string.Empty;

        public string MemberType { get; set; } = string.Empty;

        public string SermayeSistemi { get; set; } = string.Empty;

        public double KayitliSermayeTavani { get; set; }

        public string KstSonGecerlilikTarihi { get; set; } = string.Empty;

        public string SirketUnvan { get; set; } = string.Empty;

        public string MksMbrId { get; set; } = string.Empty;
    }

    public sealed class Security
    {
        public string Isin { get; set; } = string.Empty;

        public string IsinDesc { get; set; } = string.Empty;

        public string BorsaKodu { get; set; } = string.Empty;

        public string TakasKodu { get; set; } = string.Empty;

        public string TertipGroup { get; set; } = string.Empty;

        public double Capital { get; set; }

        public double CurrentCapital { get; set; }

        public string GroupCode { get; set; } = string.Empty;

        public string GroupCodeDesc { get; set; } = string.Empty;

        public bool BorsadaIslemeAcik { get; set; }
    }
}