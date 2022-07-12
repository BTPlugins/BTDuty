using BTDuty.Helpers;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;
using ShimmyMySherbet.DiscordWebhooks.Embeded;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BTDuty.Commands
{
    public class DutyCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "Duty";

        public string Help => "Duty";

        public string Syntax => "Duty <Tag>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "BTDuty.Duty" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (DutyPlugin.Instance.onDuty.ContainsKey(player.CSteamID))
            {
                DutyPlugin.Instance.OffDutyOnline(player);
                return;
            }
            if(command.Length < 1)
            {
                TranslationHelper.SendMessageTranslation(player.CSteamID, "ProperUsage", "/Duty <Tag>");
                return;
            }
            var duty = DutyPlugin.Instance.Configuration.Instance.DutyGroups.FirstOrDefault(k => k.DutyName.Equals(command[0], StringComparison.OrdinalIgnoreCase));
            if (R.Permissions.GetGroup(duty.GroupID) == null)
            {
                Logger.LogWarning("----------------------------------------");
                Logger.LogWarning("ERROR: Invalid Group: " + duty.GroupID);
                Logger.LogWarning("ERROR: Invalid Group: " + duty.GroupID);
                Logger.LogWarning("ERROR: Invalid Group: " + duty.GroupID);
                Logger.LogWarning("----------------------------------------");
                TranslationHelper.SendMessageTranslation(player.CSteamID, "ErrorContactStaff");
                return;
            }
            if (!player.HasPermission(duty.Permission))
            {
                // Missing Perms to go further.
                TranslationHelper.SendMessageTranslation(player.CSteamID, "Duty_MissingPerm", duty.Permission, duty.DutyName);
                return;
            }
            if (DutyPlugin.Instance.onDuty.ContainsKey(player.CSteamID))
            {
                // Going off Duty
                R.Permissions.RemovePlayerFromGroup(duty.GroupID, player);
                if (!DutyPlugin.Instance.onDuty.TryGetValue(player.CSteamID, out DutyPlugin.OnDutyHolder value)) return;
                TranslationHelper.SendMessageTranslation(player.CSteamID, "Duty_OffDuty", value.DutyName, TimeConverterManager.Format(TimeConverterManager.getTimeSpan(value.StartDate, DateTime.Now), 2));
                if (DutyPlugin.Instance.Configuration.Instance.ServerAnnouncer.Enabled && !player.HasPermission(DutyPlugin.Instance.Configuration.Instance.ServerAnnouncer.BypassPermission))
                {
                    foreach (SteamPlayer steamPlayer in Provider.clients)
                    {
                        UnturnedPlayer user = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                        TranslationHelper.SendMessageTranslation(user.CSteamID, "Broadcast_OffDuty", player.CharacterName, duty.DutyName);
                    }
                }
                ThreadHelper.RunAsynchronously(() =>
                {
                    WebhookMessage Embed = new WebhookMessage()
                    .PassEmbed()
                    .WithTitle(player.CharacterName + " Duty Log")
                    .WithColor(EmbedColor.Red)
                    .WithURL("https://steamcommunity.com/profiles/" + player.CSteamID)
                    .WithTimestamp(DateTime.Now)
                    .WithField("**Username**", player.CharacterName + " \n(" + player.CSteamID + ")")
                    .WithField("**TimeClock Information**", "Start Date: <t:" + ((DateTimeOffset)TimeZoneInfo.ConvertTimeToUtc(value.StartDate)).ToUnixTimeSeconds().ToString() + ">\nEnd Date: <t:" + ((DateTimeOffset)TimeZoneInfo.ConvertTimeToUtc(DateTime.Now)).ToUnixTimeSeconds().ToString() + ">" )
                    .WithField("**Total Time** ", "``" + TimeConverterManager.Format(TimeConverterManager.getTimeSpan(value.StartDate, DateTime.Now), 2) + "``")
                    .WithField("**Duty Information**", "Duty Name: ``" + duty.DutyName + "``\nPermission: ``" + duty.Permission + "``")
                    .Finalize();
                    DiscordWebhookService.PostMessageAsync(DutyPlugin.Instance.Configuration.Instance.DutyLogWebhook, Embed);
                });
                DutyPlugin.Instance.onDuty.Remove(player.CSteamID);
                return;
            }
            // Going on Duty
            R.Permissions.AddPlayerToGroup(duty.GroupID, player);
            DutyPlugin.Instance.onDuty.Add(player.CSteamID, new DutyPlugin.OnDutyHolder { DutyName = duty.DutyName, GroupID = duty.GroupID, Permission = duty.Permission, StartDate = DateTime.Now });
            TranslationHelper.SendMessageTranslation(player.CSteamID, "Duty_OnDuty", duty.DutyName);
            if (DutyPlugin.Instance.Configuration.Instance.ServerAnnouncer.Enabled && !player.HasPermission(DutyPlugin.Instance.Configuration.Instance.ServerAnnouncer.BypassPermission))
            {
                foreach (SteamPlayer steamPlayer in Provider.clients)
                {
                    UnturnedPlayer user = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                    TranslationHelper.SendMessageTranslation(user.CSteamID, "Broadcast_OnDuty", player.CharacterName, duty.DutyName);
                }
            }
        }
    }
}
