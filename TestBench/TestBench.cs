using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Core;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Epic.OnlineServices;
using JetBrains.Annotations;
using static UnityEngine.EventSystems.EventTrigger;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityTime;
/******************************
* Some Note Here
* ****************************
* 
*/

namespace TestBench
{
    [BepInPlugin("com.nekoice.plugin.testbench", "TestBench", "2.0.0")]
    public class TestBench : BaseUnityPlugin
    {
        Rect mainWindowRect = new Rect(new Vector2(500, 300), MainGUISize.WindowSize);
        Rect potionSelectWindowRect  = new Rect(0, 0, 500, 600);
        Rect magicSwordSelectWindowRect = new Rect(0, 0, 500, 500);
        Rect magicSworwGenerateWindowRect = new Rect(0, 0, 200, 320);
        Rect magicSworwEntrySelectWindowRect = new Rect(0, 0, 500, 500);
        Rect buffWindowRect  = new Rect(0, 0, 400, 400);
        GUIStyle LabelStyle { get; } = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Normal,
            font = null,
            margin = new RectOffset(10, 10, 10, 10),
            fontSize = 16,
            normal = new GUIStyleState
            {
                textColor = Color.white,
            },
        };
        GUIStyle BuffLabelStyle { get; } = new GUIStyle()
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Normal,
            font = null,
            margin = new RectOffset(10, 10, 10, 10),
            fontSize = 16,
            normal = new GUIStyleState
            {
                textColor = Color.white,
            },
        };
        private int PotionID { get; set; } = 1;
        private string CurrentPotionName
        {
            get
            {
                if (PotionID == 0)
                {
                    return "随机";
                }
                return Localization.Instance.GetLocalizedText($"PN_NAME_{PotionID}");
            }
        }

        private int MagicSwordEntryID { get; set; } = 0;
        private int PotionLevel { get; set; } = 2;
        private string CurrentMagicSwordName
        {
            get
            {
                if (UserCustomMagicSwordControl.Instance.MagicSwordID == 0)
                {
                    return "随机";
                }
                MagicSword ms = new MagicSword
                {
                    magicSwordName = ((MagicSwordName)UserCustomMagicSwordControl.Instance.MagicSwordID)
                };
                return TextControl.instance.MagicSwordInfo(ms)[0];
            }
        }
        private string StringPlayerSoulsCount
        {
            get
            {
                if (PlayerAnimControl.instance is null) return "0";
                return $"{PlayerAnimControl.instance.Souls:0}";
            }
            set
            {
                if (int.TryParse(value, out int inputNum))
                {
                    PlayerAnimControl.instance.Souls = inputNum;
                }
                else
                {
                    PlayerAnimControl.instance.Souls = 0;
                }
            }
        }
        float GameSpeedSet = 1;
        List<float> GameSpeedPreset = new List<float> { 0.1f, 0.25f, 0.5f, 1, 1.5f, 2 };
        float GameSpeedBeforePause = 1;
        bool HasPotionSelectUIOn { get; set; } = false;
        bool HasMagicSwordGenerateUIOn { get; set; } = false;
        bool HasMagicSwordSeletctUIOn { get; set; } = false;
        bool HasMagicSwordEntrySeletctUIOn { get; set; } = false;
        bool UI_Fold { get; set; } = true;
        public static int PotionsNum => Enum.GetNames(typeof(PN)).Length;
        public static int WeaponsNum => Enum.GetNames(typeof(MagicSwordName)).Length;
        public static int MSEntriesNum => Enum.GetNames(typeof(MagicSwordEntryName)).Length;
        GameObject DummyObject { get; set; }
        bool HasDummy => DummyObject != null;
        void Start()
        {
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            Logger.LogInfo(UserCustomMagicSwordControl.Instance.ToString());
            UserCustomMagicSwordControl.Instance.LogInfo += MSControl_Loginfo;
        }

        private void MSControl_Loginfo(string info)
        {
            Logger.LogInfo(info);
        }

        void Update()
        {
            if (PlayerAnimControl.instance != null && MenuSkillLearn.instance != null)
            {
                if (HasDummy && UI_Fold == false)
                {
                    MenuSkillLearn.instance.hasSkillRandom = false;
                    if (MenuSkillLearn.instance.isOn)
                    {
                        MenuSkillLearn.instance.refineButton.alpha = 1f;
                        MenuSkillLearn.instance.refineButton.blocksRaycasts = true;
                    }
                }
            }

            showtransport = Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.LeftAlt);
            if (GameSpeedSet != 1)
            {
                Time.timeScale = GameSpeedSet;
            }
        }
        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            GameObject Dummy = GameObject.Find("Dummy");
            if (Dummy != null)
            {
                DummyObject = Dummy;
            }
        }
        void OnGUI()
        {
            mainWindowRect = GUI.Window("测试台".GetHashCode(), mainWindowRect, TestBenchWindow, "测试台");
            if (HasPotionSelectUIOn)
            {
                potionSelectWindowRect = GUI.Window("圣物".GetHashCode(), potionSelectWindowRect, PotionSelectWindow, "圣物");
            }
            if (HasMagicSwordGenerateUIOn)
            {
                magicSworwGenerateWindowRect = GUI.Window("武器".GetHashCode(), magicSworwGenerateWindowRect, MagicSwordGenerateWindow, "武器");
            }
            if (HasDummy && !UI_Fold)
            {
                if (mainWindowRect.x > Screen.width / 2)
                {
                    buffWindowRect.x = mainWindowRect.x - buffWindowRect.width;
                }
                else
                {
                    buffWindowRect.x = mainWindowRect.xMax;
                }
                buffWindowRect.y = mainWindowRect.y;
                buffWindowRect = GUI.Window("木桩".GetHashCode(), buffWindowRect, BuffListWindow, "木桩");
            }
            if (HasMagicSwordSeletctUIOn)
            {
                magicSwordSelectWindowRect = GUI.Window("武器选择".GetHashCode(), magicSwordSelectWindowRect, MagicSwordSelectWindow, "武器选择");
            }
            if (HasMagicSwordEntrySeletctUIOn)
            {
                magicSworwEntrySelectWindowRect = GUI.Window("特效选择".GetHashCode(), magicSworwEntrySelectWindowRect, MagicSwordEntrySelectWindow, "特效选择");
            }

            if (showtransport)
            {
                GUI.Window("123".GetHashCode(), new Rect(0, 0, 300, 600), TransportSelectionWindow, "传送");
            }
        }
        
        void BuffListWindow(int id)
        {
            if (HasDummy && !UI_Fold)
            {
                EnemyControl bec = DummyObject.GetComponent<EnemyControl>();
                foreach (BuffData buff in bec.buffAction.buffs)
                {
                    GUILayout.Label($"{new ModBuffData(buff)}", LabelStyle);
                }
            }
            GUI.DragWindow();
        }

        void TestBenchWindow(int id)
        {
            if (UI_Fold || PlayerAnimControl.instance is null)
            {
                if (PlayerAnimControl.instance is null)
                {
                    GUI.Button(new Rect(5, 20, 190, 30), "玩家未加载");
                }
                else
                {
                    if (GUI.Button(new Rect(5, 20, 190, 30), "展开")) UI_Fold = false;
                }
                mainWindowRect.height = 60;
            }
            else
            {
                mainWindowRect.height = MainGUISize.Height;
                if (GUI.Button(new Rect(5, mainWindowRect.height - 35, 190, 30), "折叠"))
                {
                    UI_Fold = true;
                }
                
                GUILayout.BeginArea(MainGUISize.UseableRect);
                {
                    GUILayout.BeginArea(new Rect(0, 0, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit));
                    {
                        if (GUI.Button(new Rect(0, 0, MainGUISize.SplitWidth(12), MainGUISize.VerticalUnit), CurrentMagicSwordName))
                        {
                            if (mainWindowRect.x > Screen.width / 2)
                            {
                                magicSworwGenerateWindowRect.x = mainWindowRect.x - magicSworwGenerateWindowRect.width;
                            }
                            else
                            {
                                magicSworwGenerateWindowRect.x = mainWindowRect.xMax;
                            }
                            magicSworwGenerateWindowRect.y = mainWindowRect.y;
                            HasMagicSwordGenerateUIOn = !HasMagicSwordGenerateUIOn;
                            if (HasMagicSwordGenerateUIOn == false)
                            {
                                HasMagicSwordSeletctUIOn = false;
                                HasMagicSwordEntrySeletctUIOn = false;
                            }
                            HasPotionSelectUIOn = false;
                        }
                    }
                    GUILayout.EndArea();
                    GUILayout.BeginArea(new Rect(0, MainGUISize.VerticalUnit, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit * 2));
                    {
                        if (GUI.Button(new Rect(0, 0, MainGUISize.SplitWidth(9), MainGUISize.VerticalUnit), CurrentPotionName))
                        {
                            if (mainWindowRect.x > Screen.width / 2)
                            {
                                potionSelectWindowRect.x = mainWindowRect.x - potionSelectWindowRect.width;
                            }
                            else
                            {
                                potionSelectWindowRect.x = mainWindowRect.xMax;
                            }
                            potionSelectWindowRect.y = mainWindowRect.y;
                            HasPotionSelectUIOn = !HasPotionSelectUIOn;
                            HasMagicSwordGenerateUIOn = false;
                            HasMagicSwordSeletctUIOn = false;
                            HasMagicSwordEntrySeletctUIOn = false;
                        }
                        PotionLevel = GUI.Toolbar(new Rect(0, MainGUISize.VerticalUnit, MainGUISize.SplitWidth(9), MainGUISize.VerticalUnit), PotionLevel, new string[] { "白", "紫", "金" });
                        if (GUI.Button(new Rect(MainGUISize.SplitWidth(9), 0, MainGUISize.SplitWidth(3), MainGUISize.VerticalUnit * 2), "->"))
                        {
                            PotionDropPool.instance.Pop(PotionID, PotionLevel, PlayerAnimControl.instance.transform.position, true, true);
                        }
                    }
                    GUILayout.EndArea();
                    GUILayout.BeginArea(new Rect(0, MainGUISize.VerticalUnit * 3, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit));
                    {
                        if (GUI.Button(new Rect(0, 0, MainGUISize.SplitWidth(4), MainGUISize.VerticalUnit), "普通"))
                        {
                            SkillDropPool.instance.Pop(PlayerAnimControl.instance.transform.position, isGolden: false, isJump: false, isNightmare: false);
                        }
                        if (GUI.Button(new Rect(MainGUISize.SplitWidth(4), 0, MainGUISize.SplitWidth(4), MainGUISize.VerticalUnit), "金书"))
                        {
                            SkillDropPool.instance.Pop(PlayerAnimControl.instance.transform.position, isGolden: true, isJump: false, isNightmare: false);
                        }
                        if (GUI.Button(new Rect(MainGUISize.SplitWidth(8), 0, MainGUISize.SplitWidth(4), MainGUISize.VerticalUnit), "梦魇"))
                        {
                            SkillDropPool.instance.Pop(PlayerAnimControl.instance.transform.position, isGolden: true, isJump: false, isNightmare: true);
                        }
                    }
                    GUILayout.EndArea();
                    GUILayout.BeginArea(new Rect(0, MainGUISize.VerticalUnit * 4, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit));
                    {
                        if (GUI.Button(new Rect(0, 0, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit), "法印"))
                        {
                            var sealitem = NightmarePool.instance.Pop(NightmareMagicSwordPrefabType.SealDrop);
                            sealitem.transform.position = PlayerAnimControl.instance.transform.position + new Vector3(0, 0.5f, 0);
                        }
                    }
                    GUILayout.EndArea();
                    GUILayout.BeginArea(new Rect(0, MainGUISize.VerticalUnit * 5, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit));
                    {
                        GUI.Label(new Rect(0, 0, MainGUISize.SplitWidth(4), MainGUISize.VerticalUnit), "蓝魂");
                        StringPlayerSoulsCount = GUI.TextField(new Rect(MainGUISize.SplitWidth(4), 0, MainGUISize.SplitWidth(8), MainGUISize.VerticalUnit), StringPlayerSoulsCount);
                    }
                    GUILayout.EndArea();
                    GUILayout.BeginArea(new Rect(0, MainGUISize.VerticalUnit * 6, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit));
                    {
                        GUI.Label(new Rect(0, 0, MainGUISize.SplitWidth(6), MainGUISize.VerticalUnit), $"游戏速度: {(GameSpeedSet):0.##}x");
                        string pauseplayhint = GameSpeedSet == 0 ? "D" : "H";
                        var btnHint = new string[] {"<<", "<", pauseplayhint, ">", ">>"};
                        var clickid = GUI.SelectionGrid(new Rect(0, MainGUISize.VerticalUnit, MainGUISize.Width, MainGUISize.VerticalUnit), -1, btnHint, 5);
                        switch (clickid)
                        {
                            case 0:
                                GameSpeedSet = GameSpeedPreset.First();
                                break;
                            case 1:
                                if (GameSpeedSet == 0)
                                {
                                    GameSpeedSet = GameSpeedBeforePause;
                                }
                                if (GameSpeedSet == GameSpeedPreset.First())
                                {
                                    break;
                                }
                                GameSpeedSet = GameSpeedPreset[GameSpeedPreset.IndexOf(GameSpeedSet) - 1];
                                break;
                            case 2:
                                if (GameSpeedSet == 0)
                                {
                                    GameSpeedSet = GameSpeedBeforePause;
                                }
                                else
                                {
                                    GameSpeedBeforePause = GameSpeedSet;
                                    GameSpeedSet = 0;
                                }
                                break;
                            case 3:
                                if (GameSpeedSet == 0)
                                {
                                    GameSpeedSet = GameSpeedBeforePause;
                                }
                                if (GameSpeedSet == GameSpeedPreset.First())
                                {
                                    break;
                                }
                                GameSpeedSet = GameSpeedPreset[GameSpeedPreset.IndexOf(GameSpeedSet) + 1];
                                break;
                            case 4:
                                GameSpeedSet = GameSpeedPreset.Last();
                                break;
                        }
                    }
                    GUILayout.EndArea();
                    GUILayout.BeginArea(new Rect(0, MainGUISize.VerticalUnit * 8, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit));
                    {
                        if (GUI.Button(new Rect(0, 0, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit), "清除小怪 (Z)") || Input.GetKeyDown(KeyCode.Z))
                        {
                            KillAllPool.instance.Pop().transform.position = PlayerAnimControl.instance.transform.position;
                        }
                    }
                    GUILayout.EndArea();
                }
                GUILayout.EndArea();
            }

            GUI.DragWindow();
        }

        void PotionSelectWindow(int id)
        {
            string[] s = new string[PotionsNum];
            for (int i = 1; i <= PotionsNum - 1; i++)
            {
                string str = "PN_NAME_";
                int potionName = i;
                string handle = str + potionName.ToString();
                s[i] = Localization.Instance.GetLocalizedText(handle);
            }
            s[0] = "随机";
            PotionID = GUILayout.SelectionGrid(PotionID, s, 3);
            if (GUI.changed)
            {
                HasPotionSelectUIOn = false;
            }
            GUI.DragWindow();
        }

        void MagicSwordGenerateWindow(int id)
        {
            GUILayout.BeginArea(new Rect(10, 20, MainGUISize.UseableRect.width, MainGUISize.UseableRect.height));
            {
                GUILayout.BeginArea(new Rect(0, 0, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit));
                {
                    if (GUI.Button(new Rect(0, 0, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit), CurrentMagicSwordName))
                    {
                        if (mainWindowRect.x > Screen.width / 2)
                        {
                            magicSwordSelectWindowRect.x = magicSworwGenerateWindowRect.x - magicSwordSelectWindowRect.width;
                        }
                        else
                        {
                            magicSwordSelectWindowRect.x = magicSworwGenerateWindowRect.xMax;
                        }
                        magicSwordSelectWindowRect.y = magicSworwGenerateWindowRect.y;
                        HasMagicSwordSeletctUIOn = !HasMagicSwordSeletctUIOn;
                    }
                }
                GUILayout.EndArea();

                GUILayout.BeginArea(new Rect(0, MainGUISize.VerticalUnit, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit));
                {
                    UserCustomMagicSwordControl.Instance.MagicSwordLevel = GUI.Toolbar(new Rect(0, 0, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit),
                        UserCustomMagicSwordControl.Instance.MagicSwordLevel, new string[] { "白", "蓝", "金", "红" });
                }
                GUILayout.EndArea();

                GUILayout.BeginArea(new Rect(0, MainGUISize.VerticalUnit * 2, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit));
                {
                    UserCustomMagicSwordControl.Instance.EnableNightmareLevel = GUILayout.Toggle(
                        UserCustomMagicSwordControl.Instance.EnableNightmareLevel, "梦魇等级");
                }
                GUILayout.EndArea();

                GUILayout.BeginArea(new Rect(0, MainGUISize.VerticalUnit * 3, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit));
                {
                    if (GUI.Button(new Rect(0, 0, MainGUISize.SplitWidth(8), MainGUISize.VerticalUnit), "选择特效"))
                    {
                        HasMagicSwordEntrySeletctUIOn = !HasMagicSwordEntrySeletctUIOn;
                    }
                    if (GUI.Button(new Rect(MainGUISize.SplitWidth(8), 0, MainGUISize.SplitWidth(4), MainGUISize.VerticalUnit), "清除"))
                    {
                        UserCustomMagicSwordControl.Instance.MagicSwordEntries.Clear();
                    }
                }
                GUILayout.EndArea();

                int voffset = 0;
                string[] temp = new string[] { };
                try
                {
                    temp = UserCustomMagicSwordControl.Instance.StrCurrentEntries.ToArray();
                }
                catch
                {

                }
                foreach (var str in temp)
                {
                    GUI.Label(new Rect(0, MainGUISize.VerticalUnit * (voffset + 4), MainGUISize.UseableRect.width, MainGUISize.VerticalUnit),
                        str, BuffLabelStyle);
                    voffset++;
                }

                GUILayout.BeginArea(new Rect(0, MainGUISize.VerticalUnit * 9, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit));
                {
                    if (GUI.Button(new Rect(0, 0, MainGUISize.SplitWidth(12), MainGUISize.VerticalUnit), "生成"))
                    {
                        UserCustomMagicSwordControl.Instance.Pop();
                    }
                }
                GUILayout.EndArea();
            }
            GUILayout.EndArea();

            GUI.DragWindow();
        }
        void MagicSwordSelectWindow(int id)
        {
            if (HasMagicSwordSeletctUIOn == false)
            {
                return;
            }
            string[] s = new string[WeaponsNum];
            for (int i = 1; i <= WeaponsNum - 1; i++)
            {
                MagicSword ms = new MagicSword
                {
                    magicSwordName = ((MagicSwordName)i)
                };
                s[i] = TextControl.instance.MagicSwordInfo(ms)[0];
            }
            s[0] = "随机";
            UserCustomMagicSwordControl.Instance.MagicSwordID = GUILayout.SelectionGrid(UserCustomMagicSwordControl.Instance.MagicSwordID, s, 3);
            if (GUI.changed)
            {
                HasMagicSwordSeletctUIOn = false;
            }
            GUI.DragWindow();
        }

        private Vector2 scrollViewVector2 = Vector2.zero;
        void MagicSwordEntrySelectWindow(int id)
        {
            if (HasMagicSwordEntrySeletctUIOn == false)
            {
                return;
            }
            string[] s = new string[MSEntriesNum + 1];
            for (int i = 0; i < MSEntriesNum + 1; i++)
            {
                MagicSwordEntry entry = new MagicSwordEntry
                {
                    magicSwordEntryName = (MagicSwordEntryName)i,
                    values = 0,
                };
                s[i] = ((int)entry.magicSwordEntryName >= 24 ? "梦魇：" : "") + TextControl.instance.MagicSwordEntryDescribe(entry).Split('+').First().Split('-').First();
            }
            s[MSEntriesNum] = "随机";

            scrollViewVector2 = GUI.BeginScrollView(new Rect(0, 0, 500, 500), scrollViewVector2, new Rect(0, 0, 500, 2000));
            MagicSwordEntryID = GUILayout.SelectionGrid(MagicSwordEntryID, s, 1);
            GUI.EndScrollView();
            if (GUI.changed)
            {
                int entryindex = MagicSwordEntryID;
                if (MagicSwordEntryID == MSEntriesNum)
                {
                    entryindex = GlobalParameter.instance.SysRand.Next(0, MSEntriesNum);
                }

                string logHandler = $"{entryindex}";
                Logger.LogInfo(logHandler);
                UserCustomMagicSwordControl.Instance.AddMagicSwordEntry((MagicSwordEntryName)entryindex);
            }
            GUI.DragWindow();
        }

        bool showtransport = false;
        private Vector2 scrollViewVector = Vector2.zero;
        void TransportSelectionWindow(int id)
        {
            int temp = 0;
            scrollViewVector = GUI.BeginScrollView(new Rect(10, 20, 280, 580), scrollViewVector, new Rect(0, 0, 260, 10000));
            temp = GUI.SelectionGrid(new Rect(0,0,260, 9000),temp, TransportControl.Instance.SceneNameList.ToArray(), 1);
            GUI.EndScrollView();
            if (GUI.changed)
            {
                TransportControl.Instance.TransportScene(temp);
            }
        }

    }

    internal static class MainGUISize
    {
        public static int Height { get; } = 400;
        public static int Width { get; } = 200;
        public static int HorizontalGap { get; } = 10;
        public static int VerticalGap { get; } = 5;
        public static Vector2 WindowSize { get; } = new Vector2(Width, Height);
        public static Rect UseableRect { get; } = new Rect(HorizontalGap, 20, Width - HorizontalGap * 2, Height - VerticalGap * 2);
        public static int VerticalUnit { get; } = 30;
        public static int HorizontalUnit => (int)UseableRect.width / HorizontalSplitNum;
        public static int HorizontalSplitNum { get; } = 12;
        public static int SplitWidth(int num)
        {
            num = Math.Min(HorizontalSplitNum, num);
            num = Math.Max(num, 0);
            return HorizontalUnit * num;
        }
    }

    public class ModBuffData
    {
        readonly BuffData _buff;
        public ModBuffData(BuffData buff)
        {
            _buff = buff;
        }
        public static string BuffHeader => $"{"类型:",-20}{"值/层数：",-20}{"时间：",-15}";
        public override string ToString()
        {
            return $"{_buff.buffType,-20}{($"{_buff.value,-1:##0.0}/{_buff.stackLayer,-1:##0.#}"),-20}{$"{_buff.excuteTime - _buff.curtimer,6:0.0}/{_buff.excuteTime,6:0.0}",-15}";
        }
        public string StrBuffType => $"{_buff.buffType}";
        public string StrBuffValue => $"{_buff.value,-1:##0.0}/{_buff.stackLayer,-1:##0.#}";
        public string StrBuffTime => $"{_buff.excuteTime - _buff.curtimer,6:0.0}/{_buff.excuteTime,6:0.0}";
    }

    public class TransportControl
    {
        public static TransportControl Instance { get; } = new TransportControl();
        public List<Scene> Scenes { get; private set; } = new List<Scene>(EnumScene());
        public List<string> ScenePaths { get; private set; } = new List<string>(EnumScenePath());
        public List<string> SceneNameList
        {
            get
            {
                List<string> ret = new List<string>();
                ScenePaths.ForEach(sc => ret.Add(sc.Split('/').Last().Split('.').First()));
                return ret;
            }
        }
        public static IEnumerable<Scene> EnumScene()
        {
            foreach(var path in EnumScenePath())
            {
                yield return SceneManager.GetSceneByPath(path);
            }
            yield break;
        }
        public static IEnumerable<string> EnumScenePath()
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                yield return SceneUtility.GetScenePathByBuildIndex(i);
            }
            yield break;
        }
        public void TransportScene(Scene destination)
        {
            SceneManager.LoadScene(destination.name);
        }
        public void TransportScene(int destination)
        {
            SceneManager.LoadScene(destination);
        }
        TransportControl()
        {
            
        }
    }

    public class UserCustomMagicSwordControl
    {
        public static UserCustomMagicSwordControl Instance = new UserCustomMagicSwordControl();
        public delegate void LogInfoHandler(string info);
        public event LogInfoHandler LogInfo;
        public UserCustomMagicSwordControl()
        {
            
        }
        public int GetNightmareLevel(MagicSwordEntryName name)
        {
            if (EnableNightmareLevel == false) return 0;
            if ((int)name < 24) return 0;//(int)MagicSwordEntryName.nightmareDmgUp
            if ((int)name <= 39) return 1;//(int)MagicSwordEntryName.nightmareQuadrupleDamage
            if ((int)name <= 48) return 2;//(int)MagicSwordEntryName.nightmareCritical
            return 3;
        }
        public float GetMagicSwordEntryMaxValue(MagicSwordEntryName name)
        {
            switch (name)
            {
                case MagicSwordEntryName.meeleDamage:
                    return 30f / 100;
                case MagicSwordEntryName.bladeBoltDamage:
                    return 30f / 100; ;
                case MagicSwordEntryName.damage:
                    return 25f / 100; ;
                case MagicSwordEntryName.ignoreDefense:
                    return 30f / 100; ;
                case MagicSwordEntryName.attackSpeed:
                    return 40f / 100; ;
                case MagicSwordEntryName.drawSwordCD:
                    return 30f / 100; ;
                case MagicSwordEntryName.poisonExtraAttack:
                    return 40f / 100; ;
                case MagicSwordEntryName.drunkExtraAttack:
                    return 40f / 100; ;
                case MagicSwordEntryName.burnExtraAttack:
                    return 40f / 100; ;
                case MagicSwordEntryName.bleedExtraAttack:
                    return 40f / 100; ;
                case MagicSwordEntryName.freezingExtraAttack:
                    return 40f / 100; ;
                case MagicSwordEntryName.stunExtraAttack:
                    return 60f / 100; ;
                case MagicSwordEntryName.moveSpeed:
                    return 15f / 100; ;
                case MagicSwordEntryName.superRarePotionProb:
                    return 15f / 100; ;
                case MagicSwordEntryName.extraSoulsRate:
                    return 1; ;
                case MagicSwordEntryName.extraRageTimes:
                    return 20f / 100; ;
                case MagicSwordEntryName.fireDamage:
                    return 45f / 100;
                case MagicSwordEntryName.iceDamage:
                    return 45f / 100;
                case MagicSwordEntryName.poisonDamage:
                    return 45f / 100; ;
                case MagicSwordEntryName.thunderDamage:
                    return 45f / 100;
                case MagicSwordEntryName.injury1:
                    return 15f / 100; ;
                case MagicSwordEntryName.doubleDamage:
                    return 15f / 100; ;
                case MagicSwordEntryName.quadrupleDamage:
                    return 10f / 100; ;
                case MagicSwordEntryName.injury2:
                    return 10f / 100; ;
                case MagicSwordEntryName.nightmareDmgUp:
                    break;
                case MagicSwordEntryName.nightmareMeleeDmgUp:
                    break;
                case MagicSwordEntryName.nightmareRangeDmgUp:
                    break;
                case MagicSwordEntryName.nightmareFireDmgUp:
                    break;
                case MagicSwordEntryName.nightmareIceDmgUp:
                    break;
                case MagicSwordEntryName.nightmareThunderDmgUp:
                    break;
                case MagicSwordEntryName.nightmarePoisonDmgUp:
                    break;
                case MagicSwordEntryName.nightmareDrawSwordCoolDown:
                    break;
                case MagicSwordEntryName.nightmareIgnoreDefense:
                    break;
                case MagicSwordEntryName.nightmareDefense:
                    break;
                case MagicSwordEntryName.nightmareHp:
                    break;
                case MagicSwordEntryName.nightmareBloodSlash:
                    break;
                case MagicSwordEntryName.nightmarePoisonSlash:
                    break;
                case MagicSwordEntryName.nightmareBurnSlash:
                    break;
                case MagicSwordEntryName.nightmareFrozenSlash:
                    break;
                case MagicSwordEntryName.nightmareQuadrupleDamage:
                    break;
                case MagicSwordEntryName.nightmareWound:
                    break;
                case MagicSwordEntryName.nightmareSlash:
                    break;
                case MagicSwordEntryName.nightmareStorm:
                    break;
                case MagicSwordEntryName.nightmareFire:
                    break;
                case MagicSwordEntryName.nightmareIce:
                    break;
                case MagicSwordEntryName.nightmareThunder:
                    break;
                case MagicSwordEntryName.nightmarePoison:
                    break;
                case MagicSwordEntryName.nightmareAgility:
                    break;
                case MagicSwordEntryName.nightmareCritical:
                    break;
                case MagicSwordEntryName.nightmareWave:
                    break;
                case MagicSwordEntryName.nightmareStarExplosion:
                    break;
                case MagicSwordEntryName.nightmareAvatar:
                    break;
                case MagicSwordEntryName.nightmareCorrosive:
                    break;
                case MagicSwordEntryName.nightmareBloodBlade:
                    break;
                case MagicSwordEntryName.nightmareSting:
                    break;
                case MagicSwordEntryName.nightmareWhale:
                    break;
                case MagicSwordEntryName.nightmareBody:
                    break;
                case MagicSwordEntryName.nightmareChaos:
                    break;
                default:
                    return 0;
            }
            return 0;
        }
        public List<MagicSwordEntry> MagicSwordEntries { get; private set; } = new List<MagicSwordEntry>();
        public int MagicSwordLevel { get; set; } = 3;
        public int MagicSwordID { get; set; } = 0;
        public bool EnableNightmareLevel { get; set; } = true;
        public bool AllowPopMagicSword => MagicSwordEntries.Count == Math.Min(MagicSwordLevel + 2, 5);
        public IEnumerable<string> StrCurrentEntries
        {
            get
            {
                return from entry in MagicSwordEntries
                       select ((int)entry.magicSwordEntryName >= 24 ? "梦魇：":"") + TextControl.instance.MagicSwordEntryDescribe(entry);
            }
        }
        public bool AddMagicSwordEntry(MagicSwordEntryName name, float value = -1)
        {
            try
            {
                if (value < 0)
                {
                    value = GetMagicSwordEntryMaxValue(name);
                }
                if (MagicSwordEntries.Count >= Math.Min(MagicSwordLevel + 2, 5))
                {
                    return false;
                }
                MagicSwordEntry newEntry = new MagicSwordEntry
                {
                    magicSwordEntryName = name,
                    isNightmare = (int)name >= (int)MagicSwordEntryName.nightmareDmgUp,
                    values = value,
                    level = GetNightmareLevel(name),
                };
                MagicSwordEntries.Add(newEntry);
                var sort = from entry in MagicSwordEntries
                           orderby (int)entry.magicSwordEntryName ascending
                           select entry;
                MagicSwordEntries = new List<MagicSwordEntry>(sort);
                LogInfo($"Add MSEntry{MagicSwordEntries.Count}, {name}");
            }
            catch(Exception ex)
            {
                LogInfo($"On Add MS Entry: {ex.Message}\r\n{ex.StackTrace}" );
                return false;
            }
            return true;
        }
        public GameObject Pop()
        {
            try
            {
                while (AllowPopMagicSword == false)
                {
                    AddMagicSwordEntry((MagicSwordEntryName)GlobalParameter.instance.SysRand.Next(0, TestBench.MSEntriesNum));
                    if (MagicSwordEntries.Count > Math.Min(MagicSwordLevel + 2, 5))
                    {
                        MagicSwordEntries.Clear();
                    }
                }
                if (MagicSwordPool.instance is null)
                {
                    LogInfo("MSPool is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogInfo($"On Pop MS: {ex.Message}\r\n{ex.StackTrace}");
                return null;
            }
            return MagicSwordPool.instance.Pop(MagicSwordID, MagicSwordLevel, MagicSwordEntries, PlayerAnimControl.instance.transform.position, isJump: false);
        }
        public override string ToString()
        {
            return $"{this.EnableNightmareLevel}, {this.MagicSwordID}, {this.MagicSwordLevel}";
        }
    }

}
