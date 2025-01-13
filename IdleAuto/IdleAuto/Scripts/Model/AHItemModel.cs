﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AHItemModel
{
    /// <summary>
    /// 装备id
    /// </summary>
    public int eid { get; set; }

    /// <summary>
    /// 逻辑价格 通过一套衡量算法给出一个价值方便扫货
    /// </summary>
    public decimal logicPrice { get; set; }

    /// <summary>
    /// 装备名称
    /// </summary>
    public string eTitle { get; set; }

    /// <summary>
    /// 装备文字描述 用于匹配词缀
    /// </summary>
    public string content { get; set; }

    public int lv { get; set; }

    public int goldCoinPrice { get; set; }
    /// <summary>
    /// 符文价格 多个符文的情况所以是数组
    /// </summary>
    public int[] runePriceArr { get; set; }

    public int[] runeCountArr { get; set; }

    public bool canBuy { get; set; }

    public string ToPriceStr()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < runePriceArr.Length; i++)
        {
            sb.Append(runePriceArr[i].ToString());
            sb.Append("*");
            sb.Append(runeCountArr[i]);
            sb.Append(" ");
        }
        return sb.ToString();
    }

}
