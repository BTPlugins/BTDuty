using Rocket.API;
using Rocket.Core.Plugins;
using System;
using Logger = Rocket.Core.Logging.Logger;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Collections;
using UnityEngine;
using BTDuty.Helpers;
using ShimmyMySherbet.DiscordWebhooks.Embeded;
using Rocket.Core;

namespace BTDuty
{
    public partial class DutyPlugin : RocketPlugin<BTDutyConfiguration>
    {
        public static DutyPlugin Instance;
        public IDictionary<CSteamID, OnDutyHolder> onDuty = new Dictionary<CSteamID, OnDutyHolder>();
        protected override void Load()
        {
            Instance = this;
            Logger.Log("#############################################", ConsoleColor.Yellow);
            Logger.Log("###             BTDuty Loaded             ###", ConsoleColor.Yellow);
            Logger.Log("###   Plugin Created By blazethrower320   ###", ConsoleColor.Yellow);
            Logger.Log("###            Join my Discord:           ###", ConsoleColor.Yellow);
            Logger.Log("###     https://discord.gg/YsaXwBSTSm     ###", ConsoleColor.Yellow);
            Logger.Log("#############################################", ConsoleColor.Yellow);
            //
            StartCoroutine(ActiveDutySender(DutyPlugin.Instance.Configuration.Instance.ActiveDutyList.Timer));
            //
            U.Events.OnPlayerConnected += OnPlayerConnected;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            DamageTool.damagePlayerRequested += DamageTool_damagePlayerRequested;
            R.Commands.OnExecuteCommand += OnCommandExecuted;

        }

        private void OnCommandExecuted(IRocketPlayer user, IRocketCommand command, ref bool cancel)
        {
            Logger.Log("Command Ran");
            Logger.Log(command.Name);
            if (user is UnturnedPlayer player)
            {
                Logger.Log("1");
                if (onDuty.ContainsKey(player.CSteamID) && !player.HasPermission(DutyPlugin.Instance.Configuration.Instance.RestrictionsHolder.BypassPermission))
                {
                    Logger.Log("2");
                    var IsCanceld = false;
                    foreach (var comm in DutyPlugin.Instance.Configuration.Instance.RestrictionsHolder.RestrictedCommand)
                    {
                        Logger.Log("3");
                        Logger.Log(command.Name);
                        Logger.Log(comm);
                        Logger.Log("----------------------------------------------------");
                        if (command.Name.Equals(comm, StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Log("4");
                            TranslationHelper.SendMessageTranslation(player.CSteamID, "RestrictedCommand", command.Name);
                            IsCanceld = true;
                            cancel = true;
                            break;
                        }
                    }
                    ThreadHelper.RunAsynchronously(() =>
                    {
                        WebhookMessage Embed = new WebhookMessage()
                        .PassEmbed()
                        .WithTitle(player.CharacterName + "**(" + player.CSteamID + ")**")
                        .WithColor(EmbedColor.Orange)
                        .WithURL("https://steamcommunity.com/profiles/" + player.CSteamID)
                        .WithTimestamp(DateTime.Now)
                        .WithDescription("**Command Executed**")
                        .WithField("**Command:**", command.Name)
                        .WithField("Cancled: ", IsCanceld.ToString())

                        .Finalize();
                        DiscordWebhookService.PostMessageAsync(DutyPlugin.Instance.Configuration.Instance.WebhookContainer.CommandWebhook, Embed);
                    });
                }
            }
        }

        private void DamageTool_damagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            var killer = UnturnedPlayer.FromCSteamID(parameters.killer);
            var victim = UnturnedPlayer.FromPlayer(parameters.player);
            if (killer == null || victim == null) return;
            if (onDuty.ContainsKey(killer.CSteamID) && DutyPlugin.Instance.Configuration.Instance.AllowDamageToPlayers)
            {
                DebugManager.SendDebugMessage("Canceling Damage from " + killer.CharacterName);
                TranslationHelper.SendMessageTranslation(killer.CSteamID, "DamageCanceled", victim.CharacterName);
                shouldAllow = false;
            }
        }

