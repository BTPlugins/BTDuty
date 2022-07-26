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
    public class ListTagsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "ListTags";

        public string Help => "ListTags";

        public string Syntax => "ListTags";

        public List<string> Aliases => new List<string>() { "DutyTags"};

        public List<string> Permissions => new List<string>() { "BTDuty.ListTags" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = (UnturnedPlayer)caller;
            var tagText = string.Join(" [color=#F3F3F3]|[/color] ", DutyPlugin.Instance.Configuration.Instance.DutyGroups.Where(k => player.HasPermission(k.Permission)).Select(v => v.DutyName));
            TranslationHelper.SendMessageTranslation(player.CSteamID, "ListTags_List", tagText);
        }
    }
}
