using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using Overlayer;
using Overlayer.Patches;
using UnityEngine;
using UnityModManagerNet;
using System.Runtime.CompilerServices;
using System.Reflection;
using UnityEngine.UI;
using System.Security.Cryptography.Xml;
using UnityEngine.UIElements;
using ADOFAI;
using RDTools;

namespace ShowTimingWindowScale
{
    public static class Patch
    {
        private static bool first = true;

        [HarmonyPatch(typeof(scrCalibrationPlanet), "Start")]
        public static class scrCalibrationPlanet_Start_r94
        {
            public static void Postfix()
            {
                resetText();
            }
        }

        [HarmonyPatch(typeof(scrUIController), "WipeToBlack")]
        public static class scrUIController_WipeToBlack_Patch_r94
        {
            public static void Postfix()
            {
                resetText();
            }
        }

        [HarmonyPatch(typeof(scnEditor), "ResetScene")]
        public static class scnEditor_ResetScene_Patch_r94
        {
            public static void Postfix()
            {
                resetText();
            }
        }

        [HarmonyPatch(typeof(scrController), "StartLoadingScene")]
        public static class scrController_StartLoadingScene_Patch_r94
        {
            public static void Postfix()
            {
                resetText();
            }
        }

        [HarmonyPatch(typeof(CustomLevel), "Play")]
        public static class CustomLevelStart_r94
        {
            public static void Postfix(CustomLevel __instance)
            {
                onPlayPostfix(__instance);
            }
        }

        [HarmonyPatch(typeof(scrPressToStart), "ShowText")]
        public static class BossLevelStart_r94
        {
            public static void Postfix(scrPressToStart __instance)
            {
                onShowText(__instance);
            }
        }

        [HarmonyPatch(typeof(scrPlanet), "MoveToNextFloor")]
        public static class MoveToNextFloor_r94
        {
            public static void Postfix(scrPlanet __instance, scrFloor floor)
            {
                onMoveToNextFloor(__instance, floor);
            }
        }

        [HarmonyPatch(typeof(RDString), "ChangeLanguage")]
        public static class ChangeLanguage_r94
        {
            public static void Prefix(SystemLanguage language)
            {
                Main.gui.text.font = RDString.GetFontDataForLanguage(language).font;
            }
        }

        [HarmonyPatch(typeof(scrController), "Awake")]
        public static class Awake_r94
        {
            public static void Prefix()
            {
                onAwake();
            }
        }

        /*******************Private Methods*************************/

        private static void onAwake()
        {
            if (first)
            {
                first = false;
                Main.gui.text.font = RDString.GetFontDataForLanguage(RDString.language).font;
            }
        }

        private static void onMoveToNextFloor(scrPlanet __instance, scrFloor floor)
        {
            if (!Main.IsEnabled) return;
            if (!scrController.instance.gameworld) return;
            if (floor.nextfloor == null) return;
            List<string> texts = new List<string>();

            //curBPM *= isTwirl? (2.0/scrController.instance.planetList.Count):(scrController.instance.planetList.Count*0.5);
            if (Main.setting.onShowHitMargin)
            {
                if (!Main.setting.text4.Contains(@"%") && !Main.setting.text4.Contains("100") && Main.setting.text4.Contains("{value}"))
                {
                    texts.Add(Main.setting.text4.Replace("{value}", Math.Round(scrController.instance.currFloor.marginScale * 100).ToString() + "%"));
                }
                else
                {
                    texts.Add("<color=#dc143c>Invalid Text</color>");
                }
            }

            Main.gui.setText(string.Join("\n", texts));
        }

        private static void onShowText(scrPressToStart __instance)
        {
            if (!Main.IsEnabled) return;
            if (!scrController.instance.gameworld) return;

            LevelStart(scrController.instance);
        }

        private static void onPlayPostfix(CustomLevel __instance) {
            if (!Main.IsEnabled) return;
            if (!scrController.instance.gameworld) return;
            if (__instance == null) return;

            LevelStart(scrController.instance);
        }

        private static void resetText()
        {
            if (!Main.IsEnabled) return;
            Main.gui.TextObject.SetActive(false);
        }
        
        private static string Repeat(string value, int count)
        {
            return new StringBuilder(value.Length * count).Insert(0, value, count).ToString();
        }

        private static string format(float v)
        {
            return string.Format("{0:0." + Repeat(Main.setting.zero? "0":"#", Main.setting.showDecimal) + "}", v);
        }

        private static void LevelStart(scrController __instance)
        {
            Main.gui.TextObject.SetActive(true);
            List<string> texts = new List<string>();

            if (Main.setting.onShowHitMargin)
            {
                if (!Main.setting.text4.Contains(@"%") && !Main.setting.text4.Contains("100") && Main.setting.text4.Contains("{value}"))
                {
                    texts.Add(Main.setting.text4.Replace("{value}", "100%"));
                }
                else
                {
                    texts.Add("<color=#dc143c>Invalid Text</color>");
                }
            }

            Main.gui.setText(string.Join("\n", texts));
            Main.gui.setSize(Main.setting.size);
        }

    }
}
