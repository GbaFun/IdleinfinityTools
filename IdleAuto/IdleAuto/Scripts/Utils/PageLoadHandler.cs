﻿using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/// <summary>
/// 页面载入过程中涉及的处理逻辑
/// </summary>
public class PageLoadHandler
{
    public const string LoginPage = "Login";
    public const string HomePage = "Home/Index";
    public const string MaterialPage = "Equipment/Material";
    public const string AhPage = "Auction/Query";

    #region 载入js
    public static async Task LoadJsByUrl(ChromiumWebBrowser browser)
    {
        var url = browser.Address;
        //全局js
        await LoadGlobalJs(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "IdleUtils.js"), browser);
        await LoadGlobalJs(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "char.js"), browser);
        if (ContainsUrl(url, LoginPage))
        {
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "login.js");
            await LoadJs(jsPath, browser);
        }
        if (ContainsUrl(url, HomePage))
        {
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "login.js");
            await LoadJs(jsPath, browser);
        }
        if (ContainsUrl(url, AhPage))
        {
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "ah.js");
            await LoadJs(jsPath, browser);
        }
        if (ContainsUrl(url, MaterialPage))
        {
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "rune.js");
            await LoadGlobalJs(jsPath, browser);
        }


    }


    private static async Task LoadJs(string path, ChromiumWebBrowser bro)
    {
        // 在主框架中执行自定义脚本
        // 获取WinForms程序目录下的JavaScript文件路径

        string scriptContent = File.ReadAllText(path);

        // 在主框架中执行自定义脚本
        string script = $@"
                    (function() {{
                        var script = document.createElement('script');
                        script.type = 'text/javascript';
                        script.text = {scriptContent};
                        document.head.appendChild(script);
                    }})();
                ";

        bro.ExecuteScriptAsync(script);
    }
    private static async Task LoadGlobalJs(string path, ChromiumWebBrowser bro)
    {
        // 在主框架中执行自定义脚本
        // 获取WinForms程序目录下的JavaScript文件路径

        string scriptContent = File.ReadAllText(path);

        // 在主框架中执行自定义脚本
        string script = $@"
                    {scriptContent}
                ";

        bro.ExecuteScriptAsync(script);
    }


    #endregion

    #region 载入替换cookie


    public static async void SaveCookieAndCache(ChromiumWebBrowser bro, bool isDirectUpdate = false)
    {
        string stroagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", AccountController.User.Username + ".json");
        string cookiePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", AccountController.User.Username + ".txt");
        var createTime = File.GetCreationTime(cookiePath);
        TimeSpan val = DateTime.Now - createTime;
        if (val.Minutes >= 10 || isDirectUpdate)
        {
            await DevToolUtil.SaveCookiesAsync(bro, cookiePath);
            await DevToolUtil.SaveLocalStorageAsync(bro, stroagePath);
        }

    }

    public static async Task LoadCookieAndCache(ChromiumWebBrowser bro)
    {

        string stroagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", AccountController.User.Username + ".json");
        string cookiePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", AccountController.User.Username + ".txt");

        if (File.Exists(cookiePath))
        {
            if (ValidCookie(cookiePath))
            {
                await DevToolUtil.ClearCookiesAsync(bro);
                await DevToolUtil.LoadCookiesAsync(bro, cookiePath);
                bro.LoadUrl("https://www.idleinfinity.cn/Home/Index");
            }

        }

        if (File.Exists(stroagePath))
        {

            await DevToolUtil.ClearLocalStorageAsync(bro);
            await DevToolUtil.LoadLocalStorageAsync(bro, stroagePath);
        }

    }
    /// <summary>
    /// 检查cookie是否在有效期
    /// </summary>
    /// <param name="cookiePath"></param>
    /// <returns></returns>
    public static bool ValidCookie(string cookiePath)
    {
        if (!File.Exists(cookiePath))
        {
            return false;
        }
        var cookieList = new List<CefSharp.Cookie>();
        DateTime createdTime = File.GetCreationTime(cookiePath);
        using (var reader = new StreamReader(cookiePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split('\t');
                var cookie = new CefSharp.Cookie
                {
                    Domain = parts[0],
                    Name = parts[1],
                    Value = parts[2],
                    Path = parts[3],
                    Expires = parts[4] == "" ? DateTime.Now.AddDays(1) : DateTime.Parse(parts[4])
                };
                cookieList.Add(cookie);

            }
        }
        if (cookieList.Count == 0) return false;
        var idleCookie = cookieList.Where(p => p.Name == "idleinfinity.cookies").FirstOrDefault();
        if (idleCookie == null) return false;
        if (DateTime.Now.AddMinutes(-60) >= idleCookie.Expires) return false;
        return true;
    }
    #endregion

    /// <summary>
    /// 检测url是否有指定key
    /// </summary>
    /// <param name="url"></param>
    /// <param name="keyPage"></param>
    /// <returns></returns>
    public static Boolean ContainsUrl(string url, string keyPage)
    {
        return url.IndexOf(keyPage) > -1;
    }
}
