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
        public string name = "相枢";
        public string swordname = "EX咖喱棒";
        public string badname = "太吾";
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }

    public class Main
    {

        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static int num = 0;

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

            enabled = val;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry mody)
        {
            GUILayout.Label("");
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("修改太吾名称:"); GUILayout.Space(16);
            settings.name = GUILayout.TextField(settings.name, "宁叫啥勒？"); GUILayout.Space(16);
            GUILayout.Label("修改剑柄名称:"); GUILayout.Space(16);
            settings.swordname = GUILayout.TextField(settings.swordname, "剑叫啥勒？"); GUILayout.Space(16);
            GUILayout.Label("修改相枢名称:"); GUILayout.Space(16);
            settings.badname = GUILayout.TextField(settings.badname, "剑叫啥勒？"); GUILayout.Space(16);
            if (GUILayout.Button("更新名称"))
                Doit();
            GUILayout.Space(16);
            GUILayout.Label("修改将在重启游戏后生效！");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public static void Doit()
        {
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

            Logger.Log(string.Format("共替换了{0}个句子，游戏愉快！{1}!", num, settings.name));
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

        private static string SwordDetailCombiner(in string replacer)
        {
            string cache = "";
            for (int i = 0; i < replacer.Length; i++)
            {
                cache += "“" + replacer[i] + "”";
            }
            string len = replacer.Length.ToString();
            for (int j = 0; j < len.Length; j++)
            {
                switch (len[j])
                {
                    default: continue;
                    case '1': cache += "一"; break;
                    case '2': cache += "二"; break;
                    case '3': cache += "三"; break;
                    case '4': cache += "四"; break;
                    case '5': cache += "五"; break;
                    case '6': cache += "六"; break;
                    case '7': cache += "七"; break;
                    case '8': cache += "八"; break;
                    case '9': cache += "九"; break;
                    case '0': cache += "十"; break;
                }
            }
            return cache;
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
                        a.Add(pair.Key, string.Format(pair.Value.Replace("太吾", "{0}").Replace("相枢", "{1}").Replace("伏虞", "{2}").Replace("“伏”“虞”二", "{3}"), settings.name, settings.badname, settings.swordname, SwordDetailCombiner(settings.swordname)));
                    }
                    catch (Exception e)
                    {
                        i++;
                    }
                }
                b.Add(value.Key, a);
            }
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
