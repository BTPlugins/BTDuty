using BTDuty.Helpers;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BTDuty.Commands
{
    public class TimeClockCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "TimeClock";

        public string Help => "TimeClock";

        public string Syntax => "TimeClock <Target>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "BTDuty.TimeClock" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = (UnturnedPlayer)caller;
            if(command.Length < 1)
            {
                TranslationHelper.SendMessageTranslation(player.CSteamID, "ProperUsage", "/TimeClock <Target>");
                return;
            }
            var target = UnturnedPlayer.FromName(command[0]);
            if (target == null)
            {
                TranslationHelper.SendMessageTranslation(player.CSteamID, "TargetNotFound"); 
                return;
            }
            if(!DutyPlugin.Instance.onDuty.TryGetValue(player.CSteamID, out var duty))
            {
                // Not on duty
                TranslationHelper.SendMessageTranslation(player.CSteamID, "TimeClock_NotOnDuty", target.CharacterName);
                return;
            }
            TranslationHelper.SendMessageTranslation(player.CSteamID, "TimeClock_OnDuty", target.CharacterName, duty.DutyName, TimeConverterManager.Format(TimeConverterManager.getTimeSpan(duty.StartDate, DateTime.Now), 2));
            DebugManager.SendDebugMessage(player.CharacterName + " has checked " + player.CharacterName + " TimeClock: " + TimeConverterManager.Format(TimeConverterManager.getTimeSpan(duty.StartDate, DateTime.Now), 2));
            // I hope I Fixed it all
        }
    }
}
