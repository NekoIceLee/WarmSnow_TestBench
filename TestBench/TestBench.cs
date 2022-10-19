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


namespace TestBench
{
    [BepInPlugin("com.nekoice.plugin.testbench", "TestBench", "1.0.0")]
    public class TestBench : BaseUnityPlugin
    {
        private Rect mwdnd = new Rect(500, 300, 200, 450);
        Rect psuiwnd = new Rect(0, 0, 500, 400);
        Rect msuiwnd = new Rect(0, 0, 500, 500);
        Rect bfuiwnd = new Rect(0, 0, 400, 400);

        GUIStyle labelStyle = new GUIStyle();
        private int potionid = 1;
        private int potionlevel = 2;
        private int msid = 1;
        private int mslevel = 3;
        int sbid = 0;
        bool psuion = false;
        bool msuion = false;
        bool fold = true;
        bool afterdeath = false;

        int soul = 0;
        int redsoul = 0;

        float dpstimer025 = 0;
        float dpstimer1 = 0;
        float dpstimer5 = 0;
        float dps025 = 0;
        float dps1 = 0;
        float dps5 = 0;
        float dpf = 0;
        float maxdps025 = 0;
        float maxdps1 = 0;
        float maxdps5 = 0;
        float maxdpf = 0;
        float tempdps025 = 0;
        float tempdps1 = 0;
        float tempdps5 = 0;

        GameObject sb;
        bool hasgen = false;

        void Start()
        {
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontStyle = FontStyle.Normal;
            labelStyle.font = null;
            labelStyle.margin = new RectOffset(10, 10, 10, 10);
            labelStyle.fontSize = 16;
        }

