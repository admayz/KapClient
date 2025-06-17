using KapClient.Extender;
using KapClient.KapResponse;
using KapClient.Response;
using RestSharp;
using System.Text.Json;

namespace KapClient
{
    /// <summary>
    /// Client for interacting with the KAP API
    /// Provides methods to retrieve company information, fund data, and disclosure information
    /// </summary>
    public class KapClient
    {
        public string ApiKey { get; set; }
        private string BaseUrl { get; set; } = "https://apigwdev.mkk.com.tr/api/vyk";

        private readonly RestClient _client;

        public KapClient(string apiKey, string? url = null)
        {
            ApiKey = apiKey;
            if (!string.IsNullOrWhiteSpace(url))
                BaseUrl = url;
        }

        #region Token

        // <summary>
        /// Generates authentication token using the provided API key
        /// </summary>
        /// <returns>Authentication token string</returns>
        /// <exception cref="Exception">Thrown when token generation fails</exception>
        private async Task<string> GetTokenAsync()
        {
            var url = $@"https://apigwdev.mkk.com.tr/auth/generateToken?apiKey={ApiKey}";

            try
            {
                var options = new RestClientOptions(url);
                var client = new RestClient(options);

                var request = new RestRequest("");
                var response = await client.GetAsync(request);

                if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(response.Content))
                {
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(response.Content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (tokenResponse?.Token != null)
                        return tokenResponse.Token;
                }

                throw new Exception("Token response is invalid or empty");
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while generating token", ex);
            }
        }

        #endregion

        #region Base

        /// <summary>
        /// Makes authenticated HTTP GET request to the specified endpoint
        /// </summary>
        /// <param name="url">API endpoint URL</param>
        /// <returns>REST response object</returns>
        /// <exception cref="Exception">Thrown when request fails</exception>
        private async Task<RestResponse?> GetBaseAsync(string url)
        {
            try
            {
                var options = new RestClientOptions($"{BaseUrl}/{url}");
                var client = new RestClient(options);
                var token = await GetTokenAsync();

                var request = new RestRequest("");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", $"Basic {token}");

                var response = await client.GetAsync(request);

                if (response == null)
                    throw new Exception("Response is null");

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving data", ex);
            }
        }

        #endregion

        #region Company

