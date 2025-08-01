﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AttributeMatch;
using FreeSql.DataAnnotations;
using IdleAuto.Scripts.Model;
using IdleAuto.Scripts.Utils;

public class EquipModel : IModel
{

    /// <summary>
    /// 装备唯一ID
    /// </summary>
    [JsonProperty("eid")]
    [Column(IsPrimary = true)]
    public long EquipID { get; set; }
    /// <summary>
    /// 装备栏位,非装备类物品忽略
    /// </summary>
    [JsonProperty("esort")]
    public emEquipSort emEquipSort { get; set; }
    /// <summary>
    /// 物品品质
    /// </summary>
    [JsonProperty("quality")]
    public string Quality { get; set; }
    [JsonIgnore]
    public emItemQuality emItemQuality
    {
        get
        {
            return Quality.ToEnumQuality();
        }
    }

    /// <summary>
    /// 网页上装备所有属性的文本内容
    /// </summary>
    private string _content; // 私有字段存储实际值
    public string Content
    {

        get
        {
            if (_content == null) return null;

            string[] result = Regex.Split(_content, "玩家锐评");
            return result[0];
        } // 假设取分割后的第一部分
        set => _content = value;
    }


    /// <summary>
    /// 物品类型
    /// </summary>
    public emItemType emEquipType { get; set; }

    /// <summary>
    /// 装备基础类型名
    /// </summary>
    public string EquipBaseName { get; set; }

    private string _equipName;
    /// <summary>
    /// 装备名
    /// </summary>
    public string EquipName
    {
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                this._equipName = value.Replace("★", "");
            }
        }
        get
        {
            return _equipName;
        }
    }
    //物等
    public int Lv { get; set; }

    /// <summary>
    /// 底子
    /// </summary>
    public emArtifactBase ArtifactBase { get; set; }



    public string Category
    {
        get
        {
            if (this == null || string.IsNullOrWhiteSpace(this.EquipBaseName)) return null;
            return CategoryUtil.GetCategory(this.EquipBaseName);
        }

        set
        {
            return;
        }
    }

    /// <summary>
    /// 是否是太古
    /// </summary> 
    public bool IsPerfect { get; set; }

    /// <summary>
    /// 是否是绑定
    /// </summary>
    public bool IsLocal { get; set; }


    /// <summary>
    /// 装备状态
    /// </summary>
    public emEquipStatus EquipStatus { get; set; }





    private AttrV4 m_requareAttr;
    public AttrV4 RequareAttr
    {
        get
        {
            if (m_requareAttr != AttrV4.Default)
            {
                return m_requareAttr;
            }
            int str = 0, dex = 0, vit = 0, eng = 0;
            Regex regex = new Regex(@"需要力量：\n( *)(?<str>\d+)\n");
            var match = regex.Match(Content);
            if (match.Success)
                str = int.Parse(match.Groups["str"].Value);
            regex = new Regex(@"需要敏捷：\n( *)(?<dex>\d+)\n");
            match = regex.Match(Content);
            if (match.Success)
                dex = int.Parse(match.Groups["dex"].Value);
            regex = new Regex(@"需要体力：\n( *)(?<vit>\d+)\n");
            match = regex.Match(Content);
            if (match.Success)
                vit = int.Parse(match.Groups["vit"].Value);
            regex = new Regex(@"需要精神：\n( *)(?<eng>\d+)\n");
            match = regex.Match(Content);
            if (match.Success)
                eng = int.Parse(match.Groups["eng"].Value);

            return new AttrV4(str, dex, vit, eng);
        }
    }

    public bool IsMatch(Equipment con)
    {
        return AttributeMatchUtil.Match(this, con, out _);
    }

 

    public string AccountName { get; set; }
    /// <summary>
    /// 装备所属角色ID,可以为空
    /// </summary>

    public int RoleID { get; set; }

    public string RoleName { get; set; }

    public void SetAccountInfo(UserModel account, RoleModel role = null)
    {
        AccountName = account.AccountName;
        if (role != null)
        {
            RoleID = role.RoleId;
            RoleName = role.RoleName;
        }
        else
        {
            RoleID = 0;
            RoleName = "";
        }
    }

    public void SetEquipStatus(emEquipStatus status)
    {
        this.EquipStatus = status;
        DbUtil.InsertOrUpdate<EquipModel>(this);
    }

    public bool CanWear(RoleModel role)
    {
        int needLv = 0;
        bool 无形 = EquipName.Contains("无形");
        bool 无法破坏 = Content.Contains("无法破坏");
        if (无形 && !无法破坏) return false;
        Regex regex = new Regex(@"需要等级：\n( *)(?<lv>\d+)\n");
        var match = regex.Match(Content);
        if (match.Success)
            needLv = int.Parse(match.Groups["lv"].Value);
        return role.Level >= needLv;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        var t = obj as EquipModel;
        if (t == null) return false;
        return t.EquipID == this.EquipID;
    }

    public override int GetHashCode()
    {
        return EquipID.GetHashCode();
    }
}

public struct AttrV4
{
    public int Str;
    public int Dex;
    public int Vit;
    public int Eng;

    public AttrV4(int str, int dex, int vit, int eng)
    {
        Str = str;
        Dex = dex;
        Vit = vit;
        Eng = eng;
    }

    public static AttrV4 Max(AttrV4 a, AttrV4 b)
        => new AttrV4(Math.Max(a.Str, b.Str), Math.Max(a.Dex, b.Dex), Math.Max(a.Vit, b.Vit), Math.Max(a.Eng, b.Eng));

    public static AttrV4 Default => new AttrV4(0, 0, 0, 0);
    public static AttrV4 operator +(AttrV4 a, AttrV4 b)
        => new AttrV4(a.Str + b.Str, a.Dex + b.Dex, a.Vit + b.Vit, a.Eng + b.Eng);
    public static AttrV4 operator -(AttrV4 a, AttrV4 b)
        => new AttrV4(a.Str - b.Str, a.Dex - b.Dex, a.Vit - b.Vit, a.Eng - b.Eng);
    public static bool operator ==(AttrV4 a, AttrV4 b)
        => a.Str == b.Str && a.Dex == b.Dex && a.Vit == b.Vit && a.Eng == b.Eng;
    public static bool operator !=(AttrV4 a, AttrV4 b)
        => a.Str != b.Str && a.Dex != b.Dex && a.Vit != b.Vit && a.Eng != b.Eng;
}
