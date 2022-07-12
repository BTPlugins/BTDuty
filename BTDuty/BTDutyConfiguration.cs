using BTDuty.Modules;
using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BTDuty
{
    public class BTDutyConfiguration : IRocketPluginConfiguration
    {
        public ServerAnnouncer ServerAnnouncer { get; set; }
        public bool RemoveDutyOnLogout { get; set; }
        public string DutyLogWebhook { get; set; }

        [XmlArrayItem("Group")]
        public List<DutyGroups> DutyGroups { get; set; }
        public ActiveDutyList ActiveDutyList { get; set; }
        public bool DebugMode { get; set; }
        public void LoadDefaults()
        {
            ServerAnnouncer = new ServerAnnouncer()
            {
                Enabled = false,
                BypassPermission = "NoSendMessage.IfHaveThisPerm",
            };
            RemoveDutyOnLogout = false;
            DutyLogWebhook = "https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}";
            DutyGroups = new List<DutyGroups>()
            {
                new DutyGroups()
                {
                    DutyName = "Helper",
                    GroupID = "Helper",
                    Permission = "BTDuty.Helper",
                },
                new DutyGroups()
                {
                    DutyName = "Mod",
                    GroupID = "Moderator",
                    Permission = "BTDuty.Moderator",
                },
                new DutyGroups()
                {
                    DutyName = "Admin",
                    GroupID = "Administrator",
                    Permission = "BTDuty.Administrator",
                },
            };
            ActiveDutyList = new ActiveDutyList()
            {
                Enabled = true,
                Timer = 300f,
                WebhookURL = "https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}",
            };
            DebugMode = false;
        }
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
        public string WebhookURL { get; set; }
    }
}
