using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;
using UnityEngine;
using System.IO;
using System.Text;
using System.Net;
using System.Threading;
using GDMiniJSON;
using System.Linq;

namespace ShowTimingWindowScale
{
    public static class Main
    {
        public static bool IsEnabled { get; private set; }

        public static TextBehaviour gui;
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }

        public static Harmony harmony;
        public static Setting setting;
        public static bool overlayer = false;

        private static readonly int ReleaseNumber = (int) AccessTools.Field(typeof(GCNS), "releaseNumber").GetValue(null);
        private static readonly string version = "0.0.1";

        private static string modID;
        private static bool stopped = false;

        public static void Setup(UnityModManager.ModEntry modEntry)
        {
            modID = modEntry.Info.Id;
            modEntry.Info.Version = version;
            Logger = modEntry.Logger;
            startUp(modEntry);
        }

        private static void startUp(UnityModManager.ModEntry modEntry)
        {
            setting = new Setting();
            setting = UnityModManager.ModSettings.Load<Setting>(modEntry);
            modEntry.OnToggle = OnToggle;
        }

        private static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch
            {
                return "null";
            }
        }

        private static void disable()
        {
            gui.TextObject.SetActive(false);
            UnityEngine.Object.DestroyImmediate(gui);
            gui = null;
            Stop(UnityModManager.FindMod(modID));
            stopped = true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            IsEnabled = value;
            if(value && !stopped)
            {
                Start(modEntry);
                gui = new GameObject().AddComponent<TextBehaviour>();
                UnityEngine.Object.DontDestroyOnLoad(gui);
                modEntry.OnGUI = OnGUI;
                modEntry.OnSaveGUI = OnSaveGUI;
                gui.TextObject.SetActive(false);
            } 
            else
            {
                modEntry.Info.DisplayName = "ShowTimingWindowScale";
                gui.TextObject.SetActive(false);
                UnityEngine.Object.DestroyImmediate(gui);
                gui = null;
                Stop(modEntry);
            }
            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (stopped)
            {
                return;
            }

            setting.onShowHitMargin = GUILayout.Toggle(setting.onShowHitMargin, "Enable ShowTimingWindowScale");
            if (setting.onShowHitMargin)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                setting.text4 = MoreGUILayout.NamedTextField("Text", setting.text4, 300f);
                GUILayout.EndHorizontal();
            }

            if (setting.onShowHitMargin)
            {
                GUILayout.Label("   ");
                setting.useShadow = GUILayout.Toggle(setting.useShadow, "Shadow");
                gui.shadowText.enabled = setting.useShadow;
            
                setting.useBold = GUILayout.Toggle(setting.useBold, "Bold");
                gui.text.fontStyle = setting.useBold ? FontStyle.Bold : FontStyle.Normal;
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                float newX =
                    MoreGUILayout.NamedSlider(
                        "X",
                        setting.x,
                        -0.1f,
                        1.1f,
                        300f,
                        roundNearest: 0.001f,
                        valueFormat: "{0:0.###}");
                if (newX != setting.x)
                {
                    setting.x = newX;
                    gui.setPosition(setting.x,setting.y);
                }
                GUILayout.EndHorizontal();        
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                float newY =
                    MoreGUILayout.NamedSlider(
                        "Y",
                        setting.y,
                        -0.1f,
                        1.1f,
                        300f,
                        roundNearest: 0.001f,
                        valueFormat: "{0:0.###}");

                if (newY != setting.y)
                {
                    setting.y = newY;
                    gui.setPosition(setting.x, setting.y);
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                float newSize =
                    MoreGUILayout.NamedSlider(
                        "Size",
                        setting.size,
                        1f,
                        100f,
                        300f,
                        roundNearest: 1f,
                        valueFormat: "{0:0.#}");


                if ((int) newSize != setting.size)
                {
                    setting.size = (int) newSize;
                    gui.setSize(setting.size);
                }

                GUILayout.EndHorizontal();
                
                string[] aligns = new string[] {"Left", "Center", "Right" };
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Set Align Mode");

                GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
                
                foreach (string text in aligns)
                {
                    if (setting.align == Array.IndexOf(aligns, text)) guiStyle.fontStyle = FontStyle.Bold;
                    if(GUILayout.Button(text, guiStyle)) setting.align = Array.IndexOf(aligns, text);
                    guiStyle.fontStyle = FontStyle.Normal;
                }
                GUILayout.EndHorizontal();

                string md51, md52;
                try
                {
                    string path = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Replace(@"\A Dance of Fire and Ice.exe", "");
                    md51 = GetMD5HashFromFile(path + "\\Mods\\ShowTimingWindowScale\\ShowTimingWindowScale.dll");
                    md52 = GetMD5HashFromFile(path + "\\A Dance of Fire and Ice_Data\\Managed\\Assembly-CSharp.dll");
                }
                catch
                {
                    md51 = GetMD5HashFromFile(System.Windows.Forms.Application.StartupPath + "\\Mods\\ShowTimingWindowScale\\ShowTimingWindowScale.dll");
                    md52 = GetMD5HashFromFile(System.Windows.Forms.Application.StartupPath + "\\A Dance of Fire and Ice_Data\\Managed\\Assembly-CSharp.dll");
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Game ID: " + md51);
                GUILayout.Label("Mod ID: " + md52);
                GUILayout.Space(300);
                GUILayout.EndHorizontal();

                gui.text.alignment = gui.toAlign(setting.align);
            }

        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            setting.Save(modEntry);
        }

        private static void Start(UnityModManager.ModEntry modEntry)
        {
            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            stopped = false;
        }

        private static void Stop(UnityModManager.ModEntry modEntry)
        {
            harmony.UnpatchAll(modEntry.Info.Id);
        }
    }
}
