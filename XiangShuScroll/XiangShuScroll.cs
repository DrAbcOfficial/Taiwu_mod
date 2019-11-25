#define DEBUG

using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
/**憨批unity引用
CoreModule和IMGUIModule**/

namespace XiangShuScroll
{
    public class Settings : UnityModManager.ModSettings
    {
        /// <summary>
        /// 替换内容 太吾名称 相枢名称 神剑名称
        /// </summary>
        public string[] name = { "相枢", "太吾", "EX咖喱棒" };
        /// <summary>
        /// 替换任何你想替换的东西
        /// </summary>
        public string[][] other = { new string[] { "狮相", "章鱼" } , new string[] { "青琅主", "青琅猪" } };
        /// <summary>
        /// 保存按钮
        /// </summary>
        /// <param name="modEntry"></param>
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }

    public static class Main
    {
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        /// <summary>
        /// 替换条目统计
        /// </summary>
        private static int num = 0;
        /// <summary>
        /// 是否启动，是否更新
        /// </summary>
        public static bool enabled, update = false;
        /// <summary>
        /// 空白项
        /// </summary>
        private static readonly string[] cnNumberString = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
        /// <summary>
        /// 大单位替换中文字符
        /// </summary>
        private static readonly string[] cnMinUnit = { "", "十", "百", "千" };
        /// <summary>
        /// 特大单位替换中文字符，你写这么长的剑柄名是干什么
        /// </summary>
        private static readonly string[] cnMaxUnit = { "", "万", "亿", "万亿" };
        /// <summary>
        /// 被替换字段初始化
        /// </summary>
        private static string cacheReplacer = string.Empty; 
        /// <summary>
        /// 替换字段
        /// </summary>
        private static string cacheReplacee = string.Empty;
        /// <summary>
        /// 是否点下了保存按钮
        /// </summary>
        private static bool saveflag = false;
        /// <summary>
        /// 从数组读取字符串
        /// </summary>
        /// <param name="instr">输入数组</param>
        /// <param name="index">读取索引</param>
        /// <returns></returns>
        private static string ReadStrFromSave(in string[][] instr, in int index = 0)
        {
            string cache = string.Empty;
            foreach (string[] a in instr)
            {
                cache += a[index] + "\n";
            }
            return cache;
        }
        /// <summary>
        /// 从字符串读取数组
        /// </summary>
        /// <param name="str1">被替换字符串</param>
        /// <param name="str2">替换字符串</param>
        /// <returns></returns>
        private static bool ReadListFromStr(in string str1, in string str2)
        {
            string[] parser1 = str1.Split('\n').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            string[] parser2 = str2.Split('\n').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            List<string[]> list = new List<string[]>();
            if (parser1.Length != parser2.Length)
                return false;

            for (int i = 0; i < parser1.Length; i++)
            {
                list.Add(new string[] { parser1[i], parser2[i] });
            }
            settings.other = list.ToArray();

            #region  DEBUG
            int numm = 0;
            foreach (string[] b in list)
            {
                Logger.Log(numm + " | " + b[0] + b[1]);
            }
            foreach (string[] a in settings.other)
            {
                Logger.Log(numm + " | " + a[0] + a[1]);
                numm++;
            }
            #endregion

            return true;
        }
        /// <summary>
        /// 入口点
        /// </summary>
        /// <param name="mod">mod</param>
        /// <returns></returns>
        /// 
        public static bool Load(UnityModManager.ModEntry mod)
        {
            Logger = mod.Logger;

            var harmony = HarmonyInstance.Create(mod.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = Settings.Load<Settings>(mod);

            mod.OnToggle = OnToggle;
            mod.OnGUI = OnGUI;
            mod.OnSaveGUI = OnSaveGUI;

            cacheReplacer = ReadStrFromSave(settings.other, 0);
            cacheReplacee = ReadStrFromSave(settings.other, 1);

            return true;
        }
        /// <summary>
        /// 切换
        /// </summary>
        /// <param name="mod">控制的mod</param>
        /// <param name="val">是否启用</param>
        /// <returns></returns>
        public static bool OnToggle(UnityModManager.ModEntry mod, bool val)
        {
            if (!val)
                return false;

            Main.enabled = val;
            return true;
        }
        /// <summary>
        /// GUI界面
        /// </summary>
        /// <param name="mod">控制的mod</param>
        public static void OnGUI(UnityModManager.ModEntry mod)
        {
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("修改太吾名称:"); GUILayout.Space(16);
            settings.name[0] = GUILayout.TextField(settings.name[0]); GUILayout.Space(16);
            GUILayout.Label("修改相枢名称:"); GUILayout.Space(16);
            settings.name[1] = GUILayout.TextField(settings.name[1]); GUILayout.Space(16);
            GUILayout.Label("修改剑柄名称:"); GUILayout.Space(16);
            settings.name[2] = GUILayout.TextField(settings.name[2]); GUILayout.Space(16);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("修改名称"); GUILayout.Label("被修改名称");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.MinWidth(32);
            GUILayout.MaxWidth(96);
            cacheReplacer = GUILayout.TextArea(cacheReplacer);
            GUILayout.Space(16);
            GUILayout.MinWidth(32);
            GUILayout.MaxWidth(96);
            cacheReplacee = GUILayout.TextArea(cacheReplacee);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (GUILayout.Button("保存数据"))
            {
                update = true;
                saveflag = ReadListFromStr(cacheReplacer, cacheReplacee);
            }
            if (!saveflag && update)
                GUILayout.Label("<color=#E4504DFF>两边文本框长度不一致！保存失败！</color>");

            GUILayout.BeginHorizontal("Box");
            if (update)
                GUILayout.Label("<color=#24504DFF>储存完毕！</color>");
            else
                GUILayout.Label("<color=#E4504DFF>修改将在重启游戏后生效</color>");
            GUILayout.EndHorizontal();

        }
        public static void OnSaveGUI(UnityModManager.ModEntry mod)
        {
            settings.Save(mod);
        }
        /// <summary>
        /// 开始了
        /// </summary>
        public static void Doit()
        {
            //归零计数
            num = 0;

            //怎么保存地址到一个数组里呢.......
            ReplaceIt(ref DateFile.instance.massageDate);
            ReplaceIt(ref DateFile.instance.trunEventDate);
            ReplaceIt(ref DateFile.instance.actorMassageDate);
            ReplaceIt(ref DateFile.instance.placeWorldDate);
            ReplaceIt(ref DateFile.instance.resourceDate);
            ReplaceIt(ref DateFile.instance.presetGangDate);
            ReplaceIt(ref DateFile.instance.presetGangGroupDateValue);
            ReplaceIt(ref DateFile.instance.actorFeaturesDate);
            ReplaceIt(ref DateFile.instance.baseStoryDate);
            ReplaceIt(ref DateFile.instance.eventDate);
            ReplaceIt(ref DateFile.instance.identityDate);
            ReplaceIt(ref DateFile.instance.tipsMassageDate);
            ReplaceIt(ref DateFile.instance.baseMissionDate);
            ReplaceIt(ref DateFile.instance.scoreBootyDate);
            ReplaceIt(ref DateFile.instance.legendBookEffectData);
            ReplaceIt(ref DateFile.instance.basehomePlaceDate);
            ReplaceIt(ref DateFile.instance.baseTipsDate);
            ReplaceIt(ref DateFile.instance.presetitemDate);

            Logger.Log(string.Format("共替换了{0}个句子，游戏愉快！{1}!", num, settings.name[0]));
        }

        /// <summary>
        /// 小数目替换
        /// </summary>
        /// <param name="section">片段</param>
        /// <param name="cnString">中文字符</param>
        private static void SectionToChines(int section, ref string cnString)
        {
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
                    string strIns = $"{cnNumberString[v]}{cnMinUnit[unitPos]}";
                    cnString = cnString.Insert(0, strIns);
                }
                unitPos++;
                section /= 10;
            }
        }
        /// <summary>
        /// 替换为中文数字
        /// </summary>
        /// <param name="number">数字</param>
        /// <returns></returns>
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
        /// <summary>
        /// 剑柄描述生成
        /// </summary>
        /// <param name="cache">剑柄名</param>
        /// <returns></returns>
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
        /// <summary>
        /// 替换部分
        /// </summary>
        /// <param name="cacheData">需要修改的内容</param>
        /// <returns></returns>
        private static void ReplaceIt(ref Dictionary<int, Dictionary<int, string>> cacheData)
        {
            int i = 0;
            Dictionary<int, Dictionary<int, string>> b = new Dictionary<int, Dictionary<int, string>>();
            foreach (KeyValuePair<int, Dictionary<int, string>> value in cacheData)
            {
                Dictionary<int, string> a = new Dictionary<int, string>();
                foreach (KeyValuePair<int, string> pair in value.Value)
                {
                    //计数加一√
                    if (pair.Value.Contains("太吾") || pair.Value.Contains("相枢") || pair.Value.Contains("伏虞"))
                        num++;

                    //替换内容为C#占位符，如果为空则不替换
                    string cache = pair.Value;
                    //先替换其他内容
                    foreach (string[] parse in settings.other)
                    {
                        if (cache.Contains(parse[0]))
                        {
                            cache.Replace(parse[0], parse[1]);
                            num++;
                        }
                    }

                    if (!string.IsNullOrEmpty(settings.name[0]))
                        cache = cache.Replace("太吾", "{0}");
                    if (!string.IsNullOrEmpty(settings.name[1]))
                        cache = cache.Replace("相枢", "{1}");
                    if (!string.IsNullOrEmpty(settings.name[2]))
                    {
                        cache = cache.Replace("伏虞", "{2}");
                        cache = cache.Replace("“伏”“虞”二", "{3}");
                    }

                    try
                    {
                        //把占位符换为输入内容
                        cache = string.Format(cache, settings.name[0], settings.name[1], settings.name[2], SwordDetailCombiner(settings.name[2]));

                        a.Add(pair.Key, cache);
                    }
                    catch (Exception)
                    {
                        //出错未替换项目就无视了，然后+1
                        i++;
                    }
                }
                b.Add(value.Key, a);
            }
            if (i != 0)
                Logger.Log("未替换" + i + "项");
            cacheData = b;
        }
        /// <summary>
        /// 读取完默认资源后开始替换
        /// </summary>
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
