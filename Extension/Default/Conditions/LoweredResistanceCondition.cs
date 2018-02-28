﻿using ImGuiNET;
using PoeHUD.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeRoutine.Menu;
using TreeSharp;

namespace TreeRoutine.Routine.BuildYourOwnRoutine.Extension.Default.Conditions
{
    internal class LoweredResistanceCondition : ExtensionCondition
    {
        private static Dictionary<String, Tuple<PlayerStats, PlayerStats>> resistanceTypes = new Dictionary<String, Tuple<PlayerStats, PlayerStats>>()
        {
            { "Cold", Tuple.Create(PlayerStats.ColdDamageResistancePct, PlayerStats.MaximumColdDamageResistancePct) },
            { "Fire", Tuple.Create(PlayerStats.FireDamageResistancePct, PlayerStats.MaximumFireDamageResistancePct) },
            { "Lightning",Tuple.Create(PlayerStats.LightningDamageResistancePct, PlayerStats.LightningDamageResistancePct) },
            { "Chaos", Tuple.Create(PlayerStats.ChaosDamageResistancePct, PlayerStats.MaximumChaosDamageResistancePct) }
        };

        private Boolean CheckCold { get; set; }
        private readonly String CheckColdString = "CheckCold";

        private Boolean CheckFire { get; set; }
        private readonly String CheckFireString = "CheckFire";

        private Boolean CheckLightning { get; set; }
        private readonly String CheckLightningString = "CheckLightning";

        private Boolean CheckChaos { get; set; }
        private readonly String CheckChaosString = "CheckChaos";

        private int ResistanceThreshold { get; set; }
        private readonly String ResistanceThresholdString = "ResistanceThreshold";

        public LoweredResistanceCondition(string owner, string name) : base(owner, name)
        {
            CheckCold = false;
            CheckFire = false;
            CheckLightning = false;
            CheckChaos = false;
        }

        public override void Initialise(Dictionary<String, Object> Parameters)
        {
            base.Initialise(Parameters);

            CheckCold = Boolean.Parse((string)Parameters[CheckColdString]);
            CheckFire = Boolean.Parse((string)Parameters[CheckFireString]);
            CheckLightning = Boolean.Parse((string)Parameters[CheckLightningString]);
            CheckChaos = Boolean.Parse((string)Parameters[CheckChaosString]);
            ResistanceThreshold = Int32.Parse((string)Parameters[ResistanceThresholdString]);
        }

        public override bool CreateConfigurationMenu(ref Dictionary<String, Object> Parameters)
        {
            ImGui.TextDisabled("Condition Info");
            ImGuiExtension.ToolTip("This condition will return true if any of the selected player's resistances\nare reduced by more than or equal to the specified amount.\nReduced max resistance modifiers are taken into effect automatically (e.g. -res map mods).");


            base.CreateConfigurationMenu(ref Parameters);

            CheckCold = ImGuiExtension.Checkbox("Cold", CheckCold);
            Parameters[CheckColdString] = CheckCold.ToString();

            CheckFire = ImGuiExtension.Checkbox("Fire", CheckFire);
            Parameters[CheckFireString] = CheckFire.ToString();

            CheckLightning = ImGuiExtension.Checkbox("Lightning", CheckLightning);
            Parameters[CheckLightningString] = CheckLightning.ToString();

            CheckChaos = ImGuiExtension.Checkbox("Chaos", CheckChaos);
            Parameters[CheckChaosString] = CheckChaos.ToString();

            ResistanceThreshold = ImGuiExtension.IntSlider("Resistance Threshold", ResistanceThreshold, 0, 125);
            Parameters[ResistanceThresholdString] = ResistanceThreshold.ToString();

            return true;
        }

        public override Func<bool> GetCondition(ExtensionParameter profileParameter)
        {
            return () =>
            {
                bool finalResult = true;

                if (finalResult && CheckCold)
                    finalResult = CheckResistance(profileParameter, "Cold");
                if (finalResult && CheckFire)
                    finalResult = CheckResistance(profileParameter, "Fire");
                if (finalResult && CheckLightning)
                    finalResult = CheckResistance(profileParameter, "Lightning");
                if (finalResult && CheckChaos)
                    finalResult = CheckResistance(profileParameter, "Chaos");

                return finalResult;
            };
        }

        private bool CheckResistance(ExtensionParameter profileParameter, string key)
        {
            // Initialize to 0
            int? current = 0;
            int? maximum = 0;

            Tuple<PlayerStats, PlayerStats> playerStat;
            if (resistanceTypes.TryGetValue(key, out playerStat))
            {
                // Get the stats
                current = profileParameter.Plugin.PlayerHelper.getPlayerStat(playerStat.Item1) ?? 0;
                maximum = profileParameter.Plugin.PlayerHelper.getPlayerStat(playerStat.Item2) ?? current;
                if (maximum < current)
                    maximum = current;
            }

            // Adjust the Resistance Threshold by any reduced res mods (such as on a map)
            if (current >= ResistanceThreshold - (maximum - current))
            {
                return false;
            }
            return true;
        }
    }
}
