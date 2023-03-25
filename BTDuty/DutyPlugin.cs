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
using Rocket.Unturned.Enumerations;
using Rocket.Core.Assets;
using BTDuty.Storage;
using System.IO;

namespace BTDuty
{
    public partial class DutyPlugin : RocketPlugin<BTDutyConfiguration>
    {
        public static DutyPlugin Instance;
        public IDictionary<CSteamID, OnDutyHolder> onDuty = new Dictionary<CSteamID, OnDutyHolder>();
        public XMLFileAsset<TimeclockLogger> TimeClock { get; set; }
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
            TimeClock = new XMLFileAsset<TimeclockLogger>(Path.Combine(Directory, $"{Name}.TimeClock.xml"));
            TimeClock.Load();
            SaveManager.onPreSave += OnPreSave;
            //
            U.Events.OnPlayerConnected += OnPlayerConnected;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            DamageTool.damagePlayerRequested += DamageTool_damagePlayerRequested;
            R.Commands.OnExecuteCommand += OnCommandExecuted;
            UnturnedPlayerEvents.OnPlayerInventoryAdded += OnPlayerInventoryAdded;
        }

        private void OnPreSave()
        {
            TimeClock.Save();
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            DamageTool.damagePlayerRequested -= DamageTool_damagePlayerRequested;
            R.Commands.OnExecuteCommand -= OnCommandExecuted;
            UnturnedPlayerEvents.OnPlayerInventoryAdded -= OnPlayerInventoryAdded;
            Logger.Log("BTDuty Unloaded");
        }