        private void OnPlayerConnected(UnturnedPlayer player)
        {
            DebugManager.SendDebugMessage(player.CharacterName + " has joined the Server. Checking for Duty Permissions");
            // Check if they are found in the Group. If so, Annouce that you have logged in and on duty
            if (onDuty.ContainsKey(player.CSteamID))
            {
                DebugManager.SendDebugMessage("onDuty Conmtains " + player.CharacterName + "CSteamID");
                var onDuty = DutyPlugin.Instance.onDuty[player.CSteamID];
                var dutyGroup = R.Permissions.GetGroup(onDuty.GroupID);
                if (dutyGroup == null)
                {
                    DebugManager.SendDebugMessage("Invalid Group");
                    Logger.LogWarning("----------------------------------------");
                    Logger.LogWarning("ERROR: Invalid Group: " + onDuty.GroupID);
                    Logger.LogWarning("ERROR: Invalid Group: " + onDuty.GroupID);
                    Logger.LogWarning("ERROR: Invalid Group: " + onDuty.GroupID);
                    Logger.LogWarning("----------------------------------------");
                    return;
                }
                if (!dutyGroup.Members.Contains(player.CSteamID.m_SteamID.ToString()))
                {
                    DebugManager.SendDebugMessage("Added " + player.CharacterName + " to " + onDuty.GroupID + " Group");
                    // Add them to Group, and Broadcast
                    R.Permissions.AddPlayerToGroup(onDuty.GroupID, player);
                    TranslationHelper.SendMessageTranslation(player.CSteamID, "Duty_OnDuty", onDuty.DutyName);
                    if (DutyPlugin.Instance.Configuration.Instance.ServerAnnouncer.Enabled && !player.HasPermission(DutyPlugin.Instance.Configuration.Instance.ServerAnnouncer.BypassPermission))
                    {
                        foreach (SteamPlayer steamPlayer in Provider.clients)
                        {
                            UnturnedPlayer user = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                            TranslationHelper.SendMessageTranslation(user.CSteamID, "Broadcast_OnDuty", player.CharacterName, onDuty.DutyName);
                        }
                    }
                }
                else
                {
                    DebugManager.SendDebugMessage(player.CharacterName + " already in " + onDuty.GroupID + ". Just broadcasting Message");
                    // Already In group, So just boradcast
                    if (DutyPlugin.Instance.Configuration.Instance.ServerAnnouncer.Enabled && !player.HasPermission(DutyPlugin.Instance.Configuration.Instance.ServerAnnouncer.BypassPermission))
                    {
                        foreach (SteamPlayer steamPlayer in Provider.clients)
                        {
                            UnturnedPlayer user = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                            TranslationHelper.SendMessageTranslation(user.CSteamID, "Broadcast_OnDuty", player.CharacterName, onDuty.DutyName);
                        }
                    }
                }
            }
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            DebugManager.SendDebugMessage(player.CharacterName + " has left the server. Checking Duty");
            if(DutyPlugin.Instance.Configuration.Instance.RemoveBlueHammerOnLogout && player.IsAdmin)
            {
                player.Admin(false);
                DebugManager.SendDebugMessage("Removing Admin from " + player.CharacterName + " on Logout");
            }
            if (DutyPlugin.Instance.Configuration.Instance.RemoveDutyOnLogout)
            {
                DebugManager.SendDebugMessage("Remove On Duty Logout set to TRUE");
                if (onDuty.ContainsKey(player.CSteamID))
                {
                    var offDuty = DutyPlugin.Instance.onDuty[player.CSteamID];
                    if (R.Permissions.GetGroup(offDuty.GroupID) == null)
                    {
                        DebugManager.SendDebugMessage("Invalid Group");
                        Logger.LogWarning("----------------------------------------");
                        Logger.LogWarning("ERROR: Invalid Group: " + offDuty.GroupID);
                        Logger.LogWarning("ERROR: Invalid Group: " + offDuty.GroupID);
                        Logger.LogWarning("ERROR: Invalid Group: " + offDuty.GroupID);
                        Logger.LogWarning("----------------------------------------");
                        return;
                    }
                    R.Permissions.RemovePlayerFromGroup(offDuty.GroupID, player);
                    if (!DutyPlugin.Instance.onDuty.TryGetValue(player.CSteamID, out DutyPlugin.OnDutyHolder value)) return;
                    if (DutyPlugin.Instance.Configuration.Instance.ServerAnnouncer.Enabled && !player.HasPermission(DutyPlugin.Instance.Configuration.Instance.ServerAnnouncer.BypassPermission))
                    {
                        foreach (SteamPlayer steamPlayer in Provider.clients)
                        {
                            UnturnedPlayer user = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                            TranslationHelper.SendMessageTranslation(user.CSteamID, "Broadcast_OffDuty", player.CharacterName, offDuty.DutyName);
                        }
                    }
                    DebugManager.SendDebugMessage("Player Logout - Sending Webhook");
                    ThreadHelper.RunAsynchronously(() =>
                    {
                        WebhookMessage Embed = new WebhookMessage()
                        .PassEmbed()
                        .WithTitle(player.CharacterName + " Duty Log")
                        .WithColor(EmbedColor.Red)
                        .WithURL("https://steamcommunity.com/profiles/" + player.CSteamID)
                        .WithTimestamp(DateTime.Now)
                        .WithField("**Username**", player.CharacterName + " \n(" + player.CSteamID + ")")
                        .WithField("**TimeClock Information**", "Start Date: <t:" + ((DateTimeOffset)TimeZoneInfo.ConvertTimeToUtc(value.StartDate)).ToUnixTimeSeconds().ToString() + ">\nEnd Date: <t:" + ((DateTimeOffset)TimeZoneInfo.ConvertTimeToUtc(DateTime.Now)).ToUnixTimeSeconds().ToString() + ">")
                        .WithField("**Total Time** ", "``" + TimeConverterManager.Format(TimeConverterManager.getTimeSpan(value.StartDate, DateTime.Now), 2) + "``")
                        .WithField("**Duty Information**", "Duty Name: ``" + offDuty.DutyName + "``\nPermission: ``" + offDuty.Permission + "``")
                        .Finalize();
                        DiscordWebhookService.PostMessageAsync(DutyPlugin.Instance.Configuration.Instance.WebhookContainer.DutyLogWebhook, Embed);
                    });
                    DutyPlugin.Instance.onDuty.Remove(player.CSteamID);
                    return;
                }
            }
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            DamageTool.damagePlayerRequested -= DamageTool_damagePlayerRequested;
            R.Commands.OnExecuteCommand -= OnCommandExecuted;
            Logger.Log("BTDuty Unloaded");
        }
        public class OnDutyHolder
        { 
            public string DutyName { get; set; }
            public string GroupID { get; set; }
            public string Permission { get; set; }
            public DateTime StartDate { get; set; }
        }

