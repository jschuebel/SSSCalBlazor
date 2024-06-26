using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

using System.Net.Http;
using System.Net.Http.Json;
using System;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using System.IO;
using System.Linq;

using SSSCalBlazor.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

namespace SSSCalBlazor.Components
{
    public partial class EventComponent : ComponentBase
    {
        //[Inject]
        //public HttpClient client { get; set; }
        [Inject]
        public SSSCalBlazor.Models.IEventService svc { get; set; }

        [Inject]
        public IJSRuntime jsRuntime { get; set; }

        [Inject]
        public AuthenticationStateProvider AuthStateProvider { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState>? authenticationState { get; set; }


        public List<EventModel> eventData = new List<EventModel>();
        public int currentPage = 1, TotalRows = 0, pageSize = 15;
        protected string sortKey = "userName", sortDirection = "asc";
        bool isOpened = false;
        EventModel selectedEvent = new EventModel();
        private bool isadmin = false;

        protected async Task Search(string searchString = null)
        {
            //eventData = await client.GetFromJsonAsync<List<EventModel>>($"http://www.schuebelsoftware.com/SSSCalCoreApi/api/event");
            var filterParams = $"page={currentPage}&pageSize={pageSize}&sort[0][field]={sortKey}&sort[0][dir]={sortDirection}";

            var retv = await svc.GetEvent(currentPage, pageSize, sortKey, sortDirection, searchString);
            eventData = retv.Item2;
            TotalRows = retv.Item1;


            //client.DefaultRequestHeaders.Clear();
            //var httpResponse = await client.GetAsync($"https://www.schuebelsoftware.com/SSSCalWebAPI/api/event?{filterParams}", HttpCompletionOption.ResponseHeadersRead);
            //httpResponse.EnsureSuccessStatusCode(); // throws if not 200-299
            //if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
            //{
            //    if (httpResponse.Headers.Contains("Paging-TotalRecords"))
            //    {
            //        var hdrRecordCount = httpResponse.Headers.GetValues("Paging-TotalRecords").FirstOrDefault();
            //        TotalRows = int.Parse(hdrRecordCount);
            //    }
            //    var contentStream = await httpResponse.Content.ReadAsStreamAsync();
            //    var streamReader = new StreamReader(contentStream);
            //    eventData = JsonSerializer.Deserialize<List<EventModel>>(streamReader.ReadToEnd());
            //}
        }

        protected override async Task OnInitializedAsync()
        {
            if (authenticationState is not null)
            {
                var authState = await authenticationState;
                var User = authState.User;
                isadmin = User.IsInRole("admin");
            }

            await Search();
        }

        private void ShowPop(MouseEventArgs e, EventModel p)
        {
            selectedEvent = p;
            isOpened = true;
        }
        void SaveModal()
        {
            isOpened = false;
            ((IJSInProcessRuntime)jsRuntime).InvokeVoid("alert", $"Updating({selectedEvent.userName}, {selectedEvent.date}). Are you sure?");
        }

        void CloseModal()
        {
            isOpened = false;
        }


        async Task PagingHandler(int page)
        {
            currentPage = page;
            await Search();
            //            ((IJSInProcessRuntime)jsRuntime).InvokeVoid("alert", newMessage);
        }



    }
}