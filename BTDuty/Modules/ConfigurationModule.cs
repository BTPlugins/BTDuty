using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BTDuty.Modules
{
    public class DutyGroups
    {
        public string DutyName { get; set; }
        public string GroupID { get; set; }
        public string Permission { get; set; }
        public bool BlueHammer { get; set; }
        public bool Godmode { get; set; }
        public bool Vanish { get; set; }
    }

    public class WebhookContainer
    {
        public string DutyLogWebhook { get; set; }
        public string ActiveDutyWebhook { get; set; }

        public string CommandWebhook { get; set; }
        public string ItemAddedWebhook { get; set; }
    }
    public class ServerAnnouncer
    {
        public bool Enabled { get; set; }
        public string BypassPermission { get; set; }
    }
    public class ActiveDutyList
    {
        public bool Enabled { get; set; }
        public float Timer { get; set; }
    }
    public class Restrictions
    {
        public string BypassPermission { set; get; }
        [XmlArrayItem("CommandName")]
        public List<string> RestrictedCommand { get; set; }
    }
}
