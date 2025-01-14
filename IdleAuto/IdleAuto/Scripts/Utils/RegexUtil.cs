﻿using IdleAuto.Configs.CfgExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


/// <summary>
/// 根据配置来组装正则进行匹配
/// </summary>
public class RegexUtil
{
    public delegate bool ComparisonOperator<T>(T left, T right);
    private static readonly Dictionary<string, ComparisonOperator<Decimal>> DecimalOperatorDic = new Dictionary<string, ComparisonOperator<Decimal>>
    {
        { ">", (left, right) => left > right },
        { "<", (left, right) => left < right },
        { "==", (left, right) => left == right },
        { ">=", (left, right) => left >= right },
        { "<=", (left, right) => left <= right }
    };
    /// <summary>
    /// 匹配类型-数值比较
    /// </summary>
    public const string compareNum = "compareNum";
    public static bool Match(string content, List<RegexMatch> regList)
    {
        foreach (var item in regList)
        {
            if (item.type == compareNum)
            {
                var r = CompareNum(content, item);
                if (!r) return r;
            }
        }
        return true;
    }
    /// <summary>
    /// 数值比较
    /// </summary>
    /// <returns></returns>
    private static bool CompareNum(string content, RegexMatch regCfg)
    {
        var keywords = regCfg.keywords.Split(',');
        var match = Regex.Match(content, $@"{keywords[0]}.*?(\d+)%.*{keywords[1]}");
        //不匹配直接退出
        if (!match.Success)
        {
            return false;
        }
        //命中词条再比较数值
        var num = Decimal.Parse(match.Groups[1].Value);
        ComparisonOperator<decimal> comparison = GetDecimalOperator(regCfg.op);
        return comparison(num, decimal.Parse(regCfg.val));
    }




    private static ComparisonOperator<Decimal> GetDecimalOperator(string op)
    {
        if (DecimalOperatorDic.TryGetValue(op, out var comparisonOperator))
        {
            return comparisonOperator;
        }
        throw new ArgumentException($"你配的什么玩意: {op}");
    }


}
