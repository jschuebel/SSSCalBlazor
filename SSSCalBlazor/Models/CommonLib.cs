namespace SSSCalBlazor.Models
{
    public class CommonLib
    {

        public CommonLib(ConfigurationManager config) {
            API_URL = config["API_URL"];
            SSO_URL = config["SSO_URL"];
        }

        public string API_URL { get; set; }
        public string SSO_URL { get; set; }
    }
}
