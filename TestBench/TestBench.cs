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
using Epic.OnlineServices;
using JetBrains.Annotations;
using static UnityEngine.EventSystems.EventTrigger;
/******************************
* Some Note Here
* ****************************
* 
* 1. Use DummyUIControl to get Damage Value.(2022/10/19) Not Done.
* 
* 2. Use UnityEditor to Make a New Buff Property Window instead of IMGUI.(2022/10/19) Ignored.
* 
* 3. Fix Weapon Support. Not Done.
* 
* 4. Add a Support that can Change Weapon Effects.(2022/10/19) Not Done.
* 
*/

namespace TestBench
{
    [BepInPlugin("com.nekoice.plugin.testbench", "TestBench", "2.0.0")]
    public class TestBench : BaseUnityPlugin
    {
        Rect mainWindowRect = new Rect(new Vector2(500, 300), MainGUISize.WindowSize);
        Rect potionSelectWindowRect  = new Rect(0, 0, 500, 400);
        Rect magicSwordSelectWindowRect = new Rect(0, 0, 500, 500);
        Rect magicSworwGenerateWindowRect = new Rect(0, 0, 200, 500);
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
        bool HasPotionSelectUIOn { get; set; } = false;
        bool HasMagicSwordGenerateUIOn { get; set; } = false;
        bool HasMagicSwordSeletctUIOn { get; set; } = false;
        bool HasMagicSwordEntrySeletctUIOn { get; set; } = false;
        bool UI_Fold { get; set; } = true;
        int soul = 0;
        public static int PotionsNum => Enum.GetNames(typeof(PN)).Length;
        public static int WeaponsNum => Enum.GetNames(typeof(MagicSwordName)).Length;
        public static int MSEntriesNum => Enum.GetNames(typeof(MagicSwordEntryName)).Length;
        GameObject DummyObject { get; set; }
        bool HasDummy => DummyObject != null;
        void Start()
        {
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
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
            if (HasDummy)
            {
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
                GUILayout.Label(ModBuffData.BuffHeader, BuffLabelStyle);
                EnemyControl bec = DummyObject.GetComponent<EnemyControl>();
                foreach (BuffData buff in bec.buffAction.buffs)
                {
                    GUILayout.Label($"{new ModBuffData(buff)}", BuffLabelStyle);
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
                        GUI.Label(new Rect(0, 0, MainGUISize.SplitWidth(4), MainGUISize.VerticalUnit), "蓝魂");
                        StringPlayerSoulsCount = GUI.TextField(new Rect(MainGUISize.SplitWidth(4), 0, MainGUISize.SplitWidth(8), MainGUISize.VerticalUnit), StringPlayerSoulsCount);
                    }
                    GUILayout.EndArea();
                    GUILayout.BeginArea(new Rect(0, MainGUISize.VerticalUnit * 5, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit));
                    {
                        if (GUI.Button(new Rect(0, 0, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit), "法印"))
                        {
                            var sealitem = NightmarePool.instance.Pop(NightmareMagicSwordPrefabType.SealDrop);
                            sealitem.transform.position = PlayerAnimControl.instance.transform.position + new Vector3(0, 0.5f, 0);
                        }
                    }
                    GUILayout.EndArea();
                    GUILayout.BeginArea(new Rect(0, MainGUISize.VerticalUnit * 6, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit));
                    {
                        if (GUI.Button(new Rect(0, 0, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit), "清除小怪 (Z)") || Input.GetKeyDown(KeyCode.Z))
                        {
                            KillAllPool.instance.Pop().transform.position = PlayerAnimControl.instance.transform.position;
                        }
                    }
                    GUILayout.EndArea();
                }
                GUILayout.EndArea();
                showtransport = false;
                if (Input.GetKeyDown(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.X))
                {
                    showtransport = true;
                }
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
            GUILayout.BeginArea(new Rect(0, 0, MainGUISize.UseableRect.width, MainGUISize.UseableRect.height));
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
                    GUILayout.BeginHorizontal();
                    {
                        UserCustomMagicSwordControl.Instance.EnableNightmareLevel = GUI.Toggle( new Rect(0,0,MainGUISize.SplitWidth(6), MainGUISize.VerticalUnit), 
                            UserCustomMagicSwordControl.Instance.EnableNightmareLevel, "梦魇等级", LabelStyle);
                        UserCustomMagicSwordControl.Instance.IsNightmare = GUI.Toggle(new Rect(MainGUISize.SplitWidth(6), 0, MainGUISize.SplitWidth(6), MainGUISize.VerticalUnit), 
                            UserCustomMagicSwordControl.Instance.IsNightmare, "梦魇剑", LabelStyle);
                    }
                    GUILayout.EndHorizontal();
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
                GUILayout.BeginArea(new Rect(0, MainGUISize.VerticalUnit * 4, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit * 4));
                {
                    foreach(var str in UserCustomMagicSwordControl.Instance.StrCurrentEntries)
                    {
                        GUILayout.Label(str, BuffLabelStyle);
                    }
                }
                GUILayout.EndArea();
                GUILayout.BeginArea(new Rect(0, MainGUISize.VerticalUnit * 8, MainGUISize.UseableRect.width, MainGUISize.VerticalUnit));
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
                HasMagicSwordGenerateUIOn = false;
            }
            GUI.DragWindow();
        }

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
                s[i] = TextControl.instance.MagicSwordEntryDescribe(entry).Split('+').First().Split('-').First();
            }
            s[MSEntriesNum] = "随机";
            MagicSwordEntryID = GUILayout.SelectionGrid(MagicSwordEntryID, s, 3);
            if (GUI.changed)
            {
                int entryindex = MagicSwordEntryID;
                if (MagicSwordEntryID == MSEntriesNum)
                {
                    entryindex = GlobalParameter.instance.SysRand.Next(0, MSEntriesNum);
                }
                UserCustomMagicSwordControl.Instance.AddMagicSwordEntry((MagicSwordEntryName)entryindex);
            }
            GUI.DragWindow();
        }

        bool showtransport = false;
        private Vector2 scrollViewVector = Vector2.zero;
        private string innerText = "I am inside the ScrollView";
        void TransportSelectionWindow(int id)
        {
            int temp = 0;
            scrollViewVector = GUI.BeginScrollView(new Rect(0, 0, 300, 600), scrollViewVector, new Rect(0, 0, 300, 2000));
            temp = GUILayout.SelectionGrid(temp, TransportControl.Instance.SceneNameList.ToArray(), 1);
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
        public static Rect UseableRect { get; } = new Rect(HorizontalGap, VerticalGap, Width - HorizontalGap * 2, Height - VerticalGap * 2);
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
    }

    public class TransportControl
    {
        public static TransportControl Instance { get; } = new TransportControl();
        public List<Scene> Scenes { get; private set; } = new List<Scene>(EnumScene());
        public List<string> SceneNameList
        {
            get
            {
                List<string> ret = new List<string>();
                Scenes.ForEach(sc => ret.Add(sc.name));
                return ret;
            }
        }
        public static IEnumerable<Scene> EnumScene()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                yield return SceneManager.GetSceneAt(i);
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
        public static UserCustomMagicSwordControl Instance { get; } = new UserCustomMagicSwordControl();
        public int GetNightmareLevel(MagicSwordEntryName name)
        {
            if (EnableNightmareLevel == false) return 0;
            if (name < MagicSwordEntryName.nightmareDmgUp) return 0;
            if (name <= MagicSwordEntryName.nightmareQuadrupleDamage) return 1;
            if (name <= MagicSwordEntryName.nightmareCritical) return 2;
            return 3;
        }
        public int GetMagicSwordEntryMaxValue(MagicSwordEntryName name)
        {
            switch (name)
            {
                case MagicSwordEntryName.meeleDamage:
                    return 30;
                case MagicSwordEntryName.bladeBoltDamage:
                    return 30;
                case MagicSwordEntryName.damage:
                    return 25;
                case MagicSwordEntryName.ignoreDefense:
                    return 30;
                case MagicSwordEntryName.attackSpeed:
                    return 40;
                case MagicSwordEntryName.drawSwordCD:
                    return 30;
                case MagicSwordEntryName.poisonExtraAttack:
                    return 40;
                case MagicSwordEntryName.drunkExtraAttack:
                    return 40;
                case MagicSwordEntryName.burnExtraAttack:
                    return 40;
                case MagicSwordEntryName.bleedExtraAttack:
                    return 40;
                case MagicSwordEntryName.freezingExtraAttack:
                    return 40;
                case MagicSwordEntryName.stunExtraAttack:
                    return 60;
                case MagicSwordEntryName.moveSpeed:
                    return 15;
                case MagicSwordEntryName.superRarePotionProb:
                    return 15;
                case MagicSwordEntryName.extraSoulsRate:
                    return 100;
                case MagicSwordEntryName.extraRageTimes:
                    return 20;
                case MagicSwordEntryName.fireDamage:
                    return 45;
                case MagicSwordEntryName.iceDamage:
                    return 45;
                case MagicSwordEntryName.poisonDamage:
                    return 45;
                case MagicSwordEntryName.thunderDamage:
                    return 45;
                case MagicSwordEntryName.injury1:
                    return 15;
                case MagicSwordEntryName.doubleDamage:
                    return 15;
                case MagicSwordEntryName.quadrupleDamage:
                    return 10;
                case MagicSwordEntryName.injury2:
                    return 10;
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
        public List<MagicSwordEntry> MagicSwordEntries { get; private set; }
        public int MagicSwordLevel { get; set; } = 3;
        public int MagicSwordID { get; set; } = 0;
        public bool IsNightmare { get; set; } = false;
        public bool EnableNightmareLevel { get; set; } = true;
        public bool AllowPopMagicSword => MagicSwordEntries.Count == Math.Min(MagicSwordLevel + 2, 5);
        public IEnumerable<string> StrCurrentEntries
        {
            get
            {
                return from entry in MagicSwordEntries
                       select TextControl.instance.MagicSwordEntryDescribe(entry);
            }
        }
        public bool AddMagicSwordEntry(MagicSwordEntryName name, int value = -1)
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
                isNightmare = name < MagicSwordEntryName.nightmareDmgUp,
                values = value,
                level = GetNightmareLevel(name),
            };
            MagicSwordEntries.Add(newEntry);
            var sort = from entry in MagicSwordEntries
                       orderby entry.magicSwordEntryName ascending
                       select entry;
            MagicSwordEntries = new List<MagicSwordEntry>(sort);
            return true;
        }
        public GameObject Pop()
        {
            while(AllowPopMagicSword == false)
            {
                AddMagicSwordEntry((MagicSwordEntryName)GlobalParameter.instance.SysRand.Next(0, TestBench.MSEntriesNum));
                if (MagicSwordEntries.Count > Math.Min(MagicSwordLevel + 2, 5))
                {
                    MagicSwordEntries.Clear();
                }
            }
            if (MagicSwordPool.instance is null) return null;
            return MagicSwordPool.instance.Pop(MagicSwordID, MagicSwordLevel, MagicSwordEntries, PlayerAnimControl.instance.transform.position, false);
        }
    }

}