        public void OffDutyOnline(UnturnedPlayer player)
        {
            var offDuty = DutyPlugin.Instance.onDuty[player.CSteamID];
            if(R.Permissions.GetGroup(offDuty.GroupID) == null)
            {
                Logger.LogWarning("----------------------------------------");
                Logger.LogWarning("ERROR: Invalid Group: " + offDuty.GroupID);
                Logger.LogWarning("ERROR: Invalid Group: " + offDuty.GroupID);
                Logger.LogWarning("ERROR: Invalid Group: " + offDuty.GroupID);
                Logger.LogWarning("----------------------------------------");
                TranslationHelper.SendMessageTranslation(player.CSteamID, "ErrorContactStaff");
                return;
            }
            R.Permissions.RemovePlayerFromGroup(offDuty.GroupID, player);
            if (!DutyPlugin.Instance.onDuty.TryGetValue(player.CSteamID, out DutyPlugin.OnDutyHolder value)) return;
            onDuty.Remove(player.CSteamID);
            TranslationHelper.SendMessageTranslation(player.CSteamID, "Duty_OffDuty", value.DutyName, TimeConverterManager.Format(TimeConverterManager.getTimeSpan(value.StartDate, DateTime.Now), 2));
            if (player.IsAdmin)
            {
                player.Admin(false);
                DebugManager.SendDebugMessage("Removing BlueHammer for " + player.CharacterName);
            }
            //
            player.Features.GodMode = false;
            player.Features.VanishMode = false;
            //
            if (DutyPlugin.Instance.Configuration.Instance.ServerAnnouncer.Enabled && !player.HasPermission(DutyPlugin.Instance.Configuration.Instance.ServerAnnouncer.BypassPermission))
            {
                foreach (SteamPlayer steamPlayer in Provider.clients)
                {
                    UnturnedPlayer user = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                    TranslationHelper.SendMessageTranslation(user.CSteamID, "Broadcast_OffDuty", player.CharacterName, offDuty.DutyName);
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
                .WithField("**TimeClock Information**", "Start Date: <t:" + ((DateTimeOffset)TimeZoneInfo.ConvertTimeToUtc(value.StartDate)).ToUnixTimeSeconds().ToString() + ">\nEnd Date: <t:" + ((DateTimeOffset)TimeZoneInfo.ConvertTimeToUtc(DateTime.Now)).ToUnixTimeSeconds().ToString() + ">")
                .WithField("**Total Time** ", "``" + TimeConverterManager.Format(TimeConverterManager.getTimeSpan(value.StartDate, DateTime.Now), 2) + "``")
                .WithField("**Duty Information**", "Duty Name: ``" + offDuty.DutyName + "``\nPermission: ``" + offDuty.Permission + "``")
                .Finalize();
                DiscordWebhookService.PostMessageAsync(DutyPlugin.Instance.Configuration.Instance.WebhookContainer.DutyLogWebhook, Embed);
            });
            DutyPlugin.Instance.onDuty.Remove(player.CSteamID);
            return;
        }

