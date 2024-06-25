using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSSCalBlazor.Models
{
    public class PeopleModel
    {
        public PeopleModel() {}
        public PeopleModel(PeopleModel org) {
            this.id = org.id;
            this.name = org.name;
            this.dateOfBirth = org.dateOfBirth;
            this.homePhone = org.homePhone;
            this.pager = org.pager;
            this.work = org.work;
            this.eMail = org.eMail;
            this.fax = org.fax;
            this.mobile = org.mobile;
            this.addressId = org.addressId;
            this.birthdayAlert = org.birthdayAlert;
            this.createdate = org.createdate;
        }


        public int id { get; set; }
        public string name { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string homePhone { get; set; }
        public string pager { get; set; }
        public string work { get; set; }
        public string eMail { get; set; }
        public string fax { get; set; }
        public string mobile { get; set; }
        public int addressId { get; set; }
        public bool birthdayAlert { get; set; }
        public DateTime createdate { get; set; }
        //createdate":"1999-11-10T00:00:00","address":null,"events
    }
    public class PersonHistoryModel
    {
    
        public Int64 Id { get; set; }
        public int PersonID { get; set; }
        public string Name { get; set; }
        public string FieldName { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public string PreviousValue { get; set; }
        public string NewValue { get; set; }
    }

    public class EventModel
    {
        public int id { get; set; }
        public string description { get; set; }
        public string displayDescription
        {
            get
            {
                var len = 0;
                var newstring = "";
                if (description == null || (description != null && description.Trim().Length == 0))
                {
                    newstring = this.topic == null ? "N/A" : userName + " " + this.topic.Trim();
                    if (newstring.Trim().Length > 25)
                        newstring = newstring.Substring(0, 25).Trim();
                    len = newstring.Length;
                }
                else
                {
                    if (this.description.Contains("@fmt"))
                    {
                        return this.description.Substring(4);
                    }
                    if (description.Trim().Length > 25)
                        newstring = description.Substring(0, 25).Trim();
                    else
                        newstring = description.Trim();
                    len = newstring.Length;
                }
                len = (len > 25) ? 25 : len;
                var padd = new string[27 - len];
                var padds = string.Join("&nbsp;", padd);

                if (id != -1)
                    return newstring + padds + (date == null ? "N/A" : date.Value.ToString("MM-dd-yyyy"));
                else
                    return newstring;

            }
        }
        public string userName { get; set; }
        public int userId { get; set; }
        public int topicId { get; set; }
        public DateTime? date { get; set; }
        public bool? repeatYearly { get; set; }
        public bool? repeatMonthly { get; set; }
        public string topic { get; set; }
        public Topic topicf { get; set; }

        public string topicTitle { get { return topicf?.topicTitle; } set { topicf.topicTitle=value; } }
        

    }

    public class Topic
    {
        //{"id":1,"topicTitle":"Birthday","createdate":"1999-11-10T00:00:00"}
        public int id { get; set; }
        public string topicTitle { get; set; }
    }

    public class GroupModel
    {
        public int id { get; set; }
        public int eventId { get; set; }
        public int personId { get; set; }
        PeopleModel[] people { get; set; }
    }
    public class PictureNode
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string FileExtension { get; set; }
        public string FileType { get; set; }
        public string CellType { get; set; }
        public Dictionary<string, PictureNode> folders { get; set; }
 
    }

    //{"id":1,"address1":"","city":null,"state":"??","zip":null,"createdate":"2000-02-16T00:00:00"}
    public class AddressModel
    {
        public AddressModel() { }
        public AddressModel(AddressModel org) {
            this.id=org.id;
            this.address1 = org.address1;
            this.city = org.city;
            this.state = org.state;
            this.zip = org.zip;
        }
        public int id { get; set; }
        public string address1 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public DateTime createdate { get; set; }
    }

    public class UserLoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
