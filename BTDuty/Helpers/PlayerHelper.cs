using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTDuty.Helpers
{
    public static class PlayerHelper
    {
        public static bool isPlayerOnline(CSteamID SteamId)
        {
            return Provider.clients.Select(x => x.playerID.steamID).Contains(SteamId);
        }
        public static string getPlayerName(CSteamID SteamId)
        {
            return PlayerTool.getPlayer(SteamId).channel.owner.playerID.playerName;
        }
    }
}
