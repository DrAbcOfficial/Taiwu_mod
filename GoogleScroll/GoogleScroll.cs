using Harmony12;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;
/**憨批unity引用
CoreModule和IMGUIModule**/

namespace GoogleScroll
{
    public class Settings : UnityModManager.ModSettings
    {
        public int selectedLang = 0;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }
    public static class Main
    {
        static readonly string[] langCode = { "zh-TW", "en", "de", "ja", "ko", "fr" };
        static readonly string[] langName = { "繁體中文", "English", "Deutsch", "日本語", "한국어", "Français" };

        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static JsonDIcList list = new JsonDIcList();

        private static bool HttpDone = false;

        public static bool enabled;
        public static string error = string.Empty;
        private static int num = -1;
        public static bool Load(UnityModManager.ModEntry mod)
        {
            Logger = mod.Logger;

            HarmonyInstance harmony = HarmonyInstance.Create(mod.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = Settings.Load<Settings>(mod);

            mod.OnToggle = OnToggle;
            mod.OnGUI = OnGUI;
            mod.OnSaveGUI = OnSaveGUI;

            ReadList();

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
            GUILayout.Label("翻译至");
            settings.selectedLang = GUILayout.SelectionGrid(0, langName, 5);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (GUILayout.Button("保存数据"))
                SaveList();

            if (HttpDone)
                GUILayout.Label("请求完毕，可以保存！");
            else
                GUILayout.Label("未请求完毕，请等待");
        }
        public static void OnSaveGUI(UnityModManager.ModEntry mod)
        {
            settings.Save(mod);
        }

        private static void ReadList()
        {
            string path = $"{"."}/Data/Lang/{langCode[settings.selectedLang]}.txt";
            if (File.Exists(path))
                using (StreamReader sr = new StreamReader(path))
                {
                    list = (JsonDIcList)JsonConvert.DeserializeObject(sr.ReadToEnd());
                }
            else
                Logger.Log("No existing lang file, passed...");
        }

        private static void SaveList()
        {
            string json = JsonConvert.SerializeObject(list);
            string path = $"{"."}/Data/Lang/{langCode[settings.selectedLang]}.txt";
            if(!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Logger.Log("Directory didn't exist, creating..");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            using (StreamWriter sw = new StreamWriter(path))
            {
                Logger.Log($"Writting Lang file, Total{json.Length}Chars.");
                sw.Write(json);
            }
        }
        public static void Doit()
        {
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
        }
        private static bool IsChina(in string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                Regex rx = new Regex("^[\u4e00-\u9fa5]$");
                if (rx.IsMatch(s[i].ToString()))
                    return true;
            }
            return false;
        }
        private static string JsonRequest(string url)
        {
            string result = string.Empty;
            try
            {

                HttpWebRequest w1 = (HttpWebRequest)WebRequest.Create(url);
                w1.Credentials = CredentialCache.DefaultCredentials;
                w1.Timeout = 600;
                WebResponse w2 = w1.GetResponse();
                //通过WebRequest 对象的GetResponse方法实例化一个WebResponse 对象
                Stream s1 = w2.GetResponseStream();
                //调用WebResponse 对象的GetResponseStream方法返回数据流
                using (StreamReader sr = new StreamReader(s1,Encoding.UTF8))
                    result = sr.ReadToEnd();

            }
            catch (Exception e)
            {
                error = e.Message;
                Logger.Error(error);
            }
            return result;
        }
        private static bool PreProcess(ref Dictionary<int, Dictionary<int, string>> cacheData)
        {
            foreach (Dictionary<int, Dictionary<int, string>> d in list.DicList)
            {
                if (cacheData.Keys.Equals(d.Keys))
                {
                    cacheData = d;
                    return true;
                }
            }
            return false;
        }

        private static void TaskThread(KeyValuePair<int,string> pair, Dictionary<int, string> a)
        {
            string url = "http://translate.google.cn/translate_a/single?client=gtx&dt=t&dj=1&ie=UTF-8&sl=auto&tl={0}&q={1}";
            try
            {
                if (IsChina(pair.Value))
                {
                    string result = JsonRequest(string.Format(url, langCode[settings.selectedLang], pair.Value));
                    if (string.IsNullOrEmpty(result))
                    {
                        a[pair.Key] = pair.Value;
                    }
                    else
                    {
                        Root rt = (Root)JsonConvert.DeserializeObject(result);
                        a[pair.Key] = rt.sentences[0].trans;
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Error(e.Message);
            }
        }
        /// <summary>
        /// 替换部分
        /// </summary>
        /// <param name="cacheData">需要修改的内容</param>
        /// <returns></returns>
        private static void ReplaceIt(ref Dictionary<int, Dictionary<int, string>> cacheData)
        {
            Task task = null;
            num++;
            //已经翻译过一遍了，不用再翻译了
            if (PreProcess(ref cacheData))
                return;

            Dictionary<int, Dictionary<int, string>> b = new Dictionary<int, Dictionary<int, string>>();
            foreach (KeyValuePair<int, Dictionary<int, string>> value in cacheData)
            {
                Dictionary<int, string> a = new Dictionary<int, string>();
                foreach (KeyValuePair<int, string> pair in value.Value)
                {
                    task = Task.Factory.StartNew(() => TaskThread(pair, a));
                }
                b.Add(value.Key, a);
            }
            Task.WaitAll(task);
            list.DicList[num] = b;
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


        public class JsonDIcList
        {
            public List<Dictionary<int, Dictionary<int, string>>> DicList = new List<Dictionary<int, Dictionary<int, string>>>();
        }

        public class SentencesItem
        {
            /// <summary>
            /// 太吾繪卷
            /// </summary>
            public string trans { get; set; }
            /// <summary>
            /// 太吾绘卷
            /// </summary>
            public string orig { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int backend { get; set; }
        }

        public class Spell
        {
        }

        public class Ld_result
        {
            /// <summary>
            /// 
            /// </summary>
            public List<string> srclangs { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<double> srclangs_confidences { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<string> extended_srclangs { get; set; }
        }
        /// <summary>
        /// Json实体类
        /// </summary>
        public class Root
        {
            /// <summary>
            /// 
            /// </summary>
            public List<SentencesItem> sentences { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string src { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public double confidence { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Spell spell { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Ld_result ld_result { get; set; }
        }
    }
}
