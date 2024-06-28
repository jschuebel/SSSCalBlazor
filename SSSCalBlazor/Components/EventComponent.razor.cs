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
using System.Reflection;


namespace SSSCalBlazor.Components
{
    public partial class EventComponent : ComponentBase
    {
        //[Inject]
        //public HttpClient client { get; set; }
        [Inject]
        public SSSCalBlazor.Models.IEventService svc { get; set; }

        [Inject]
        public SSSCalBlazor.Models.IPersonService svcP { get; set; }

        [Inject]
        public IJSRuntime jsRuntime { get; set; }

        [Inject]
        public AuthenticationStateProvider AuthStateProvider { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState>? authenticationState { get; set; }


        public List<EventModel> eventData = new List<EventModel>();
        public List<TopicModel> topicData = new List<TopicModel>();
        
        public int currentPage = 1, TotalRows = 0, pageSize = 15;
        protected string sortKey = "Date", sortDirection = "desc", holdSearchString;
        bool isOpened = false;
        EventModel selectedEvent;
        EventModel selectedCopyEvent = new EventModel();
        public List<PeopleModel> peopleData = new List<PeopleModel>();
        public bool hasChanged = false;
        private bool isadmin = false;
        public string errorMsg { get; set; }
        public string errorMsgColor { get; set; } = "color:red;";

        protected async Task Search(string searchString = null)
        {
            errorMsg = string.Empty;
            //eventData = await client.GetFromJsonAsync<List<EventModel>>($"http://www.schuebelsoftware.com/SSSCalCoreApi/api/event");
            try
            {
                var retv = await svc.GetEvent(currentPage, pageSize, sortKey, sortDirection, searchString);
                eventData = retv.Item2;
                TotalRows = retv.Item1;
            }
            catch (Exception ex)
            {
                errorMsgColor = "color:red;";
                errorMsg = ex.Message;

                eventData = new List<EventModel>();
                TotalRows = 0;
            }


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

        protected override async Task OnAfterRenderAsync(bool firstRender) //
        {
            if (firstRender)
            {
                if (authenticationState is not null)
                {
                    var authState = await authenticationState;
                    var User = authState.User;
                    isadmin = User.IsInRole("admin");
                }
                var retvAddr = await svcP.GetPeople(1, 9999, "Name", "asc", null);
                peopleData = retvAddr.Item2;
                peopleData.Insert(0, new PeopleModel() { id = 0, name = "&lt;&lt;Make Selection&gt;&gt;" });

                topicData.Insert(0, new TopicModel() { Id = 0, TopicTitle = "&lt;&lt;Make Selection&gt;&gt;" });
                topicData.Insert(0, new TopicModel() { Id = 1, TopicTitle = "Birthday" });
                topicData.Insert(0, new TopicModel() { Id = 2, TopicTitle = "Hockey" });
                topicData.Insert(0, new TopicModel() { Id = 3, TopicTitle = "Business Meetings" });
                topicData.Insert(0, new TopicModel() { Id = 5, TopicTitle = "TODO" });
                topicData.Insert(0, new TopicModel() { Id = 11, TopicTitle = "Training" });
                topicData.Insert(0, new TopicModel() { Id = 12, TopicTitle = "Party" });
                topicData.Insert(0, new TopicModel() { Id = 14, TopicTitle = "Dinner" });
                topicData.Insert(0, new TopicModel() { Id = 15, TopicTitle = "Special Day" });
                topicData.Insert(0, new TopicModel() { Id = 16, TopicTitle = "Basketball One on One" });
                topicData.Insert(0, new TopicModel() { Id = 17, TopicTitle = "HealthCare" });



                await Search();
                StateHasChanged();
            }
        }

        private void ShowPop(MouseEventArgs e, EventModel p)
        {
            errorMsgColor = "color:red;";
            errorMsg = string.Empty;


            if (e == null && p == null)
            {
                selectedCopyEvent = new EventModel();
            }
            else
            {
                selectedEvent = p;
                selectedCopyEvent = new EventModel(p);
            }
            isOpened = true;
        }
        private async Task SaveModal()
        {

            await AuthStateProvider.GetAuthenticationStateAsync();
            try
            {
                if (selectedCopyEvent.userId == 0)
                {
                    await jsRuntime.InvokeVoidAsync("alert", $"Name is a required field");
                    return;
                }
                var nwevt = await svc.Save(selectedCopyEvent);
                isOpened = false;
                await Search(holdSearchString);
                errorMsgColor = "color:green;";

                if (nwevt.topicf==null)
                {
                    var tp = topicData.FirstOrDefault(x => x.Id == selectedCopyEvent.topicId);
                    nwevt.topicf = new Topic() { id = tp.Id, topicTitle = tp.TopicTitle };
                }
                errorMsg = $"Event({nwevt.topicTitle}, {nwevt.date}) for {nwevt.userName} Saved...";
                // await jsRuntime.InvokeVoidAsync("alert", $"Updated Event({selectedCopyEvent.topicTitle}, {selectedCopyEvent.date}) for {selectedCopyEvent.userName}");

            }
            catch (Exception ex)
            {
                isOpened = false;
                errorMsgColor = "color:red;";
                errorMsg = ex.Message;
            }

        }

        private async Task CloseModal()
        {
            if (hasChanged)
            {
                bool confirmed = await jsRuntime.InvokeAsync<bool>("confirm", new object[] { $"{selectedCopyEvent.userName} has been changed.\n\nExit without saving. Are you sure?" });
                if (confirmed)
                {
                    isOpened = false;
                }
            }
            else
            {
                isOpened = false;
            }
        }


        async Task PagingHandler(int page)
        {
            currentPage = page;
            await Search(holdSearchString);
            //            ((IJSInProcessRuntime)jsRuntime).InvokeVoid("alert", newMessage);
        }

        async Task Filter(Tuple<string, string, string, string> searchv)
        {
            string srch = null;
            if (!string.IsNullOrEmpty(searchv.Item3))
            {
                if (searchv.Item2 == "string")
                    srch = $"&filter[logic]=and&filter[filters][0][field]={searchv.Item1}&filter[filters][0][operator]=contains&filter[filters][0][value]={searchv.Item3}";
                if (searchv.Item2 == "date")
                {
                    //srch = $"&filter[logic]=and&filter[filters][0][field]={searchv.Item1}&filter[filters][0][operator]=eq&filter[filters][0][value]={searchv.Item3}";
                    //create range
                    srch = $"&filter[logic]=and&filter[filters][0][field]={searchv.Item1}&filter[filters][0][operator]=gte&filter[filters][0][value]={searchv.Item3}";
                }
                if (searchv.Item2 == "int")
                    srch = $"&filter[logic]=and&filter[filters][0][field]={searchv.Item1}&filter[filters][0][operator]=eq&filter[filters][0][value]={searchv.Item3}";
            }
            if (!string.IsNullOrEmpty(searchv.Item4))
            {
                if (searchv.Item2 == "date")
                {
                    if (string.IsNullOrEmpty(searchv.Item3))
                        srch = $"&filter[logic]=and&filter[filters][0][field]={searchv.Item1}&filter[filters][0][operator]=gte&filter[filters][0][value]={searchv.Item4}";
                    if (!string.IsNullOrEmpty(searchv.Item3))
                        srch += $"&filter[logic]=and&filter[filters][1][field]={searchv.Item1}&filter[filters][1][operator]=lte&filter[filters][1][value]={searchv.Item4}";
                }
                if (searchv.Item2 == "int")
                    srch = $"&filter[logic]=and&filter[filters][0][field]={searchv.Item1}&filter[filters][0][operator]=eq&filter[filters][0][value]={searchv.Item3}";
            }
            currentPage = 1;
            holdSearchString = srch;
            await Search(srch);
        }
        async Task Sort(string fldName)
        {
            if (sortKey == fldName)
            {
                if (sortDirection == "asc")
                    sortDirection = "desc";
                else sortDirection = "asc";
            }
            else
            {
                sortKey = fldName;
                sortDirection = "asc";
            }
            Console.WriteLine($"fldname({fldName}) sortdir({sortDirection})");
            await Search(holdSearchString);
        }

        private async Task onChangePerson(ChangeEventArgs e)
        {

            Console.WriteLine("onChange selectedEventID=" + e.Value.ToString());
            selectedCopyEvent.userId = int.Parse(e.Value.ToString());
        }
        private async Task onChangeCategory(ChangeEventArgs e)
        {

            Console.WriteLine("onChange selectedEventID=" + e.Value.ToString());
            selectedCopyEvent.topicId = int.Parse(e.Value.ToString());

            var per = peopleData.FirstOrDefault(x => x.id == selectedCopyEvent.userId);
            if (per != null)
                selectedCopyEvent.userName = per.name;
        }
    }
}