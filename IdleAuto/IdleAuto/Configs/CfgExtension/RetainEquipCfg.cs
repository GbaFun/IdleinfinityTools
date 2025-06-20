﻿using AttributeMatch;
using IdleAuto.Configs.CfgExtension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class RetainEquipCfg
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "RetainEquipCfg.json");
    public static RetainEquipCfg Instance { get; } = new RetainEquipCfg();

    public List<RetainEquip> Equips { get; set; }

    public RetainEquipCfg()
    {
        LoadConfig();
    }

    public void ResetCount()
    {
        foreach (var item in Equips)
        {
            item.ResetCount();
        }
    }
    public void LoadConfig()
    {
        if (File.Exists(ConfigFilePath))
        {
            var json = File.ReadAllText(ConfigFilePath);
            Equips = JsonConvert.DeserializeObject<List<RetainEquip>>(json);
        }
        else
        {
            Equips = new List<RetainEquip>();
        }
    }

    public bool IsRetain(EquipModel equip)
    {
        var configs = Equips.Where(p=>(p.Equip.Category.Split('|').Contains(equip.Category)||p.Equip.Category=="全部")&&(p.Equip.Quality.Split('|').Contains(equip.emItemQuality.ToString()) || p.Equip.Quality == "全部")).ToList();
    
        foreach (var item in configs)
        {
            if (equip.EquipName == "混沌碎片")
            {
                P.Log("测试混沌");
            }
            if (AttributeMatchUtil.Match(equip, item.Equip, out var report) && item.AddCount())
            {
                return true;
            }
        }

        return false;
    }
}

public class RetainEquip
{
    /// <summary>
    /// 配置装备
    /// </summary>
    [JsonProperty("equip")]
    public Equipment Equip { get; set; }

    /// <summary>
    /// 保留最大数量
    /// </summary>
    [JsonProperty("maxCount")]
    public int MaxCount { get; set; }

    public int CurCount { get; private set; }
    public bool AddCount()
    {
        if (CurCount < MaxCount)
        {
            CurCount++;
            return true;
        }
        return false;
    }
    public void ResetCount()
    {
        CurCount = 0;
    }
}
