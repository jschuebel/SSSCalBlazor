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
    public interface IPersonService
    {
        Task<Tuple<int, List<PeopleModel>>> GetPeople(int currentPage, int pageSize, string sortKey, string sortDirection, string searchString);
        Task<PeopleModel> SavePerson(PeopleModel person);
        Task<EventModel> GetDOB(int userID);
    }
    public class PersonService : IPersonService
    {
        public HttpClient _client { get; set; }
        private readonly ILocalStorageService _localStorage;

        public int TotalRows = 0;

        public PersonService(ILocalStorageService localStorage, HttpClient client) { _client = client; _localStorage = localStorage; }

        public async Task<PeopleModel> SavePerson(PeopleModel person)
        {
            string token = await _localStorage.GetItemAsStringAsync("accesstoken");

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            
            string jsonString = JsonSerializer.Serialize(person);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var result = await _client.PutAsync($"https://www.schuebelsoftware.com/SSSCalWebAPI/api/person/{person.id}", content);
            //var result = await _client.PutAsync($"https://localhost:5011/api/person/{person.id}", content);

            var tokestr = await result.Content.ReadAsStringAsync();
            if (result.IsSuccessStatusCode)
            {
                person = JsonSerializer.Deserialize<PeopleModel>(tokestr);
            }
            else
            {
                throw new Exception(tokestr);
            }
            return person;
        }


        public async Task<Tuple<int, List<PeopleModel>>> GetPeople(int currentPage, int pageSize, string sortKey, string sortDirection, string searchString)
        {
            var filterParams = new System.Text.StringBuilder($"page={currentPage}&pageSize={pageSize}&sort[0][field]={sortKey}&sort[0][dir]={sortDirection}");
            if (!string.IsNullOrEmpty(searchString))
                filterParams.Append(searchString);
            _client.DefaultRequestHeaders.Clear();
            var httpResponse = await _client.GetAsync($"https://www.schuebelsoftware.com/SSSCalWebAPI/api/person?{filterParams}", HttpCompletionOption.ResponseHeadersRead);
//            var httpResponse = await _client.GetAsync($"https://localhost:5011/api/person?{filterParams}", HttpCompletionOption.ResponseHeadersRead);
            List<PeopleModel> lst = null;
            Tuple<int, List<PeopleModel>> retVal = null;

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
                lst= JsonSerializer.Deserialize<List<PeopleModel>>(streamReader.ReadToEnd());
                retVal = new Tuple<int, List<PeopleModel>>(TotalRows, lst);
            }
            return retVal;

        }

        //https://www.schuebelsoftware.com/SSSCalWebAPI/api/event?filter[logic]=and&filter[filters][0][field]=UserId&filter[filters][0][operator]=eq&filter[filters][0][value]=1&filter[filters][1][field]=TopicId&filter[filters][1][operator]=eq&filter[filters][1][value]=1

        public async Task<EventModel> GetDOB(int userID)
        {
            var filterParams = new System.Text.StringBuilder($"filter[logic]=and&filter[filters][0][field]=UserId&filter[filters][0][operator]=eq&filter[filters][0][value]={userID}&filter[filters][1][field]=TopicId&filter[filters][1][operator]=eq&filter[filters][1][value]=1");
            _client.DefaultRequestHeaders.Clear();
            var httpResponse = await _client.GetAsync($"https://www.schuebelsoftware.com/SSSCalWebAPI/api/event?{filterParams}", HttpCompletionOption.ResponseHeadersRead);
//            var httpResponse = await _client.GetAsync($"https://localhost:5011/api/event?{filterParams}", HttpCompletionOption.ResponseHeadersRead);
            List<EventModel> lst = null;
            EventModel retVal = null;

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
                lst = JsonSerializer.Deserialize<List<EventModel>>(streamReader.ReadToEnd());
                retVal = lst.First();
            }
            return retVal;

        }
    }
    public interface IAddressService
    {
        Task<Tuple<int, List<AddressModel>>> GetAddress(int currentPage, int pageSize, string sortKey, string sortDirection, string searchString);
        Task<AddressModel> SaveAddress(AddressModel addr);
    }
    public class AddressService : IAddressService
    {
        public HttpClient _client { get; set; }
        private readonly ILocalStorageService _localStorage;

        public int TotalRows = 0;

        public AddressService(ILocalStorageService localStorage, HttpClient client) { _client = client; _localStorage = localStorage; }

        public async Task<Tuple<int, List<AddressModel>>> GetAddress(int currentPage, int pageSize, string sortKey, string sortDirection, string searchString)
        {
            var filterParams = new System.Text.StringBuilder($"page={currentPage}&pageSize={pageSize}&sort[0][field]={sortKey}&sort[0][dir]={sortDirection}");
            if (!string.IsNullOrEmpty(searchString))
                filterParams.Append(searchString);

            var httpResponse = await _client.GetAsync($"https://www.schuebelsoftware.com/SSSCalWebAPI/api/Address?{filterParams}", HttpCompletionOption.ResponseHeadersRead);
        //    var httpResponse = await _client.GetAsync($"https://localhost:5011/api/Address?{filterParams}", HttpCompletionOption.ResponseHeadersRead);
        
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


        public async Task<AddressModel> SaveAddress(AddressModel addr)
        {
            string token = await _localStorage.GetItemAsStringAsync("accesstoken");

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            string jsonString = JsonSerializer.Serialize(addr);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var result = await _client.PostAsync($"https://www.schuebelsoftware.com/SSSCalWebAPI/api/Address", content);
            //var result = await _client.PostAsync($"https://localhost:5011/api/Address", content);

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
