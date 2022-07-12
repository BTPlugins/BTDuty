using Steamworks;
using Logger = Rocket.Core.Logging.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDG.Unturned;
using Rocket.API.Collections;
using UnityEngine;

namespace BTDuty.Helpers
{
    public class TranslationHelper
    {
        public static void SendMessageTranslation(CSteamID playerId, string translationKey, params object[] placeholder)
        {
            if (!PlayerHelper.isPlayerOnline(playerId))
            {
                Logger.LogError(GetTranslation("TargetNotFound", playerId));
                return;
            }

            if (string.IsNullOrEmpty(translationKey) || string.IsNullOrWhiteSpace(translationKey))
            {
                Logger.LogError(GetTranslation("TranslationKeyEmpty"));
                return;
            }

            SendMessage(PlayerTool.getSteamPlayer(playerId), GetTranslation(translationKey, placeholder), Color.white, null);
        }
        private static string GetTranslation(string key, params object[] placeholder)
        {
            return string.Format(DefaultTranslations[key], placeholder);
        }
        private static TranslationList DefaultTranslations => DutyPlugin.Instance.Translations.Instance;
        private static void SendMessage(SteamPlayer target, string message, UnityEngine.Color color, SteamPlayer sender = default)
        {
            ChatManager.serverSendMessage(message.Replace("[", "<").Replace("]", ">").Replace("{", "[").Replace("}", "]"), color, sender, target, EChatMode.SAY, null, true);
        }
    }
}
