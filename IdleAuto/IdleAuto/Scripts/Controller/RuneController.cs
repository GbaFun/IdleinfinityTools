﻿using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FreeSql;
using FreeSql.Sqlite;

public class RuneController
{
    private EventSystem EventSystem;
    private static RuneController instance;
    public static RuneController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new RuneController();
            }
            return instance;
        }
    }

    private delegate void OnUpgradeRuneBack(bool result);
    private OnUpgradeRuneBack onUpgradeRuneCallBack;
    private delegate void OnJsInitCallBack(bool result);
    private OnJsInitCallBack onJsInitCallBack;

    public async void AutoUpgradeRune()
    {
        //Console.WriteLine($"{DateTime.Now}---开始一键升级符文");
        long start = DateTime.Now.Ticks;
        P.Log("开始一键升级符文", emLogType.RuneUpgrate);
        EventSystem.SubscribeEvent(emEventType.OnUpgradeRuneBack, OnEventUpgradeRuneBack);
        EventSystem.SubscribeEvent(emEventType.OnJsInited, OnRuneJsInited);
        //MainForm.Instance.browser.FrameLoadEnd += OnMainFormBrowseFrameLoad;
        List<RuneCompandData> cfg = RuneCompandCfg.Instance.RuneCompandData;

        int broIndex = BroTabManager.Instance.GetFocusID();

        foreach (var item in cfg)
        {
            P.Log($"开始检查{item.ID}#符文", emLogType.RuneUpgrate);
            long duration = (DateTime.Now.Ticks - start) / 10000;
            //如果配置保留数量为-1，则不处理
            if (item.CompandNum == -1)
            {
                P.Log($"{item.ID}#符文配置保留数量为无限，无需升级", emLogType.RuneUpgrate);
                await Task.Delay(500);
                continue;
            }
            var response = await BroTabManager.Instance.TriggerCallJs(broIndex, $@"getRuneNum({item.ID})");
            //GetRuneNum(item.ID);
            if (response.Success)
            {
                int curNum = (int)response.Result;
                if (curNum >= item.CompandNum)
                {
                    int count = curNum - item.CompandNum;
                    count = count - count % 2;
                    if (count < 2)
                    {
                        P.Log($"{item.ID}#符文空余数量不足2，无需升级", emLogType.RuneUpgrate);
                        await Task.Delay(500);
                        continue;
                    }
                    P.Log($"开始升级{item.ID}#符文，升级数量{count}", emLogType.RuneUpgrate);
                    var response2 = await BroTabManager.Instance.TriggerCallJs(broIndex, $@"upgradeRune({item.ID},{count})");
                    //UpgradeRune(item.ID, count);
                    //var tcs = new TaskCompletionSource<bool>();
                    //onUpgradeRuneCallBack = (result) => tcs.SetResult(result);
                    //await tcs.Task;
                    //var tcs2 = new TaskCompletionSource<bool>();
                    //onJsInitCallBack = (result) => tcs2.SetResult(result);
                    //await tcs2.Task;

                    if (response2.Success == false)
                    {
                        MessageBox.Show($"自动升级符文失败，详情请查看log文件({Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", emLogType.RuneUpgrate.ToString())})");
                        break;
                    }
                }
            }
        }

        P.Log($"全部符文升级完成\n\t\n\t\n\t\n\t\n\t", emLogType.RuneUpgrate);
        EventSystem.UnsubscribeEvent(emEventType.OnUpgradeRuneBack, OnEventUpgradeRuneBack);
        EventSystem.UnsubscribeEvent(emEventType.OnJsInited, OnRuneJsInited);
    }

    private void OnEventUpgradeRuneBack(params object[] args)
    {
        bool isSuccess = (bool)args[0];
        int runeId = (int)args[1];
        int runeNum = (int)args[2];

        if (!isSuccess)
        {
            string msg = args[3] as string;
            P.Log($"{runeId}#符文升级失败，升级数量{runeNum}，失败原因{msg}", emLogType.RuneUpgrate);
        }
        else
        {
            P.Log($"{runeId}#符文升级成功，升级数量{runeNum}", emLogType.RuneUpgrate);
        }

        onUpgradeRuneCallBack?.Invoke(true);
        onUpgradeRuneCallBack = null;
    }

    private void OnRuneJsInited(params object[] args)
    {
        string jsName = args[0] as string;
        if (jsName == "rune")
        {
            onJsInitCallBack?.Invoke(true);
            onJsInitCallBack = null;
        }
    }
}

