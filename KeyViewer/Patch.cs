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

namespace ShowHitMargin_Eng
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

        [HarmonyPatch(typeof(scrMisc), "GetHitMargin")]
        public static class GetHitMarginFixer_r94
        {
            public static bool Prefix(float hitangle, float refangle, bool isCW, float bpmTimesSpeed, float conductorPitch, double marginScale, ref HitMargin __result)
            {
                __result = GetHitMargin(GCS.difficulty, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                try
                {
                    if (Main.overlayer && UnityModManager.FindMod("Overlayer") != null && UnityModManager.FindMod("Overlayer").Active)
                    {
                        onOverlayerHitMargin(hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale, __result);
                    }
                }catch  { }
                return false;
            }
        }

        /*******************Private Methods*************************/

        private static void onOverlayerHitMargin(float hitangle, float refangle, bool isCW, float bpmTimesSpeed, float conductorPitch, double marginScale, HitMargin __result)
        {
            scrController instance = scrController.instance;
            Variables.Lenient = GetHitMargin(Difficulty.Lenient, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
            Variables.Normal = GetHitMargin(Difficulty.Normal, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
            Variables.Strict = GetHitMargin(Difficulty.Strict, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
            if (!Variables.Lenient.SafeMargin())
            {
                Dictionary<HitMargin, int> lenientCounts = Variables.LenientCounts;
                HitMargin key = Variables.Lenient;
                int num = lenientCounts[key];
                lenientCounts[key] = num + 1;
            }
            if (!Variables.Normal.SafeMargin())
            {
                Dictionary<HitMargin, int> normalCounts = Variables.NormalCounts;
                HitMargin key = Variables.Normal;
                int num = normalCounts[key];
                normalCounts[key] = num + 1;
            }
            if (!Variables.Strict.SafeMargin())
            {
                Dictionary<HitMargin, int> strictCounts = Variables.StrictCounts;
                HitMargin key = Variables.Strict;
                int num = strictCounts[key];
                strictCounts[key] = num + 1;
            }
            if (__result == HitMargin.Perfect)
            {
                Variables.Combo++;
            }
            else
            {
                Variables.Combo = 0;
            }
            switch (GCS.difficulty)
            {
                case Difficulty.Lenient:
                    Overlayer.Patches.GetHitMarginFixer.CalculateScores(__result, Variables.Normal, Variables.Strict, __result);
                    break;
                case Difficulty.Normal:
                    Overlayer.Patches.GetHitMarginFixer.CalculateScores(Variables.Lenient, __result, Variables.Strict, __result);
                    break;
                case Difficulty.Strict:
                    Overlayer.Patches.GetHitMarginFixer.CalculateScores(Variables.Lenient, Variables.Normal, __result, __result);
                    break;
                default:
                    break;
            }
        }

        private static HitMargin GetHitMargin(Difficulty diff, float hitangle, float refangle, bool isCW, float bpmTimesSpeed, float conductorPitch, double marginScale)
        {
            float num = (hitangle - refangle) * (float) (isCW ? 1 : -1);
            HitMargin result = HitMargin.TooEarly;
            float num2 = num;
            num2 = 57.29578f * num2;
            marginScale = scrController.instance.currFloor.marginScale;
            double adjustedAngleBoundaryInDeg = GetAdjustedAngleBoundaryInDeg(diff, HitMarginGeneral.Counted, (double) bpmTimesSpeed, (double) conductorPitch, marginScale);
            double adjustedAngleBoundaryInDeg2 = GetAdjustedAngleBoundaryInDeg(diff, HitMarginGeneral.Perfect, (double) bpmTimesSpeed, (double) conductorPitch, marginScale);
            double adjustedAngleBoundaryInDeg3 = GetAdjustedAngleBoundaryInDeg(diff, HitMarginGeneral.Pure, (double) bpmTimesSpeed, (double) conductorPitch, marginScale);
            if ((double) num2 > -adjustedAngleBoundaryInDeg)
            {
                result = HitMargin.VeryEarly;
            }
            if ((double) num2 > -adjustedAngleBoundaryInDeg2)
            {
                result = HitMargin.EarlyPerfect;
            }
            if ((double) num2 > -adjustedAngleBoundaryInDeg3)
            {
                result = HitMargin.Perfect;
            }
            if ((double) num2 > adjustedAngleBoundaryInDeg3)
            {
                result = HitMargin.LatePerfect;
            }
            if ((double) num2 > adjustedAngleBoundaryInDeg2)
            {
                result = HitMargin.VeryLate;
            }
            if ((double) num2 > adjustedAngleBoundaryInDeg)
            {
                result = HitMargin.TooLate;
            }
            return result;
        }

        private static double GetAdjustedAngleBoundaryInDeg(Difficulty diff, HitMarginGeneral marginType, double bpmTimesSpeed, double conductorPitch, double marginMult = 1.0)
        {
            float num = 0.065f;
            if (diff == Difficulty.Lenient)
            {
                num = 0.091f;
            }
            if (diff == Difficulty.Normal)
            {
                num = 0.065f;
            }
            if (diff == Difficulty.Strict)
            {
                num = 0.04f;
            }
            bool isMobile = ADOBase.isMobile;
            num = (isMobile ? 0.09f : (num / GCS.currentSpeedTrial));
            float num2 = isMobile ? 0.07f : (0.03f / GCS.currentSpeedTrial);
            float a = isMobile ? 0.05f : (0.02f / GCS.currentSpeedTrial);
            num = Mathf.Max(num, 0.025f);
            num2 = Mathf.Max(num2, 0.025f);
            double timeinAbsoluteSpace = (double)Mathf.Max(a, 0.025f);
            double val = scrMisc.TimeToAngleInRad((double)num, bpmTimesSpeed, conductorPitch, false) * 57.295780181884766;
            double val2 = scrMisc.TimeToAngleInRad((double)num2, bpmTimesSpeed, conductorPitch, false) * 57.295780181884766;
            double val3 = scrMisc.TimeToAngleInRad(timeinAbsoluteSpace, bpmTimesSpeed, conductorPitch, false) * 57.295780181884766;
            double result = Math.Max((double)GCS.HITMARGIN_COUNTED * marginMult, val);
            double result2 = Math.Max(45.0 * marginMult, val2);
            double result3 = Math.Max(30.0 * marginMult, val3);
            if (marginType == HitMarginGeneral.Counted)
            {
                return result;
            }
            if (marginType == HitMarginGeneral.Perfect)
            {
                return result2;
            }
            if (marginType == HitMarginGeneral.Pure)
            {
                return result3;
            }
            return result;
        }

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
