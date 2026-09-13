using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Lunarbin.Valheim.ExpandedBoatMinimap
{
    [BepInPlugin("lunarbin.games.valheim.expanded-boat-minimap", "Valheim Expanded Boat Minimap", BuildInfo.Version)]
    public class ExpandedBoatMinimap : BaseUnityPlugin
    {
        public const string PluginName = "Expanded Boat Minimap";
        public static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("ExpandedBoatMinimap");
        private readonly Harmony harmony = new Harmony("lunarbin.games.valheim.expanded-boat-minimap");

        private static float _defaultMinimapExploreRadius = 0f;


        private static ConfigEntry<float> _drakkarRadius;
        private static ConfigEntry<float> _longshipRadius;
        private static ConfigEntry<float> _karveRadius;
        private static ConfigEntry<float> _raftRadius;
        private static ConfigEntry<bool> _notifyOnChange;

        private void Awake()
        {
            harmony.PatchAll();
            _drakkarRadius = Config.Bind("General", // Section
                "DrakkarRadius", // Key
                150f, // Default
                "The Drakkar's minimap explore radius. Mod default = 150; Game default = 50" // Description
            );
            _longshipRadius = Config.Bind("General", // Section
                "LongshipRadius", // Key
                150f, // Default
                "The Longship's minimap explore radius. Mod default = 150; Game default = 50" // Description
            ); // Bind config.
            _karveRadius = Config.Bind("General", "KarveRadius", 120f,
                "The Karve's minimap explore radius. Mod default = 120; Game default = 50");
            _raftRadius = Config.Bind("General", "RaftRadius", 80f,
                "The Raft's minimap explore radius. Mod default = 80; Game default = 50");
            _notifyOnChange = Config.Bind("General", "NotifyOnChange", false,
                "Notify the player when their explore radius changes.");
        }

        // When the player gets on a ship, triple their minimap explore radius.
        [HarmonyPatch(typeof(Ship), "OnTriggerEnter")]
        internal class PatchShipOnTriggerEnter
        {
            private static void Postfix(Collider collider, Ship __instance)
            {
                Player player = collider.GetComponent<Player>();
                if (null != player && player == Player.m_localPlayer)
                {
                    if (_defaultMinimapExploreRadius == 0f)
                    {
                        _defaultMinimapExploreRadius = Minimap.instance.m_exploreRadius;
                    }

                    if (__instance.name.Contains("VikingShip_Ashlands"))
                    {
                        Minimap.instance.m_exploreRadius = _drakkarRadius.Value;
                    }
                    else if (__instance.name.Contains("VikingShip"))
                    {
                        Minimap.instance.m_exploreRadius = _longshipRadius.Value;
                    }
                    else if (__instance.name.Contains("Karve"))
                    {
                        Minimap.instance.m_exploreRadius = _karveRadius.Value;
                    }
                    else if (__instance.name.Contains("Raft"))
                    {
                        Minimap.instance.m_exploreRadius = _raftRadius.Value;
                    }

                    if (_notifyOnChange.Value)
                    {
                        
                        MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft,
                            $"Minimap explore radius changed to {Minimap.instance.m_exploreRadius}");
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
                Player player = collider.GetComponent<Player>();
                if ((bool)player)
                {
                    if (player == Player.m_localPlayer)
                    {
                        if (_defaultMinimapExploreRadius != 0f)
                        {
                            Minimap.instance.m_exploreRadius = _defaultMinimapExploreRadius;
                            if (_notifyOnChange.Value)
                            {
                                MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft,
                                    $"Minimap explore radius returned to default: {Minimap.instance.m_exploreRadius}");
                            }
                        }
                    }
                }
            }
        }
    }
}