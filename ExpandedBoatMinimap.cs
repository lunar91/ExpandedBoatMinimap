using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Lunarbin.Valheim.CrossServerPortals;

[BepInPlugin("lunarbin.games.valheim.expanded-boat-minimap", "Valheim Expanded Boat Miniimap", "0.1.0")]
public class ExpandedBoatMinimap : BaseUnityPlugin
{
    public const string pluginName = "Expanded Boat Minimap";
    public static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("ExpandedBoatMinimap");
    private readonly Harmony harmony = new Harmony("lunarbin.games.valheim.expanded-boat-minimap");

    private static float defaultMinimapExploreRadius = 0f;


    private static ConfigEntry<float> longshipRadius;
    private static ConfigEntry<float> karveRadius;
    private static ConfigEntry<float> raftRadius;
    private static ConfigEntry<bool> notifyOnChange;

    private void Awake()
    {
        harmony.PatchAll();

        longshipRadius = Config.Bind("General", // Section
                                     "LongshipRadius", // Key
                                     150f, // Default
                                     "The Longship's minimap explore radius. Mod default = 150; Game default = 50" // Description
                                     ); // Bind config.
        karveRadius = Config.Bind("General", "KarveRadius", 120f, "The Karve's minimap explore radius. Mod default = 120; Game default = 50");
        raftRadius = Config.Bind("General", "RaftRadius", 80f, "The Raft's minimap explore radius. Mod default = 80; Game default = 50");
        notifyOnChange = Config.Bind("General", "NotifyOnChange", false, "Notify the player when their explore radius changes.");
    }

    // When the player gets on a ship, triple their minimap explore radius.
    [HarmonyPatch(typeof(Ship), "OnTriggerEnter")]
    internal class PatchShipOnTriggerEnter
    {
        private static void Postfix(Collider collider, Ship __instance)
        {
            Player player = ((Component)(object)collider).GetComponent<Player>();
            if ((bool)player)
            {
                if (player == Player.m_localPlayer)
                {
                    if (defaultMinimapExploreRadius == 0f)
                    {
                        player.Message(MessageHud.MessageType.Center, $"Minimap explore radius before change: {Minimap.instance.m_exploreRadius}");
                        defaultMinimapExploreRadius = Minimap.instance.m_exploreRadius;
                    }

                    if (__instance.name.Contains("VikingShip"))
                    {
                        Minimap.instance.m_exploreRadius = longshipRadius.Value;
                    }
                    else if (__instance.name.Contains("Karve"))
                    {
                        Minimap.instance.m_exploreRadius = karveRadius.Value;
                    }
                    else if (__instance.name.Contains("Raft"))
                    {
                        Minimap.instance.m_exploreRadius = raftRadius.Value;
                    }
                    if (notifyOnChange.Value)
                        player.Message(MessageHud.MessageType.TopLeft, $"Minimap explore radius changed to {Minimap.instance.m_exploreRadius}");
                }
            }
        }
    }

    // When the player gets off a ship, return their minimap explore radius to normal.
    [HarmonyPatch(typeof(Ship), "OnTriggerExit")]
    internal class PatchShipOnTriggerExit
    {
        private static void Postfix(Collider collider, Ship __instance)
        {
            Player player = ((Component)(object)collider).GetComponent<Player>();
            if ((bool)player)
            {
                if (player == Player.m_localPlayer)
                {
                    if (defaultMinimapExploreRadius != 0f)
                    {
                        Minimap.instance.m_exploreRadius = defaultMinimapExploreRadius;
                        if (notifyOnChange.Value)
                            player.Message(MessageHud.MessageType.TopLeft, $"Minimap explore radius returned to default: {Minimap.instance.m_exploreRadius}");
                    }
                }
            }
        }
    }
}

