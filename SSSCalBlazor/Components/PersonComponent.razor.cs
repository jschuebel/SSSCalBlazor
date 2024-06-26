using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

//using System.Net.Http;
//using System.Net.Http.Json;
using System;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
//using System.IO;
using System.Linq;

//using Newtonsoft.Json;
using SSSCalBlazor.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.RegularExpressions;

namespace SSSCalBlazor.Components
{
    public partial class PersonComponent : ComponentBase //Fluxor.Blazor.Web.Components.FluxorComponent
    {
        //[Inject]
        //public HttpClient client { get; set; }

        [Inject]
        public SSSCalBlazor.Models.IPersonService svc { get; set; }
        [Inject]
        public SSSCalBlazor.Models.IAddressService svcA { get; set; }
        [Inject]
        public IJSRuntime jsRuntime { get; set; }

        [Inject]
        public AuthenticationStateProvider AuthStateProvider { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState>? authenticationState { get; set; }

        /*Fluxor state changes
                @inherits Fluxor.Blazor.Web.Components.FluxorComponent
        <h1>Sel Name: @pState.Value.peopleModel.name</h1>


                [Inject]
                public IState<PersonState> pState { get; set; }

                [Inject]
                public IDispatcher dispatch {get;set;}

        */

        public bool hasChanged { get; set; } = false;
        public string selectedCountryID { get; set; }
        public List<PeopleModel> peopleData = new List<PeopleModel>();
        public List<AddressModel> addressData = new List<AddressModel>();
        public int selectedID { get; set; }
        public int selectedAddressID { get; set; }
        public AddressModel selectedAddress { get; set; }
        public AddressModel selectedCopyAddress { get; set; } = new AddressModel();
        public AddressModel Address2Add { get; set; } = new AddressModel();
        public int currentPage = 1, TotalRows = 0, pageSize = 10;
        protected string sortKey = "Name", activeKey = "name", sortDirection ="asc", holdSearchString;
        bool isOpened = false;
        bool isAddressOpened { get; set; } = false;

        PeopleModel selectedPerson { get; set; }
        PeopleModel selectedCopyPerson = new PeopleModel();
        private bool isadmin = false;

        public string errorAddrMsg { get; set; }
        public string errorAddrMsgColor { get; set; } = "color:red;";
        public string errorMsg { get; set; }
        public string errorMsgColor { get; set; } = "color:red;";

        protected override async Task OnInitializedAsync()
        {
            if (authenticationState is not null)
            {
                var authState = await authenticationState;
                var User = authState.User;
                isadmin = User.IsInRole("admin");
            }

            var retvAddr = await svcA.GetAddress(1, 999, "address1", "asc", null);
            addressData = retvAddr.Item2;

            var noneselected = addressData.FirstOrDefault(x => x.address1 == "" && x.state == "??");
            if (noneselected != null)
            {
                noneselected.address1 = null;
                noneselected.state = null;
            }

            await Search();
        }


        private async Task onChange(ChangeEventArgs e)
        {
            var AddressID = int.Parse(e.Value.ToString());
            selectedCopyPerson.addressId = AddressID;
            selectedCopyAddress = addressData.First(x => x.id == selectedCopyPerson.addressId);
            hasChanged = true;
        }

        protected async Task Search(string searchString=null) {

            var retv = await svc.GetPeople(currentPage, pageSize, sortKey, sortDirection, searchString);
            peopleData = retv.Item2;
            TotalRows = retv.Item1;


            

            /*  ==========>REAL 
            //https://www.schuebelsoftware.com/SSSCalWebAPI/api/person?page=1&pageSize=10&sort[0][field]=name&sort[0][dir]=asc&filter[logic]=and&filter[filters][0][field]=name&%20filter[filters][0][operator]=contains&filter[filters][0][value]=am
            var filterParams = new System.Text.StringBuilder($"page={currentPage}&pageSize={pageSize}&sort[0][field]={sortKey}&sort[0][dir]={sortDirection}");
            if (!string.IsNullOrEmpty(searchString))
                filterParams.Append(searchString);

            //            peopleData = await client.GetFromJsonAsync<List<PeopleModel>>($"http://www.schuebelsoftware.com/SSSCalCoreApi/api/person?{filterParams}");
            //var httpResponse = await client.GetAsync($"http://api.schuebelsoftware.com/api/person?{filterParams}", HttpCompletionOption.ResponseHeadersRead);
            //Azure requires SSL


             var httpResponse = await client.GetAsync($"https://www.schuebelsoftware.com/SSSCalWebAPI/api/person?{filterParams}", HttpCompletionOption.ResponseHeadersRead);

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
                peopleData = JsonConvert.DeserializeObject<List<PeopleModel>>(streamReader.ReadToEnd());
            }
            */
        }


        async Task PagingHandler(int page)
        {
            currentPage = page;
            await Search(holdSearchString);
            //            ((IJSInProcessRuntime)jsRuntime).InvokeVoid("alert", newMessage);
        }

        protected async Task ShowPop(MouseEventArgs e, PeopleModel p)
        {
            /*Fluxor state changes
                        dispatch.Dispatch(new AddPerson(p));
                        return;
            */
            errorMsgColor = "color:red;";
            errorMsg = string.Empty;
            hasChanged=false;

            selectedPerson = p;
            selectedCopyPerson = new PeopleModel(p);

            var retDOB = await svc.GetDOB(selectedCopyPerson.id);

            if (selectedPerson.addressId != 0)
            {
                selectedAddress = addressData.First(x => x.id == selectedPerson.addressId);
                selectedCopyAddress = new AddressModel(selectedAddress);
            }

            isOpened = true;

        }

        async Task OnAddressChange(KeyboardEventArgs args)
        {

            hasChanged = selectedAddress.address1 != selectedCopyAddress.address1;

//            errorMsg = hasChanged ? "changed" : string.Empty;
        }
        async Task OnWorkChange(KeyboardEventArgs args)
        {

            hasChanged = selectedPerson.work != selectedCopyPerson.work;
            if (hasChanged && string.IsNullOrEmpty(selectedCopyPerson.work?.Trim())) {
                selectedCopyPerson.work = null;
            }

  //          errorMsg = hasChanged ? "changed" : string.Empty;
        }


        async Task Filter(Tuple<string, string, string> searchv)
        {
            //ColumnName, ColumnType, _searchString
            Console.WriteLine($" column({searchv.Item1})  columnType({searchv.Item2}) searchString({searchv.Item3})");
            //https://www.schuebelsoftware.com/SSSCalWebAPI/api/person?page=1&pageSize=10&sort[0][field]=name&sort[0][dir]=asc&filter[logic]=and&filter[filters][0][field]=homePhone&filter[filters][0][operator]=contains&filter[filters][0][value]=678
            string srch =null;
            if (!string.IsNullOrEmpty(searchv.Item3))
            {
                if (searchv.Item2 == "string")
                    srch = $"&filter[logic]=and&filter[filters][0][field]={searchv.Item1}&filter[filters][0][operator]=contains&filter[filters][0][value]={searchv.Item3}";
                if (searchv.Item2 == "date")
                {
                    srch = $"&filter[logic]=and&filter[filters][0][field]={searchv.Item1}&filter[filters][0][operator]=eq&filter[filters][0][value]={searchv.Item3}";
                    //create range
                    //srch = $"&filter[logic]=and&filter[filters][0][field]={searchv.Item1}&filter[filters][0][operator]=gte&filter[filters][0][value]={searchv.Item3}";
                    // &filter[filters][1][field]=Date&filter[filters][1][operator]=lte&filter[filters][1][value]=Mon+Apr+30+2018+00%3A00%3A00+GMT-0500+(Central+Daylight+Time)&_=1562110553341";
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

            if (sortKey == fldName) {
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

        //disabled="@(isadmin==false)"
        private async Task ShowPopAddress()
        {
            Address2Add = new AddressModel();
            isAddressOpened = true;
        }
        private async Task CloseAddressModal()
        {
            isAddressOpened = false;
        }
        private async Task SaveAddressModal()
        {
            //force check for refreshtoken
            await AuthStateProvider.GetAuthenticationStateAsync();
            try
            {
                selectedCopyAddress = await svcA.Save(Address2Add);

                var retvAddr = await svcA.GetAddress(1, 999, "address1", "asc", null);
                addressData = retvAddr.Item2;

                selectedCopyPerson.addressId = selectedCopyAddress.id;
                hasChanged=true;

                isAddressOpened = false;

                StateHasChanged();
            }
            catch (Exception ex)
            {
                errorAddrMsgColor = "color:red;";
                errorAddrMsg = ex.Message;

            }
        }

        private async Task SaveModal()
        {
            isOpened = false;

            await AuthStateProvider.GetAuthenticationStateAsync();
            try
            {
                //((IJSInProcessRuntime)jsRuntime).InvokeVoid("alert", $"Updating({selectedPerson.name}, {selectedPerson.dateOfBirth}, {selectedPerson.homePhone}). Are you sure?");
                //bool confirmed = await ((IJSInProcessRuntime)jsRuntime).InvokeAsync<bool>("confirm", new object[] { $"Updating({selectedPerson.name}, {selectedPerson.dateOfBirth}, {selectedPerson.homePhone}).\n\nAre you sure?" });
                //if (confirmed)

                var nwPerson = await svc.Save(selectedCopyPerson);
                errorMsgColor = "color:green;";
                errorMsg = $"{selectedPerson.name} Saved...";
                await Search(holdSearchString);
            }
            catch (Exception ex)
            {
                errorMsgColor = "color:red;";
                errorMsg = ex.Message;

            }
        }

        private async Task CloseModal()
        {
            if (hasChanged)
            {
                bool confirmed = await ((IJSInProcessRuntime)jsRuntime).InvokeAsync<bool>("confirm", new object[] { $"{selectedPerson.name} has been changed.\n\nExit without saving. Are you sure?" });
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

        private async Task OnTestClick()
        {
            await jsRuntime.InvokeVoidAsync("alert", "selected val" + selectedCountryID);

            //        ((IJSInProcessRuntime)jsRuntime).InvokeVoid("ShowAlert", "selected val" + selectedCountryID);

        }

  
     }
}