        /// <summary>
        /// Retrieves list of all companies from KAP API
        /// </summary>
        /// <returns>List of Company objects</returns>
        /// <exception cref="Exception">Thrown when company data retrieval fails</exception>
        public async Task<List<Company>> GetCompanyListAsync()
        {
            try
            {
                var response = await GetBaseAsync("members");

                if (string.IsNullOrEmpty(response.Content))
                    return new List<Company>();

                var companyResponse = JsonSerializer.Deserialize<List<CompanyResponse>>(response.Content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (companyResponse == null)
                    return new List<Company>();

                return companyResponse
                    .Where(item => item != null)
                    .Select(item => new Company
                    {
                        Id = int.TryParse(item.Id, out int id) ? id : 0,
                        Name = item.Title?.Trim() ?? "",
                        Code = item.StockCode?.Trim() ?? "",
                        Codes = string.IsNullOrWhiteSpace(item.StockCode)
                            ? new List<Code>()
                            : item.StockCode.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(code => new Code { Name = code.Trim() })
                                .ToList(),
                        Type = item.MemberType?.Trim() ?? "",
                        Types = string.IsNullOrWhiteSpace(item.MemberType)
                            ? new List<Response.Type>()
                            : item.MemberType.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => new Response.Type { Name = t.Trim() })
                                .ToList(),
                        Url = item.KfifUrl ?? ""
                    }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving company data", ex);
            }
        }

        // <summary>
        /// Retrieves a specific company by its ID
        /// </summary>
        /// <param name="id">Company ID</param>
        /// <returns>Company object or null if not found</returns>
        /// <exception cref="Exception">Thrown when company data retrieval fails</exception>
        public async Task<Company?> GetCompanyByIdAsync(int id)
        {
            try
            {
                var companyList = await GetCompanyListAsync();
                return companyList.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving company data", ex);
            }
        }

        /// <summary>
        /// Retrieves a specific company by its stock code
        /// </summary>
        /// <param name="code">Stock code</param>
        /// <returns>Company object or null if not found</returns>
        /// <exception cref="Exception">Thrown when company data retrieval fails</exception>
        public async Task<Company?> GetCompanyByCodeAsync(string code)
        {
            try
            {
                var companyList = await GetCompanyListAsync();
                return companyList.FirstOrDefault(c => c.Codes.Any(codeItem => codeItem.Name == code));
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving company data", ex);
            }
        }

        #region Company Detail

        /// <summary>
        /// Retrieves detailed information for a specific company including financial and operational data
        /// </summary>
        /// <param name="id">Company ID to get details for</param>
        /// <returns>List of CompanyDetail objects containing detailed company information</returns>
        /// <exception cref="Exception">Thrown when company detail retrieval fails</exception>
        public async Task<List<CompanyDetail>> GetCompanyDetailAsync(int id)
        {
            try
            {
                var response = await GetBaseAsync("memberDetail/" + id);

                if (string.IsNullOrEmpty(response.Content))
                    return new List<CompanyDetail>();

                var companyDetailResponse = JsonSerializer.Deserialize<List<CompanyDetailResponse>>(response.Content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (companyDetailResponse == null)
                    return new List<CompanyDetail>();

                return companyDetailResponse
                    .Where(item => item != null)
                    .Select(item => new CompanyDetail
                    {
                        Name = new Name
                        {
                            Tr = item.NameTr?.Trim() ?? "",
                            En = item.NameEn.Trim() ?? ""
                        },
                        Key = item.Key?.Trim() ?? "",
                        PublishDateTime = item.PublishDateTime?.ToNullableDateTime(),
                        Value = ValueExtender.NormalizeValue(item.Value)
                    }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving company detail data", ex);
            }
        }

        #endregion

        #region Company Securities

        /// <summary>
        /// Retrieves securities information for all companies including stock codes, ISIN numbers, and trading information
        /// </summary>
        /// <returns>List of CompanySecurities objects containing securities data</returns>
        /// <exception cref="Exception">Thrown when securities data retrieval fails</exception>
        public async Task<List<CompanySecurities>> GetCompanySecuritiesAsync()
        {
            try
            {
                var response = await GetBaseAsync("memberSecurities");

                if (string.IsNullOrEmpty(response.Content))
                    return new List<CompanySecurities>();

                var companySecuritiesResponse = JsonSerializer.Deserialize<List<CompanySecuritiesResponse>>(response.Content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (companySecuritiesResponse == null)
                    return new List<CompanySecurities>();

                return companySecuritiesResponse
                    .Where(item => item != null)
                    .Select(s => new CompanySecurities
                    {
                        Data = new Data
                        {
                            Id = int.TryParse(s.Member.Id, out var idParsed) ? idParsed : 0,
                            MemberType = s.Member.MemberType,
                            CapitalSystem = s.Member.SermayeSistemi,
                            RegisteredCapitalCeiling = s.Member.KayitliSermayeTavani,
                            KstExpiryDate = DateTime.TryParse(s.Member.KstSonGecerlilikTarihi, out var dt) ? dt : (DateTime?)null,
                            CompanyTitle = s.Member.SirketUnvan,
                            MksMbrId = s.Member.MksMbrId,
                            Items = s.Securities?.Select(sec => new Item
                            {
                                Isin = sec.Isin,
                                IsinDescription = sec.IsinDesc,
                                ExchangeCode = sec.BorsaKodu,
                                SwapCode = sec.TakasKodu,
                                TertipGroup = sec.TertipGroup,
                                Capital = sec.Capital,
                                CurrentCapital = sec.CurrentCapital,
                                GroupCode = sec.GroupCode,
                                GroupCodeDescription = sec.GroupCodeDesc,
                                ExchangeTradingOpen = sec.BorsadaIslemeAcik
                            }).ToList() ?? new List<Item>()
                        }
                    }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving company securities data", ex);
            }
        }

        /// <summary>
        /// Retrieves securities information for a specific company by its ID
        /// </summary>
        /// <param name="id">Company ID to get securities information for</param>
        /// <returns>CompanySecurities object or null if not found</returns>
        /// <exception cref="Exception">Thrown when securities data retrieval fails</exception>
        public async Task<CompanySecurities?> GetCompanySecuritiesByIdAsync(int id)
        {
            try
            {
                var companySecuritiesList = await GetCompanySecuritiesAsync();
                return companySecuritiesList.FirstOrDefault(c => c.Data.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving company securities data", ex);
            }
        }

        #endregion

        #endregion

        #region Fund

        /// <summary>
        /// Retrieves list of funds with optional filtering parameters for state, class, and type
        /// </summary>
        /// <param name="fundState">Fund state filter (e.g., "ACTIVE", "INACTIVE") - optional</param>
        /// <param name="fundClass">Fund class filter (e.g., "A", "B") - optional</param>
        /// <param name="fundType">Fund type filter (e.g., "EQUITY", "BOND") - optional</param>
        /// <returns>List of Fund objects containing fund information</returns>
        /// <exception cref="Exception">Thrown when fund data retrieval fails</exception>
        public async Task<List<Fund>> GetFundListAsync(string? fundState = null, string? fundClass = null, string? fundType = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(fundState)) queryParams.Add($"fundState={fundState}");
                if (!string.IsNullOrEmpty(fundClass)) queryParams.Add($"fundClass={fundClass}");
                if (!string.IsNullOrEmpty(fundType)) queryParams.Add($"fundType={fundType}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await GetBaseAsync($"funds{queryString}");

                if (string.IsNullOrEmpty(response.Content))
                    return new List<Fund>();

                var fundsResponse = JsonSerializer.Deserialize<List<FundReponse>>(response.Content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (fundsResponse == null)
                    return new List<Fund>();

                return fundsResponse
                    .Where(item => item != null)
                    .Select(s => new Fund
                    {
                        Id = s.FundId,
                        Name = s.FundName,
                        Code = s.FundCode,
                        Type = s.FundType,
                        Class = s.FundClass,
                        Expiry = s.FundExpiry,
                        State = s.FundState,
                        UmbMemberType = s.UmbMemberTypes,
                        UmbMemberTypes = string.IsNullOrWhiteSpace(s.UmbMemberTypes)
                            ? new List<MemberType>()
                            : s.UmbMemberTypes.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(code => new MemberType { Name = code.Trim() })
                                .ToList(),
                        MemberType = s.FundMemberTypes,
                        MemberTypes = string.IsNullOrWhiteSpace(s.FundMemberTypes)
                            ? new List<MemberType>()
                            : s.FundMemberTypes.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(code => new MemberType { Name = code.Trim() })
                                .ToList(),
                        Url = s.KapUrl,
                        NonInactiveCount = s.NonInactiveCount,
                        CompanyId = int.TryParse(s.FundCompanyId ?? "", out var companyId) ? companyId : 0,
                        CompanyTitle = s.FundCompanyTitle
                    }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving fund data", ex);
            }
        }

        /// <summary>
        /// Retrieves a specific fund by its ID
        /// </summary>
        /// <param name="id">Fund ID to search for</param>
        /// <returns>Fund object or null if not found</returns>
        /// <exception cref="Exception">Thrown when fund data retrieval fails</exception>
        public async Task<Fund?> GetFundByIdAsync(int id)
        {
            try
            {
                var fundList = await GetFundListAsync();
                return fundList.FirstOrDefault(f => f.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving fund data", ex);
            }
        }

        /// <summary>
        /// Retrieves a specific fund by the company ID that manages it
        /// </summary>
        /// <param name="id">Company ID that manages the fund</param>
        /// <returns>Fund object or null if not found</returns>
        /// <exception cref="Exception">Thrown when fund data retrieval fails</exception>
        public async Task<Fund?> GetFundByCompanyIdAsync(int id)
        {
            try
            {
                var fundList = await GetFundListAsync();
                return fundList.FirstOrDefault(f => f.CompanyId == id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving fund data", ex);
            }
        }

        #region Fund Detail

        /// <summary>
        /// Retrieves detailed information for a specific fund including performance metrics and holdings
        /// </summary>
        /// <param name="id">Fund ID to get details for</param>
        /// <returns>List of FundDetailResponse objects containing detailed fund information</returns>
        /// <exception cref="Exception">Thrown when fund detail retrieval fails</exception>
        public async Task<List<FundDetailResponse>> GetFundDetailAsync(int id)
        {
            try
            {
                var response = await GetBaseAsync("fundDetail/" + id);

                if (string.IsNullOrEmpty(response.Content))
                    return new List<FundDetailResponse>();

                var fundDetailResponse = JsonSerializer.Deserialize<List<FundDetailResponse>>(response.Content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return fundDetailResponse ?? new List<FundDetailResponse>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving fund detail data", ex);
            }
        }

        #endregion

        #endregion

        #region Disclosure

        /// <summary>
        /// Retrieves the last disclosure index number from the KAP system
        /// This is used to track the most recent disclosure ID for incremental data retrieval
        /// </summary>
        /// <returns>Last disclosure index number as integer</returns>
        /// <exception cref="Exception">Thrown when last disclosure ID retrieval fails</exception>
        public async Task<int> GetLastDisclosureId()
        {
            try
            {
                var response = await GetBaseAsync("lastDisclosureIndex");

                if (string.IsNullOrEmpty(response.Content))
                    return 0;

                var lastDisclosureIdResponse = JsonSerializer.Deserialize<LastDisclosureIdResponse>(response.Content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (lastDisclosureIdResponse == null)
                    return 0;

                return Convert.ToInt32(lastDisclosureIdResponse.LastDisclosureIndex.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving last disclosure ID", ex);
            }
        }

        /// <summary>
        /// Retrieves list of disclosures based on specified criteria including disclosure index, type, class, and company
        /// </summary>
        /// <param name="disclosureIndex">Starting disclosure index number for retrieval</param>
        /// <param name="disclosureType">Type of disclosure (e.g., "TR" for Turkish, "EN" for English) - optional</param>
        /// <param name="disclosureClass">Class of disclosure (e.g., "ODA" for special situation disclosures) - optional</param>
        /// <param name="companyId">Specific company ID to filter disclosures - optional</param>
        /// <returns>List of disclosure objects containing disclosure information</returns>
        /// <exception cref="Exception">Thrown when disclosure data retrieval fails</exception>
        public async Task<List<DisclosureResponse>> GetDisclosureListAsync(int id, string? type = "TR", string? className = "ODA", int? companyId = null)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"disclosureIndex={id}"
                };

                if (!string.IsNullOrEmpty(type))
                    queryParams.Add($"disclosureTypes={type}");

                if (!string.IsNullOrEmpty(className))
                    queryParams.Add($"disclosureClass={className}");

                if (companyId.HasValue)
                    queryParams.Add($"companyId={companyId.Value}");

                var queryString = "?" + string.Join("&", queryParams);
                var response = await GetBaseAsync($"disclosures{queryString}");

                if (string.IsNullOrEmpty(response.Content))
                    return new List<DisclosureResponse>();

                var disclosureResponse = JsonSerializer.Deserialize<List<DisclosureResponse>>(response.Content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return disclosureResponse ?? new List<DisclosureResponse>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving disclosure data", ex);
            }
        }

        #endregion
    }
}