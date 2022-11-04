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
/******************************
* Some Note Here
* ****************************
* 
* 1. Use DummyUIControl to get Damage Value.(2022/10/19) Not Done.
* 
* 2. Use UnityEditor to Make a New Buff Property Window instead of IMGUI.(2022/10/19) Not Done.
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
        Rect mainWindowRect  = new Rect(500, 300, 200, 450);
        Rect potionSelectWindowRect  = new Rect(0, 0, 500, 400);
        Rect magicSworwSelectWindowRect = new Rect(0, 0, 500, 500);
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
        private int PotionID { get; set; } = 1;
        private int PotionLevel { get; set; } = 2;
        private int MagicSwordID { get; set; } = 1;
        private int MagicSwordLevel { get; set; } = 3;
        bool HasPotionSelectUIOn { get; set; } = false;
        bool HasMagicSwordSelectUIOn { get; set; } = false;
        bool UI_Fold { get; set; } = true;
        int soul = 0;
        static int PotionsNum => Enum.GetNames(typeof(PN)).Length;
        static int WeaponsNum => Enum.GetNames(typeof(MagicSwordName)).Length;
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
            if (HasMagicSwordSelectUIOn)
            {
                magicSworwSelectWindowRect = GUI.Window("武器".GetHashCode(), magicSworwSelectWindowRect, MagicSwordSelectWindow, "武器");
            }
            if (HasDummy)
            {
                buffWindowRect = GUI.Window("木桩".GetHashCode(), buffWindowRect, BuffListWindow, "木桩");
            }
        }
        
        void BuffListWindow(int id)
        {
            Rect r = new Rect(5, 20, buffWindowRect.width - 10, 25);
            Rect r2 = new Rect(r.x, r.y, r.width/2, r.height);
            GUI.Label(r2, "BUFF类型", LabelStyle);
            r2.x += r2.width;
            r2.width *= 0.5f;
            GUI.Label(r2, "值/层数", LabelStyle);
            r2.x += r2.width;
            GUI.Label(r2, "时间", LabelStyle);
            r.y += r.height;
            if (HasDummy)
            {
                EnemyControl bec = DummyObject.GetComponent<EnemyControl>();
                foreach (BuffData buff in bec.buffAction.buffs)
                {
                    SingleBuffLine(buff, r);
                    r.y += r.height;
                }
            }
            GUI.DragWindow();
        }

        void SingleBuffLine(BuffData bd, Rect rect)
        {
            rect.width *= 0.5f;
            GUI.Label(rect, bd.buffType.ToString(), LabelStyle);
            rect.x += rect.width;
            rect.width *= 0.5f;
            GUI.Label(rect, $"{bd.value:##0.0}/{bd.stackLayer:##0.#}", LabelStyle);
            rect.x += rect.width;
            GUI.Label(rect, $"{bd.excuteTime - bd.curtimer:0.0}/{bd.excuteTime:0.0}", LabelStyle);
        }

        void TestBenchWindow(int id)
        {
            if (UI_Fold || PlayerAnimControl.instance == null)
            {
                if (GUI.Button(new Rect(5, 20, 190, 30), "展开")) UI_Fold = false;
                mainWindowRect.height = 60;
                
            }
            else
            {
                mainWindowRect.height = 400;
                if (GUI.Button(new Rect(5, mainWindowRect.height-35, 190, 30), "折叠")) UI_Fold = true;
                
                Rect r = new Rect(5, 20, 140, 30);
                if (GUI.Button(r, ((PN)PotionID).ToString())) { 
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
                    HasMagicSwordSelectUIOn = false;
                }
                r.Set(5, 50, 140, 30);
                PotionLevel = GUI.Toolbar(r, PotionLevel, new string[] { "白", "紫", "金" });
                r.Set(150, 20, 45, 60);
                if (GUI.Button(r, "->"))
                {
                    PotionDropPool.instance.Pop(PotionID, PotionLevel, PlayerAnimControl.instance.transform.position, true, true);
                }
                r.Set(5, 85, 140, 30);
                if (GUI.Button(r, ((MagicSwordName)MagicSwordID).ToString()))
                {
                    if (mainWindowRect.x > Screen.width / 2)
                    {
                        magicSworwSelectWindowRect.x = mainWindowRect.x - magicSworwSelectWindowRect.width;
                    }
                    else
                    {
                        magicSworwSelectWindowRect.x = mainWindowRect.xMax;
                    }
                    magicSworwSelectWindowRect.y = mainWindowRect.y;
                    HasMagicSwordSelectUIOn = !HasMagicSwordSelectUIOn;
                    HasPotionSelectUIOn = false;
                }
                r.Set(5, 115, 140, 30);
                MagicSwordLevel = GUI.Toolbar(r, MagicSwordLevel, new string[] { "白", "蓝", "金", "红" });
                r.Set(150, 85, 45, 60);
                if (GUI.Button(r, "->"))
                {
                    List<MagicSwordEntry> list = MagicSwordControl.instance.RandomEntrys((MagicSwordName)MagicSwordID, MagicSwordLevel);
                    MagicSwordPool.instance.Pop(MagicSwordID, MagicSwordLevel, list, PlayerAnimControl.instance.transform.position, true, true);
                }
                r.Set(5, 150, 90, 30);
                if (GUI.Button(r, "普通书")) SkillDropPool.instance.Pop(PlayerAnimControl.instance.transform.position, false, true);
                r.Set(105, 150, 90, 30);
                if (GUI.Button(r, "金书")) SkillDropPool.instance.Pop(PlayerAnimControl.instance.transform.position, true, true);
                r.Set(5, 185, 140, 30);
                soul = int.Parse(GUI.TextField(r, soul.ToString()));
                r.Set(150, 185, 45, 30);
                if (GUI.Button(r, "魂")) PlayerAnimControl.instance.Souls = soul;
                r.Set(5, 215, 140, 30);
                GUI.TextField(r, PlayerAnimControl.instance.RedSouls.ToString());
                r.Set(150, 215, 45, 30);
                if (GUI.Button(r, "红魂")) { }
                r.Set(5, 250, 190, 30);
                if (GUI.Button(r, "清除小怪 (Z)") || Input.GetKeyDown(KeyCode.Z))
                {
                    KillAllPool.instance.Pop().transform.position = PlayerAnimControl.instance.transform.position;
                }
            }
            GUI.DragWindow();
        }

        void PotionSelectWindow(int id)
        {
            string[] s = new string[PotionsNum];
            for (int i = 1; i <= PotionsNum - 1; i++)
            {
                //Potion p = new Potion();
                //p.PotionName = ((PN)i);
                //s[i] = TextControl.instance.PotionTitle(p, true);
                //int f = s[i].Remove(s[i].IndexOf());

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

        void MagicSwordSelectWindow(int id)
        {
            string[] s = new string[WeaponsNum];
            for (int i = 1; i <= WeaponsNum - 1; i++)
            {
                MagicSword ms = new MagicSword();
                ms.magicSwordName = ((MagicSwordName)i);
                s[i] = TextControl.instance.MagicSwordInfo(ms)[0];
            }
            s[0] = "随机";
            MagicSwordID = GUILayout.SelectionGrid(MagicSwordID, s, 3);
            if (GUI.changed)
            {
                HasMagicSwordSelectUIOn = false;
            }
            GUI.DragWindow();
        }

    }

}
