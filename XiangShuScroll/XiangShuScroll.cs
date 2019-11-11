using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
/**憨批unity引用
CoreModule和IMGUIModule**/

namespace XiangShuScroll
{
    public class Settings : UnityModManager.ModSettings
    {
        public string[] name = { "相枢", "太吾", "EX咖喱棒" };
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }

    public class Main
    {
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static int num = 0;
        public static bool enabled, update = false;
        public static List<int> nullObj = new List<int>();

        public static bool Load(UnityModManager.ModEntry mod)
        {
            Logger = mod.Logger;

            var harmony = HarmonyInstance.Create(mod.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = Settings.Load<Settings>(mod);

            mod.OnToggle = OnToggle;
            mod.OnGUI = OnGUI;
            mod.OnSaveGUI = OnSaveGUI;
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry mod, bool val)
        {
            if (!val)
                return false;

            Main.enabled = val;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry mody)
        {
            GUILayout.Label("");
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("修改太吾名称:"); GUILayout.Space(16);
            settings.name[0] = GUILayout.TextField(settings.name[0], "宁叫啥勒？"); GUILayout.Space(16);
            GUILayout.Label("修改相枢名称:"); GUILayout.Space(16);
            settings.name[1] = GUILayout.TextField(settings.name[1], "他叫啥勒？"); GUILayout.Space(16);
            GUILayout.Label("修改剑柄名称:"); GUILayout.Space(16);
            settings.name[2] = GUILayout.TextField(settings.name[2], "剑叫啥勒？"); GUILayout.Space(16);

            if (GUILayout.Button("更新名称"))
                Main.update = true;
            GUILayout.Space(16);
            if (Main.update)
                GUILayout.Label("<color=#E4504DFF>修改将在重启游戏后生效！</color>");
            else
                GUILayout.Label("修改将在重启游戏后生效");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public static void Doit()
        {
            for (int i = 0; i < settings.name.Length; i++)
            {
                if (string.IsNullOrEmpty(settings.name[i]))
                    nullObj.Add(i);
            }
            if (nullObj.Count != 0)
            {
                foreach (int i in nullObj)
                {
                    switch (i)
                    {
                        case 0: settings.name[0] = "太吾"; break;
                        case 1: settings.name[1] = "相枢"; break;
                        case 2: settings.name[2] = "伏虞"; break;
                    }
                }
            }

            num = 0;

            DateFile.instance.massageDate = ReplaceIt(DateFile.instance.massageDate);
            DateFile.instance.trunEventDate = ReplaceIt(DateFile.instance.trunEventDate);
            DateFile.instance.actorMassageDate = ReplaceIt(DateFile.instance.actorMassageDate);
            DateFile.instance.placeWorldDate = ReplaceIt(DateFile.instance.placeWorldDate);
            DateFile.instance.resourceDate = ReplaceIt(DateFile.instance.resourceDate);
            DateFile.instance.presetGangDate = ReplaceIt(DateFile.instance.presetGangDate);
            DateFile.instance.presetGangGroupDateValue = ReplaceIt(DateFile.instance.presetGangGroupDateValue);
            DateFile.instance.actorFeaturesDate = ReplaceIt(DateFile.instance.actorFeaturesDate);
            DateFile.instance.baseStoryDate = ReplaceIt(DateFile.instance.baseStoryDate);
            DateFile.instance.eventDate = ReplaceIt(DateFile.instance.eventDate);
            DateFile.instance.identityDate = ReplaceIt(DateFile.instance.identityDate);
            DateFile.instance.tipsMassageDate = ReplaceIt(DateFile.instance.tipsMassageDate);
            DateFile.instance.baseMissionDate = ReplaceIt(DateFile.instance.baseMissionDate);
            DateFile.instance.scoreBootyDate = ReplaceIt(DateFile.instance.scoreBootyDate);
            DateFile.instance.legendBookEffectData = ReplaceIt(DateFile.instance.legendBookEffectData);
            DateFile.instance.basehomePlaceDate = ReplaceIt(DateFile.instance.basehomePlaceDate);
            DateFile.instance.baseTipsDate = ReplaceIt(DateFile.instance.baseTipsDate);
            DateFile.instance.presetitemDate = ReplaceIt(DateFile.instance.presetitemDate);

            Logger.Log(string.Format("共替换了{0}个句子，游戏愉快！{1}!", num, settings.name[0]));
#if array
            num = 0;
            for (int i = 0; i < dictionaries.Length; i++)
            {
                if (dictionaries[i] is null)
                    Logger.Log("数据编号:" + i + "为空!已跳过....");
                else
                    ReplaceIt(dictionaries[i]);
            }
#endif
        }
        private static readonly string[] cnNumberString = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
        private static readonly string[] cnMaxUnit = { "", "万", "亿", "万亿" };
        private static readonly string[] cnMinUnit = { "", "十", "百", "千" };

        private static void SectionToChines(int section, ref string cnString)
        {
            string strIns = string.Empty;
            int unitPos = 0;
            bool zero = true;
            while (section > 0)
            {
                int v = section % 10;
                if (v == 0)
                {
                    if (!zero)
                    {
                        zero = true;
                        cnString = cnString.Insert(0, cnNumberString[v]);
                    }
                }
                else
                {
                    zero = false;
                    strIns = $"{cnNumberString[v]}{cnMinUnit[unitPos]}";
                    cnString = cnString.Insert(0, strIns);
                }
                unitPos++;
                section /= 10;
            }
        }

        public static string GetNumber(int number)
        {
            int tempNumber = Math.Abs(number);
            if (tempNumber == 0)
            {
                return "零";
            }

            string result = string.Empty;
            int unitPos = 0;
            bool needZero = false;
            while (tempNumber > 0)
            {
                string strIns = string.Empty;
                int section = tempNumber % 10000;
                if (needZero)
                {
                    result = result.Insert(0, cnNumberString[0]);
                }
                SectionToChines(section, ref strIns);
                strIns += (section != 0) ? cnMaxUnit[unitPos] : cnMaxUnit[0];
                result = result.Insert(0, strIns);
                needZero = (section < 1000) && (section > 0);
                tempNumber /= 10000;
                unitPos++;
            }
            if (number < 0)
            {
                result = $"负{result}";
            }
            return result;
        }

        public static string SwordDetailCombiner(in string cache)
        {
            string res = string.Empty;
            foreach (char c in cache)
            {
                res += "“" + c + "”";
            }
            res += GetNumber(cache.Length);
            return res;
        }

        private static Dictionary<int, Dictionary<int, string>> ReplaceIt(Dictionary<int, Dictionary<int, string>> cacheData)
        {
            int i = 0;
            Dictionary<int, Dictionary<int, string>> b = new Dictionary<int, Dictionary<int, string>>();
            foreach (KeyValuePair<int, Dictionary<int, string>> value in cacheData)
            {
                Dictionary<int, string> a = new Dictionary<int, string>();
                foreach (KeyValuePair<int, string> pair in value.Value)
                {
                    if (pair.Value.Contains("太吾"))
                        num++;
                    try
                    {
                        a.Add(pair.Key, string.Format(pair.Value.Replace("太吾", "{0}").Replace("相枢", "{1}").Replace("伏虞", "{2}").Replace("“伏”“虞”二", "{3}"), settings.name[0], settings.name[1], settings.name[2], SwordDetailCombiner(settings.name[2])));
                    }
                    catch (Exception e)
                    {
                        i++;
                    }
                }
                b.Add(value.Key, a);
            }
            if (i != 0)
                Logger.Log("未替换" + i + "项");
            return b;
        }
        public static void OnSaveGUI(UnityModManager.ModEntry mod)
        {
            settings.Save(mod);
        }
        [HarmonyPatch(typeof(ArchiveSystem.LoadGame), "LoadReadonlyData")]
        private static class ArchiveSystem_LoadGame_LoadReadonlyData_Patch
        {
            public static void Postfix()
            {
                if (!Main.enabled)
                    return;
                Doit();
            }
        }
    }
}
