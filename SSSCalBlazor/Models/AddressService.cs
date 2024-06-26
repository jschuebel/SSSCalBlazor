using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Net.Http;
using System.Net.Http.Json;
using System.IO;
using System.Text.Json;
using static SSSCalBlazor.Models.CustomAuthStateProvider;
using static System.Net.WebRequestMethods;
using Blazored.LocalStorage;
using System.Text.Json.Nodes;
using System.Text;

namespace SSSCalBlazor.Models
{
    public interface IAddressService
    {
        Task<Tuple<int, List<AddressModel>>> GetAddress(int currentPage, int pageSize, string sortKey, string sortDirection, string searchString);
        Task<AddressModel> Save(AddressModel addr);
    }
    public class AddressService : IAddressService
    {
        public HttpClient _client { get; set; }
        private readonly ILocalStorageService _localStorage;
        private CommonLib _cmm;

        public int TotalRows = 0;

        public AddressService(ILocalStorageService localStorage, HttpClient client, CommonLib cmm) { _client = client; _localStorage = localStorage; _cmm = cmm; }

        public async Task<Tuple<int, List<AddressModel>>> GetAddress(int currentPage, int pageSize, string sortKey, string sortDirection, string searchString)
        {
            var filterParams = new System.Text.StringBuilder($"page={currentPage}&pageSize={pageSize}&sort[0][field]={sortKey}&sort[0][dir]={sortDirection}");
            if (!string.IsNullOrEmpty(searchString))
                filterParams.Append(searchString);

            //var httpResponse = await _client.GetAsync($"https://www.schuebelsoftware.com/SSSCalWebAPI/api/Address?{filterParams}", HttpCompletionOption.ResponseHeadersRead);
            //var httpResponse = await _client.GetAsync($"https://localhost:5011/api/Address?{filterParams}", HttpCompletionOption.ResponseHeadersRead);
            var httpResponse = await _client.GetAsync($"{_cmm.API_URL}/api/Address?{filterParams}", HttpCompletionOption.ResponseHeadersRead);

            List<AddressModel> lst = null;
            Tuple<int, List<AddressModel>> retVal = null;

            _client.DefaultRequestHeaders.Clear();
            //client.DefaultRequestHeaders.Authorization=new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", savedToken);
            httpResponse.EnsureSuccessStatusCode(); // throws if not 200-299

            if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
            {
                if (httpResponse.Headers.Contains("Paging-TotalRecords"))
                {
                    var hdrRecordCount = httpResponse.Headers.GetValues("Paging-TotalRecords").FirstOrDefault();
                    TotalRows = int.Parse(hdrRecordCount);
                }

                var contentStream = await httpResponse.Content.ReadAsStreamAsync();
                var streamReader = new StreamReader(contentStream);
                lst = JsonSerializer.Deserialize<List<AddressModel>>(streamReader.ReadToEnd());
                retVal = new Tuple<int, List<AddressModel>>(TotalRows, lst);
            }
            return retVal;

        }


        public async Task<AddressModel> Save(AddressModel addr)
        {
            string token = await _localStorage.GetItemAsStringAsync("accesstoken");

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            string jsonString = JsonSerializer.Serialize(addr);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            //var result = await _client.PostAsync($"https://www.schuebelsoftware.com/SSSCalWebAPI/api/Address", content);
            //var result = await _client.PostAsync($"https://localhost:5011/api/Address", content);
            var result = await _client.PostAsync($"{_cmm.API_URL}/api/Address", content);
            
            if (result.IsSuccessStatusCode)
            {
                var tokestr = await result.Content.ReadAsStringAsync();
                addr = JsonSerializer.Deserialize<AddressModel>(tokestr);
            }
            else
            {
                //var rf = result.ReasonPhrase;
                throw new Exception("Problem Saving Address" + result.StatusCode.ToString());
            }
            return addr;
        }

    }
}
