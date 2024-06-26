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
    public interface IEventService
    {
        Task<Tuple<int, List<EventModel>>> GetEvent(int currentPage, int pageSize, string sortKey, string sortDirection, string searchString);
        Task<EventModel> Save(EventModel person);
    }
    public class EventService : IEventService
    {
        public HttpClient _client { get; set; }
        private readonly ILocalStorageService _localStorage;
        private CommonLib _cmm;

        public int TotalRows = 0;

        public EventService(ILocalStorageService localStorage, HttpClient client, CommonLib cmm) { _client = client; _localStorage = localStorage; _cmm = cmm; }

        public async Task<EventModel> Save(EventModel person)
        {
            string token = await _localStorage.GetItemAsStringAsync("accesstoken");

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            
            string jsonString = JsonSerializer.Serialize(person);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            //var result = await _client.PutAsync($"https://www.schuebelsoftware.com/SSSCalWebAPI/api/person/{person.id}", content);
            //var result = await _client.PutAsync($"https://localhost:5011/api/person/{person.id}", content);
            var result = await _client.PutAsync($"{_cmm.API_URL}/api/person/{person.id}", content);

            var tokestr = await result.Content.ReadAsStringAsync();
            if (result.IsSuccessStatusCode)
            {
                person = JsonSerializer.Deserialize<EventModel>(tokestr);
            }
            else
            {
                throw new Exception(tokestr);
            }
            return person;
        }


        public async Task<Tuple<int, List<EventModel>>> GetEvent(int currentPage, int pageSize, string sortKey, string sortDirection, string searchString)
        {
            var filterParams = new System.Text.StringBuilder($"page={currentPage}&pageSize={pageSize}&sort[0][field]={sortKey}&sort[0][dir]={sortDirection}");
            if (!string.IsNullOrEmpty(searchString))
                filterParams.Append(searchString);
            _client.DefaultRequestHeaders.Clear();
            //var httpResponse = await _client.GetAsync($"https://www.schuebelsoftware.com/SSSCalWebAPI/api/event?{filterParams}", HttpCompletionOption.ResponseHeadersRead);
            //var httpResponse = await _client.GetAsync($"https://localhost:5011/api/event?{filterParams}", HttpCompletionOption.ResponseHeadersRead);
            var httpResponse = await _client.GetAsync($"{_cmm.API_URL}/api/event?{filterParams}", HttpCompletionOption.ResponseHeadersRead);
            List<EventModel> lst = null;
            Tuple<int, List<EventModel>> retVal = null;

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
                lst= JsonSerializer.Deserialize<List<EventModel>>(streamReader.ReadToEnd());
                retVal = new Tuple<int, List<EventModel>>(TotalRows, lst);
            }
            return retVal;

        }
    }
}
