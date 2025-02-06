﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MapSetting
{
    /// <summary>
    /// 人物等级>=这个值则跳到Map
    /// </summary>
    public int Lv { get; set; }

    public int MapLv { get; set; }

    public bool CanSwitch(int roleLv, int curMapLv)
    {
        if (roleLv >= Lv && curMapLv != MapLv) return true;
        else return false;
    }
}
public class MapSettingCfg
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "MapSetting.json");
    public static MapSettingCfg Instance { get; } = new MapSettingCfg();

    public List<MapSetting> Data { get; set; }

    public MapSettingCfg()
    {
        LoadConfig();
    }

    public void LoadConfig()
    {
        if (File.Exists(ConfigFilePath))
        {
            var json = File.ReadAllText(ConfigFilePath);
            Data = json.ToUpperCamelCase<List<MapSetting>>();
        }
        else
        {
            Data = new List<MapSetting>();
        }
    }

    public MapSetting GetSetting(int roleLv)
    {
        var last = Data.Where(p => roleLv >= p.Lv).LastOrDefault();
        if (last == null) P.Log("未配置该级别切图数据"); //throw new Exception("未配置该级别切图数据");
        return last;
    }
}


