﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum emEventType
{
    OnBrowserFrameLoadStart,
    OnBrowserFrameLoadEnd,
    /// <summary>
    /// js初始化完成
    /// </summary>
    OnJsInited,
    /// <summary>
    /// 登录成功
    /// </summary>
    OnLoginSuccess,
    /// <summary>
    /// 登录账户变化
    /// </summary>
    OnAccountDirty,
    /// <summary>
    /// 升级符文返回
    /// </summary>
    OnUpgradeRuneBack,
    /// <summary>
    /// 角色载入
    /// </summary>
    OnCharLoaded,
    /// <summary>
    /// 角色名冲突
    /// </summary>
    OnCharNameConflict,

    /// <summary>
    /// 地图切换需要秘境
    /// </summary>
    OnDungeonRequired,

    /// <summary>
    /// 检查账号
    /// </summary>
    OnAccountCheck,
    /// <summary>
    /// jsPost消息失败
    /// </summary>
    OnPostFailed,

    /// <summary>
    /// 监听信号量
    /// </summary>
    OnSignal,

    /// <summary>
    /// san值不够
    /// </summary>
    OnSanError

}

public class EventSystem
{


    public EventSystem()
    {
        eventDic = new Dictionary<emEventType, Action<object[]>>();
    }
    public void Dispose()
    {
        eventDic = null;
    }

    private Dictionary<emEventType, Action<object[]>> eventDic;

    public void SubscribeEvent(emEventType eventType, Action<object[]> action)
    {
        if (eventDic.ContainsKey(eventType))
        {
            eventDic[eventType] += action;
        }
        else
        {
            eventDic.Add(eventType, action);
        }
    }
    public void UnsubscribeEvent(emEventType eventType, Action<object[]> action)
    {
        if (eventDic.ContainsKey(eventType))
        {
            eventDic[eventType] -= action;
        }
    }
    public void InvokeEvent(emEventType eventType, params object[] args)
    {
        if (eventDic.ContainsKey(eventType))
        {
            eventDic[eventType]?.Invoke(args);
        }
    }
}