        void Update()
        {
            if (PlayerAnimControl.instance == null) { sb = null; hasgen = false; afterdeath = false; }
            if (hasgen && sb != null)
            {
                GlobalParameter.instance.diff_fishTailTimer = 0;

                EnemyControl ec = sb.GetComponent<EnemyControl>();
                ec.specialDiffTimer = 0;
                ec.specialDotDiffTimer = 0;
                ec.isStun = true;
                if (!ec.isDeath)
                {
                    dpf = ec.enemyParameter.MAX_HP - ec.enemyParameter.HP;
                    dpstimer025 += Time.deltaTime;
                    dpstimer1 += Time.deltaTime;
                    dpstimer5 += Time.deltaTime;
                    tempdps025 += dpf;
                    tempdps1 += dpf;
                    tempdps5 += dpf;
                    ec.enemyParameter.HP = ec.enemyParameter.MAX_HP;
                }
                else
                {
                    sb = null;
                    maxdpf = 16777216;
                    afterdeath = true;
                }

                
                
                if (dpf > maxdpf)
                {
                    maxdpf = dpf;
                }

                
                if (dpstimer025 > 0.25)
                {
                    dpstimer025 = 0;
                    dps025 = tempdps025;
                    dps025 *= 4f;
                    tempdps025 = 0;
                    if (dps025 > maxdps025)
                    {
                        maxdps025 = dps025;
                    }
                }
                if (dpstimer1 > 1)
                {
                    dpstimer1 = 0;
                    dps1 = tempdps1;
                    tempdps1 = 0;
                    if (dps1 > maxdps1)
                    {
                        maxdps1 = dps1;
                    }
                }
                if (dpstimer5 > 5)
                {
                    dpstimer5 = 0;
                    dps5 = tempdps5;
                    dps5 *= 0.2f;
                    tempdps5 = 0;
                    if (dps5 > maxdps5)
                    {
                        maxdps5 = dps5;
                    }
                }
                if (PlayerAnimControl.instance != null && MenuSkillLearn.instance != null)
                {
                    if (true)
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
        }

        void OnGUI()
        {
            mwdnd = GUI.Window(4444, mwdnd, TestBenchWindow, "测试台");
            if (psuion)
            {
                psuiwnd = GUI.Window(4445, psuiwnd, potionselectwindow, "圣物");
            }
            if (msuion)
            {
                msuiwnd = GUI.Window(4446, msuiwnd, msselectwindow, "武器");
            }
            if (hasgen || afterdeath)
            {
                
                bfuiwnd = GUI.Window(4447, bfuiwnd, sandbagbufflist, "木桩");
            }
            if (Input.GetKey(KeyCode.L))
            {
                GUI.Window(4448, new Rect(0, 0, 200, 100), sbselectwindow, "sb");
            }
        }
        
        void sandbagbufflist(int id)
        {
            
            Rect r = new Rect(5, 20, bfuiwnd.width - 10, 25);
            dpshead(r);
            r.y += r.height;
            singledpsline("DPS(0.25s): ", dps025, maxdps025, r);
            r.y += r.height;
            singledpsline("DPS(1s): ", dps1, maxdps1, r);
            r.y += r.height;
            singledpsline("DPS(5s): ", dps5, maxdps5, r);
            r.y += r.height;
            singledpsline("DPF: ", dpf, maxdpf, r);
            r.y += r.height;
            if (GUI.Button(r, "重置数据")) { maxdpf = 0; maxdps025 = 0; maxdps1 = 0; maxdps5 = 0; }
            r.y += r.height;
            Rect r2 = new Rect(r.x, r.y, r.width/2, r.height);
            GUI.Label(r2, "BUFF类型", labelStyle);
            r2.x += r2.width;
            r2.width *= 0.5f;
            GUI.Label(r2, "值/层数", labelStyle);
            r2.x += r2.width;
            GUI.Label(r2, "时间", labelStyle);
            r.y += r.height;
            if (sb != null)
            {
                EnemyControl bec = sb.GetComponent<EnemyControl>();
                foreach (BuffData buff in bec.buffAction.buffs)
                {
                    singlebuffline(buff, r);
                    r.y += r.height;
                }
            }
            GUI.DragWindow();
        }

        void singlebuffline(BuffData bd, Rect rect)
        {
            rect.width *= 0.5f;
            GUI.Label(rect, bd.buffType.ToString(), labelStyle);
            rect.x += rect.width;
            rect.width *= 0.5f;
            GUI.Label(rect, bd.value.ToString("##0.0") +"/" + (bd.stackLayer).ToString("##0.#"), labelStyle);
            rect.x += rect.width;
            GUI.Label(rect, (bd.excuteTime - bd.curtimer).ToString("0.0") + "/" + bd.excuteTime.ToString("0.0"), labelStyle);
        }

        void singledpsline(string desc, float dps, float maxdps, Rect rect)
        {
            rect.width *= 0.33f;
            GUI.Label(rect, desc, labelStyle);
            rect.x += rect.width;
            GUI.Label(rect, dps.ToString("0.0"), labelStyle);
            rect.x += rect.width;
            GUI.Label(rect, maxdps.ToString("0.0"), labelStyle);
        }
        void dpshead(Rect rect)
        {
            rect.width *= 0.33f;
            rect.x += rect.width;
            GUI.Label(rect, "当前: ", labelStyle);
            rect.x += rect.width;
            GUI.Label(rect, "最大: ", labelStyle);
        }

        void TestBenchWindow(int id)
        {
            if (fold || PlayerAnimControl.instance == null)
            {
                if (GUI.Button(new Rect(5, 20, 190, 30), "展开")) fold = false;
                mwdnd.height = 60;
                
            }
            else
            {
                mwdnd.height = 400;
                if (GUI.Button(new Rect(5, mwdnd.height-35, 190, 30), "折叠")) fold = true;
                
                Rect r = new Rect(5, 20, 140, 30);
                if (GUI.Button(r, ((PN)potionid).ToString())) { 
                    if (mwdnd.x > Screen.width / 2)
                    {
                        psuiwnd.x = mwdnd.x - psuiwnd.width;
                    }
                    else
                    {
                        psuiwnd.x = mwdnd.xMax;
                    }
                    psuiwnd.y = mwdnd.y;
                    psuion = !psuion;
                    msuion = false;
                }
                r.Set(5, 50, 140, 30);
                potionlevel = GUI.Toolbar(r, potionlevel, new string[] { "白", "紫", "金" });
                r.Set(150, 20, 45, 60);
                if (GUI.Button(r, "->"))
                {
                    PotionDropPool.instance.Pop(potionid, potionlevel, PlayerAnimControl.instance.transform.position, true, true);
                }
                r.Set(5, 85, 140, 30);
                if (GUI.Button(r, ((MagicSwordName)msid).ToString()))
                {
                    if (mwdnd.x > Screen.width / 2)
                    {
                        msuiwnd.x = mwdnd.x - msuiwnd.width;
                    }
                    else
                    {
                        msuiwnd.x = mwdnd.xMax;
                    }
                    msuiwnd.y = mwdnd.y;
                    msuion = !msuion;
                    psuion = false;
                }
                r.Set(5, 115, 140, 30);
                mslevel = GUI.Toolbar(r, mslevel, new string[] { "白", "蓝", "金", "红" });
                r.Set(150, 85, 45, 60);
                if (GUI.Button(r, "->"))
                {
                    List<MagicSwordEntry> list = MagicSwordControl.instance.RandomEntrys((MagicSwordName)msid, mslevel);
                    MagicSwordPool.instance.Pop(msid, mslevel, list, PlayerAnimControl.instance.transform.position, true, true);
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
                redsoul = int.Parse(GUI.TextField(r, PlayerAnimControl.instance.RedSouls.ToString()));
                r.Set(150, 215, 45, 30);
                if (GUI.Button(r, "红魂")) { }//PlayerAnimControl.instance.RedSouls = redsoul;

                r.Set(5, 250, 190, 30);
                if (GUI.Button(r, "生成木桩 (X)") || Input.GetKeyDown(KeyCode.X))
                {
                    if (hasgen && sb != null)
                    {
                        UnityEngine.Object.Destroy(sb);
                    }
                    Vector3 v3 = PlayerAnimControl.instance.transform.position + new Vector3(0f, +2f, 0f);
                    //GameObject item = MonstersControl.instance.MonsterDynamicInstantiate(sbid, PlayerAnimControl.instance.transform, false);
                    GameObject item = UnityEngine.Object.Instantiate<GameObject>(MonstersControl.instance.EnemyObject[sbid], v3, Quaternion.identity); 
                    sb = item;
                    item.GetComponent<EnemyControl>().canBeMove = false;
                    item.GetComponent<EnemyControl>().isStun = true;
                    item.GetComponent<EnemyControl>().isNoGenBloodFleshMonster = true;
                    if (mwdnd.x > Screen.width / 2)
                    {
                        bfuiwnd.x = mwdnd.x - msuiwnd.width;
                    }
                    else
                    {
                        bfuiwnd.x = mwdnd.xMax;
                    }
                    bfuiwnd.y = mwdnd.y;

                    StartCoroutine(setinit());
                    hasgen = true;
                    afterdeath = false;
                }
                r.Set(5, 280, 190, 30);
                if (GUI.Button(r, "移除木桩 (C)") || Input.GetKeyDown(KeyCode.C))
                {
                    if (hasgen)
                    {
                        UnityEngine.Object.Destroy(sb);
                        sb = null;
                        hasgen = false;
                        afterdeath = false;

                        maxdpf = 0; maxdps025 = 0; maxdps1 = 0; maxdps5 = 0;
                        tempdps025 = 0; tempdps1 = 0; tempdps5 = 0; dpf = 0;
                        dps025 = 0; dps1 = 0; dps5 = 0;
                        dpstimer025 = 0; dpstimer1 = 0; dpstimer5 = 0;
                    }
                }
                r.Set(5, 315, 190, 30);
                if (GUI.Button(r, "清除小怪 (Z)") || Input.GetKeyDown(KeyCode.Z))
                {
                    KillAllPool.instance.Pop().transform.position = PlayerAnimControl.instance.transform.position;
                }
            }
            GUI.DragWindow();
        }

        void potionselectwindow(int id)
        {
            string[] s = new string[(int)PN.SpiritJade + 1];
            for (int i = 1; i <= (int)PN.SpiritJade; i++)
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
            potionid = GUILayout.SelectionGrid(potionid, s, 3);
            if (GUI.changed)
            {
                psuion = false;
            }
            GUI.DragWindow();
        }

        void sbselectwindow(int id)
        {
            sbid = (int)GUILayout.HorizontalSlider(sbid, 0, 86);
            GUILayout.Label(sbid.ToString());
        }
        
        void msselectwindow(int id)
        {
            string[] s = new string[(int)MagicSwordName.ShuangSheng + 1];
            for (int i = 1; i <= (int)MagicSwordName.ShuangSheng; i++)
            {
                MagicSword ms = new MagicSword();
                ms.magicSwordName = ((MagicSwordName)i);
                s[i] = TextControl.instance.MagicSwordInfo(ms)[0];
            }
            s[0] = "随机";
            msid = GUILayout.SelectionGrid(msid, s, 3);
            if (GUI.changed)
            {
                msuion = false;
            }
            GUI.DragWindow();
        }

        IEnumerator setinit()
        {
            yield return new WaitForSeconds(1f);

            sb.GetComponent<EnemyControl>().enemyParameter.MAX_HP = 16777215f;
            sb.GetComponent<EnemyControl>().enemyParameter.EXTRA_DEFENSE = 0f;
            yield return new WaitForSeconds(0.1f);
            maxdpf = 0;maxdps025 = 0; maxdps1 = 0; maxdps5 = 0;
            tempdps025 = 0;tempdps1 = 0;tempdps5 = 0;dpf = 0;
            dps025 = 0;dps1 = 0;dps5 = 0;
            dpstimer025 = 0;dpstimer1 = 0;dpstimer5 = 0;
            yield break;
        }

    }

}
