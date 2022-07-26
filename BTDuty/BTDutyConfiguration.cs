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
        public bool RemoveBlueHammerOnLogout { get; set; }
        public bool AllowDamageToPlayers { get; set; }
        public WebhookContainer WebhookContainer { get; set; }
        [XmlArrayItem("Group")]
        public List<DutyGroups> DutyGroups { get; set; }
        public ActiveDutyList ActiveDutyList { get; set; }
        //
        public Restrictions RestrictionsHolder { get; set; }
        public bool DebugMode { get; set; }
        public void LoadDefaults()
        {
            ServerAnnouncer = new ServerAnnouncer()
            {
                Enabled = false,
                BypassPermission = "NoSendMessage.IfHaveThisPerm",
            };
            RemoveDutyOnLogout = false;
            RemoveBlueHammerOnLogout = false;
            AllowDamageToPlayers = true;
            WebhookContainer = new WebhookContainer()
            {
                DutyLogWebhook = "https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}",
                ActiveDutyWebhook = "https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}",
                CommandWebhook = "https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}",
                ItemAddedWebhook = "https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}",
            };
            DutyGroups = new List<DutyGroups>()
            {
                new DutyGroups()
                {
                    DutyName = "Helper",
                    GroupID = "Helper",
                    Permission = "BTDuty.Helper",
                    BlueHammer = false,
                    Vanish = true,
                    Godmode = true,
                },
                new DutyGroups()
                {
                    DutyName = "Mod",
                    GroupID = "Moderator",
                    Permission = "BTDuty.Moderator",
                    BlueHammer = false,
                    Vanish = true,
                    Godmode = true,
                },
                new DutyGroups()
                {
                    DutyName = "Admin",
                    GroupID = "Administrator",
                    Permission = "BTDuty.Administrator",
                    BlueHammer = true,
                    Vanish = false,
                    Godmode = false,
                },
            };
            ActiveDutyList = new ActiveDutyList()
            {
                Enabled = true,
                Timer = 300f,
            };
            RestrictionsHolder = new Restrictions()
            {
                BypassPermission = "Ignore.CommandRestrictions",
                RestrictedCommand = new List<string>()
                {
                    "Admin",
                    "Slay",
                    "frisk",
                },
            };
            DebugMode = false;
        }
    }

}
