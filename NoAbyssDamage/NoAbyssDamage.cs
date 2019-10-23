using Harmony12;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
/**憨批unity引用
CoreModule和IMGUIModule**/

namespace NoAbyssDamage
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool isAll = false;
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

        static void OnGUI(UnityModManager.ModEntry mody)
        {
            settings.isAll = GUILayout.Toggle(settings.isAll, "启用所有人均无伤害");
        }

        static void OnSaveGUI(UnityModManager.ModEntry mod)
        {
            settings.Save(mod);
        }

        /// <summary>
        ///  重新把移动代码复制一遍 获取数据接口拦截，提前修改伤害
        /// </summary>
        [HarmonyPatch(typeof(WorldMapSystem), "PlayerMoveDone")]
        public static class WorldMapSystem_PlayerMoveDone_Patch
        {
            private static bool DoDamage()
            {

                List<int> list = new List<int>(DateFile.instance.GetFamily(true, false));
                for (int i = 0; i < list.Count; i++)
                {

                    DateFile.instance.MakeRandInjury(
                        list[i],
                        (UnityEngine.Random.Range(0, 100) < 75) ? 0 : 10,
                        200);
                }

                //发送提示你受伤了笨逼
                TipsWindow.instance.SetTips(20, new string[]
                {
                    ""
                }, 300, 225f, 153f, 450, 100);

                return true;
            }

            private static bool Prefix(bool showLoading, bool noNeed, int[] newMoveNeed, int worldId, int partId, int placeId, bool fastMove)
            {
                bool flag = !showLoading;
                if (flag)
                {
                    WorldMapSystem.instance.ChangeMoveNeed(noNeed, newMoveNeed, true);
                }
                WorldMapSystem.instance.UpdateMovePath(WorldMapSystem.instance.targetPlaceId);
                int num = int.Parse(DateFile.instance.GetNewMapDate(partId, placeId, 999));
                int num2 = num;
                if (num2 == 20002)
                {
                    bool flag2 = !fastMove;
                    if (flag2 && !settings.isAll)
                        DoDamage();
                }

                bool flag3 = placeId == DateFile.instance.baseWorldDate[worldId][partId][1] && DateFile.instance.baseWorldDate[worldId][partId][0] != 1;
                if (flag3)
                {
                    DateFile.instance.baseWorldDate[worldId][partId][0] = 1;
                    TipsWindow.instance.SetTips(
                        19, new string[]
                                {
                                    DateFile.instance.GetNewMapDate(partId, placeId, 0)
                                },
                        300, 225f, 153f, 450, 100);
                    DateFile.instance.AddActorScore(601, 100);
                }
                int num3 = int.Parse(DateFile.instance.GetNewMapDate(partId, placeId, 94));
                bool flag4 = num3 > 0 && int.Parse(DateFile.instance.GetGangDate(num3, 2)) == 1 && int.Parse(DateFile.instance.GetGangDate(num3, 10)) == 0;
                if (flag4)
                {
                    DateFile.instance.SetEvent(
                        new int[]
                        {
                            0,
                            -1,
                            1282 + num3,
                            num3
                        },
                        true, true);
                }

                bool flag5 = DateFile.instance.teachingOpening > 0;
                if (flag5)
                {
                    bool flag6 = placeId == 11 && DateFile.instance.teachingOpening == 200;
                    if (flag6)
                    {
                        DateFile.instance.teachingOpening = 201;
                        Teaching.instance.RemoveTeachingWindow(0);
                        Teaching.instance.SetTeachingWindow(1);
                    }
                    bool flag7 = placeId == 12 && DateFile.instance.teachingOpening == 202;
                    if (flag7)
                    {
                        Teaching.instance.RemoveTeachingWindow(4);
                    }
                }
                AudioManager.instance.UpdatePlaceBGM(partId, placeId);
                DateFile.instance.playerMoveing = false;
                //阻断call原版的
                return false;
            }
        }
    }
}