        public IEnumerator ActiveDutySender(float time)
        {
            while (DutyPlugin.Instance.Configuration.Instance.ActiveDutyList.Enabled)
            {
                if (onDuty.Count == 0)
                {
                    //Do something
                    yield return new WaitForSeconds(time);
                    continue;
                }
                var sb = new StringBuilder();
                foreach (var duty in onDuty)
                {
                    if(onDuty.Count == 1)
                    {
                        sb.AppendLine($"**Username:** ``{PlayerTool.getPlayer(duty.Key).channel.owner.playerID.playerName}``");
                        sb.AppendLine($"**Position:** ``{duty.Value.DutyName}``");
                        sb.AppendLine($"**Time:** ``{TimeConverterManager.Format(TimeConverterManager.getTimeSpan(duty.Value.StartDate, DateTime.Now), 2)}``");
                        break;
                    }
                    sb.AppendLine($"**Username:** ``{PlayerTool.getPlayer(duty.Key).channel.owner.playerID.playerName}``");
                    sb.AppendLine($"**Position:** ``{duty.Value.DutyName}``");
                    sb.AppendLine($"**Time:** ``{TimeConverterManager.Format(TimeConverterManager.getTimeSpan(duty.Value.StartDate, DateTime.Now), 2)}``");
                    sb.AppendLine("-------------------------------------");
                }
                var ip = Provider.ip;
                var bytes = BitConverter.GetBytes(ip);
                var serverIP = $"{bytes[0]}.{bytes[1]}.{bytes[2]}.{bytes[3]}";
                ThreadHelper.RunAsynchronously(() =>
                {
                    var embed = new WebhookMessage()
                    .PassEmbed()
                    .WithTitle("Active Duty List")
                    .WithColor(EmbedColor.Blue)
                    .WithDescription("**Server IP:** " + serverIP + "\n **Server Port:** " + Provider.port + "\n\n**[ Duty List ]**\n\n" + sb.ToString());
                    embed.footer = new WebhookFooter() { text = "BTDuty - " + DateTime.Now.ToString("dddd, dd MMMM yyyy") + "" };
                    var send = embed.Finalize();
                    DiscordWebhookService.PostMessageAsync(DutyPlugin.Instance.Configuration.Instance.WebhookContainer.ActiveDutyWebhook, send);
                });
                yield return new WaitForSeconds(time);
            }
        }
    }
}
