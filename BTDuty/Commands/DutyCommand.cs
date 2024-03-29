﻿using BTDuty.Helpers;
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
                DebugManager.SendDebugMessage(player.CharacterName + " has gone off Duty");
                DutyPlugin.Instance.OffDutyOnline(player);
                return;
            }
            if(command.Length < 1)
            {
                TranslationHelper.SendMessageTranslation(player.CSteamID, "ProperUsage", "/Duty <Tag>");
                return;
            }
            var duty = DutyPlugin.Instance.Configuration.Instance.DutyGroups.FirstOrDefault(k => k.DutyName.Equals(command[0], StringComparison.OrdinalIgnoreCase));
            if(duty == null)
            {
                TranslationHelper.SendMessageTranslation(player.CSteamID, "DutyGroup_NotFound", command[0]);
                DebugManager.SendDebugMessage("Duty - Invlaid Duty Group Selected");
                return;
            }
            if (R.Permissions.GetGroup(duty.GroupID) == null)
            {
                DebugManager.SendDebugMessage("Invalid Group");
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
                DebugManager.SendDebugMessage(player.CharacterName + " Missing Permission: " + duty.Permission + " for " + duty.DutyName);
                TranslationHelper.SendMessageTranslation(player.CSteamID, "Duty_MissingPerm", duty.Permission, duty.DutyName);
                return;
            }
            if (DutyPlugin.Instance.onDuty.ContainsKey(player.CSteamID))
            {
                // Going off Duty
                if(player.IsAdmin && DutyPlugin.Instance.onDuty[player.CSteamID].BlueHammer)
                {
                    player.Admin(false);
                    DebugManager.SendDebugMessage("Removing BlueHammer for " + player.CharacterName);
                }
                //
                player.Features.GodMode = false;
                player.Features.VanishMode = false;
                //
                DebugManager.SendDebugMessage(player.CharacterName + " has gone off Duty");
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
                DebugManager.SendDebugMessage("Duty - Sending Webhook");
                DutyPlugin.Instance.TimeClock.Instance.TimeClock.Add(new Modules.PlayerTimeclock { playerID = player.CSteamID.m_SteamID, DutyGroup = value.GroupID, DutyName = value.DutyName, StartDate = value.StartDate, EndDate = DateTime.Now, TotalTime = (int)(DateTime.Now - value.StartDate).TotalSeconds });
                DutyPlugin.Instance.TimeClock.Save();
                var playerTimeClock = DutyPlugin.Instance.TimeClock.Instance.TimeClock.Where(c => c.playerID == player.CSteamID.m_SteamID);
                var sessionTime = TimeConverterManager.Format(TimeConverterManager.getTimeSpan(value.StartDate, DateTime.Now), 2);
                var weekTime = TimeConverterManager.Format(TimeSpan.FromSeconds(playerTimeClock.Where(v => (v.EndDate - DateTime.Now).TotalDays < 7).Sum(b => b.TotalTime)), 2);
                var totalTime = TimeConverterManager.Format(TimeSpan.FromSeconds(playerTimeClock.Sum(b => b.TotalTime)), 2);
                var timeToday = TimeConverterManager.Format(TimeSpan.FromSeconds(playerTimeClock.Where(v => v.EndDate.Date == DateTime.Now.Date).Sum(c => c.TotalTime)), 2);


                ThreadHelper.RunAsynchronously(async () =>
                {
                    var embed = new WebhookMessage()
                    .PassEmbed()
                    .WithTitle(player.CharacterName + " Duty Log")
                    .WithColor(EmbedColor.Red)
                    .WithURL("https://steamcommunity.com/profiles/" + player.CSteamID)
                    .WithField("**Username**", player.CharacterName + " \n(" + player.CSteamID + ")")
                    .WithField("**TimeClock Information**", "Start Date: <t:" + ((DateTimeOffset)TimeZoneInfo.ConvertTimeToUtc(value.StartDate)).ToUnixTimeSeconds().ToString() + ">\nEnd Date: <t:" + ((DateTimeOffset)TimeZoneInfo.ConvertTimeToUtc(DateTime.Now)).ToUnixTimeSeconds().ToString() + ">")
                    .WithField("**Total Time** ", "``" + TimeConverterManager.Format(TimeConverterManager.getTimeSpan(value.StartDate, DateTime.Now), 2) + "``")
                    .WithField("**Duty Information**", "Duty Name: ``" + duty.DutyName + "``\nPermission: ``" + duty.Permission + "``");
                    embed.footer = new WebhookFooter() { text = "[BTDuty] " + Provider.serverName + " - " + DateTime.Now.ToString("dddd, dd MMMM yyyy") + "" };
                    var send = embed.Finalize();
                    await DiscordWebhookService.PostMessageAsync(DutyPlugin.Instance.Configuration.Instance.WebhookContainer.DutyLogWebhook, send);
                    var embed2 = new WebhookMessage()
                    .PassEmbed()
                    .WithTitle(player.CharacterName + " **(" + player.CSteamID + ")**")
                    .WithColor(EmbedColor.Yellow)
                    .WithThumbnail(player.SteamProfile.AvatarFull.AbsoluteUri)
                    .WithURL("https://steamcommunity.com/profiles/" + player.CSteamID)
                    .WithDescription("**User Summary**")
                    .WithField("**Duty Session**", sessionTime)
                    .WithField("**Time Today**", timeToday)
                    .WithField("**Time This Week**", weekTime)
                    .WithField("**Total Time**", totalTime);

                    embed2.footer = new WebhookFooter() { text = "[BTDuty] " + Provider.serverName + " - " + DateTime.Now.ToString("dddd, dd MMMM yyyy") + "" };
                    var send2 = embed2.Finalize();
                    await DiscordWebhookService.PostMessageAsync(DutyPlugin.Instance.Configuration.Instance.WebhookContainer.DutySummary, send2);
                });
                DutyPlugin.Instance.onDuty.Remove(player.CSteamID);
                return;
            }
            // Going on Duty
            DebugManager.SendDebugMessage(player.CharacterName + " is now going on Duty for: " + duty.DutyName);
            R.Permissions.AddPlayerToGroup(duty.GroupID, player);
            DutyPlugin.Instance.onDuty.Add(player.CSteamID, new DutyPlugin.OnDutyHolder { DutyName = duty.DutyName, GroupID = duty.GroupID, BlueHammer = duty.DutySettings.BlueHammer, GodMode = duty.DutySettings.Godmode, Vanish = duty.DutySettings.Vanish, Permission = duty.Permission, AllowDamageToPlayers = duty.DutySettings.AllowDamageToPlayers, StartDate = DateTime.Now });
            TranslationHelper.SendMessageTranslation(player.CSteamID, "Duty_OnDuty", duty.DutyName);
            if (duty.DutySettings.BlueHammer)
            {
                player.Admin(true);
            }
            //
            if (DutyPlugin.Instance.onDuty[player.CSteamID].Vanish)
            {
                player.Features.VanishMode = true;
            }
            if (DutyPlugin.Instance.onDuty[player.CSteamID].GodMode)
            {
                player.Features.GodMode = true;
            }
            //
            if (DutyPlugin.Instance.Configuration.Instance.ServerAnnouncer.Enabled && R.Permissions.GetPermissions(player, DutyPlugin.Instance.Configuration.Instance.ServerAnnouncer.BypassPermission).Count == 0)
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