        private void OnPlayerInventoryAdded(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        {
            DebugManager.SendDebugMessage("Item Added to Inventory");
            if (onDuty.ContainsKey(player.CSteamID))
            {
                DebugManager.SendDebugMessage("Sending Webhook");
                ThreadHelper.RunAsynchronously(async () =>
                {
                    var embed = new WebhookMessage()
                    .PassEmbed()
                    .WithTitle(player.CharacterName + " **(" + player.CSteamID + ")**")
                    .WithColor(EmbedColor.GreenYellow)
                    .WithThumbnail(player.SteamProfile.AvatarFull.AbsoluteUri)
                    .WithURL("https://steamcommunity.com/profiles/" + player.CSteamID)
                    .WithDescription("**Item Added**")
                    .WithField("**Item Name**", Assets.find(EAssetType.ITEM, P.item.id)?.FriendlyName)
                    .WithField("**Item ID**", P.item.id.ToString());
                    embed.footer = new WebhookFooter() { text = "[BTDuty] " + Provider.serverName + " - " + DateTime.Now.ToString("dddd, dd MMMM yyyy") + "" };
                    var send = embed.Finalize();
                    await DiscordWebhookService.PostMessageAsync(DutyPlugin.Instance.Configuration.Instance.WebhookContainer.ItemAddedWebhook, send);
                });
            }
        }

        private void OnCommandExecuted(IRocketPlayer user, IRocketCommand command, ref bool cancel)
        {
            DebugManager.SendDebugMessage("Command Executed");
            if (user is UnturnedPlayer player)
            {
                if (onDuty.ContainsKey(player.CSteamID) && !(R.Permissions.GetPermissions(player, DutyPlugin.Instance.Configuration.Instance.RestrictionsHolder.BypassPermission).Count != 0))
                {
                    var IsCanceld = false;
                    foreach (var comm in DutyPlugin.Instance.Configuration.Instance.RestrictionsHolder.RestrictedCommand)
                    {
                        DebugManager.SendDebugMessage("Real Command: " + command.Name);
                        DebugManager.SendDebugMessage("Configuration Command: " + comm);
                        DebugManager.SendDebugMessage("----------------------------------------------------");
                        if (command.Name.Equals(comm, StringComparison.OrdinalIgnoreCase))
                        {
                            DebugManager.SendDebugMessage("Restricted Command: " + player.CharacterName + " tried using " + command.Name + " while on Duty!");
                            TranslationHelper.SendMessageTranslation(player.CSteamID, "RestrictedCommand", command.Name);
                            IsCanceld = true;
                            cancel = true;
                            break;
                        }
                    }
                    ThreadHelper.RunAsynchronously(async () =>
                    {
                        var embed = new WebhookMessage()
                        .PassEmbed()
                        .WithTitle(player.CharacterName + " **(" + player.CSteamID + ")**")
                        .WithColor(EmbedColor.Orange)
                        .WithThumbnail(player.SteamProfile.AvatarFull.AbsoluteUri)
                        .WithURL("https://steamcommunity.com/profiles/" + player.CSteamID)
                        .WithDescription("**Command Executed**")
                        .WithField("**Command:**", command.Name)
                        .WithField("Cancled: ", IsCanceld.ToString());
                        embed.footer = new WebhookFooter() { text = "[BTDuty] " + Provider.serverName + " - " + DateTime.Now.ToString("dddd, dd MMMM yyyy") + "" };
                        var send = embed.Finalize();
                        await DiscordWebhookService.PostMessageAsync(DutyPlugin.Instance.Configuration.Instance.WebhookContainer.CommandWebhook, send);
                    });
                }
            }
        }

        private void DamageTool_damagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            var kil = PlayerTool.getPlayer(parameters.killer);
            if (kil == null) return;
            var killer = UnturnedPlayer.FromPlayer(kil);
            var victim = UnturnedPlayer.FromPlayer(parameters.player);
            if (onDuty.ContainsKey(killer.CSteamID) && !onDuty[killer.CSteamID].AllowDamageToPlayers)
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
                    TimeClock.Instance.TimeClock.Add(new Modules.PlayerTimeclock { playerID = player.CSteamID.m_SteamID, DutyGroup = offDuty.GroupID, DutyName = offDuty.DutyName, StartDate = offDuty.StartDate, EndDate = DateTime.Now, TotalTime = (int)(DateTime.Now - offDuty.StartDate).TotalSeconds });
                    TimeClock.Save();
                    var playerTimeClock = Instance.TimeClock.Instance.TimeClock.Where(c => c.playerID == player.CSteamID.m_SteamID);
                    var sessionTime = TimeConverterManager.Format(TimeConverterManager.getTimeSpan(value.StartDate, DateTime.Now), 2);
                    var weekTime = TimeConverterManager.Format(TimeSpan.FromSeconds(playerTimeClock.Where(v => (v.EndDate - DateTime.Now).TotalDays < 7).Sum(b => b.TotalTime)), 2);
                    var totalTime = TimeConverterManager.Format(TimeSpan.FromSeconds(playerTimeClock.Sum(b => b.TotalTime)), 2);
                    var timeToday = TimeConverterManager.Format(TimeSpan.FromSeconds(playerTimeClock.Where(v => v.EndDate.Date == DateTime.Now.Date).Sum(c => c.TotalTime)), 2);
                    DebugManager.SendDebugMessage("Player Logout - Sending Webhook");
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
                        .WithField("**Duty Information**", "Duty Name: ``" + offDuty.DutyName + "``\nPermission: ``" + offDuty.Permission + "``");
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
            }
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
            //
            TimeClock.Instance.TimeClock.Add(new Modules.PlayerTimeclock { playerID = player.CSteamID.m_SteamID, DutyGroup = offDuty.GroupID, DutyName = offDuty.DutyName, StartDate = offDuty.StartDate, EndDate = DateTime.Now, TotalTime = (int)(DateTime.Now - offDuty.StartDate).TotalSeconds });
            //
            TimeClock.Save();
            var playerTimeClock = Instance.TimeClock.Instance.TimeClock.Where(c => c.playerID == player.CSteamID.m_SteamID);
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
                .WithField("**Duty Information**", "Duty Name: ``" + offDuty.DutyName + "``\nPermission: ``" + offDuty.Permission + "``");
                embed.footer = new WebhookFooter() { text = "[BTDuty] " + Provider.serverName + " - " + DateTime.Now.ToString("dddd, dd MMMM yyyy") + "" };
                var send = embed.Finalize();
                await DiscordWebhookService.PostMessageAsync(DutyPlugin.Instance.Configuration.Instance.WebhookContainer.DutyLogWebhook, send);
                //
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

            onDuty.Remove(player.CSteamID);
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
                    if (onDuty.Count == 1)
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
                ThreadHelper.RunAsynchronously(async () =>
                {
                    var embed = new WebhookMessage()
                    .PassEmbed()
                    .WithTitle("Active Duty List")
                    .WithColor(EmbedColor.Blue)
                    .WithDescription("**Server IP:** " + serverIP + "\n **Server Port:** " + Provider.port + "\n\n**[ Duty List ]**\n\n" + sb.ToString());
                    embed.footer = new WebhookFooter() { text = "[BTDuty] " + Provider.serverName + " - " + DateTime.Now.ToString("dddd, dd MMMM yyyy") + "" };
                    var send = embed.Finalize();
                    await DiscordWebhookService.PostMessageAsync(DutyPlugin.Instance.Configuration.Instance.WebhookContainer.ActiveDutyWebhook, send);
                });
                yield return new WaitForSeconds(time);
            }
        }

        public class OnDutyHolder
        {
            public string DutyName { get; set; }
            public string GroupID { get; set; }
            public string Permission { get; set; }
            public bool BlueHammer { get; set; }
            public bool GodMode { get; set; }
            public bool Vanish { get; set; }
            public bool AllowDamageToPlayers { get; set; }
            public DateTime StartDate { get; set; }
        }
    }
}
