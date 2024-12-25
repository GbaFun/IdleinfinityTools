﻿using CefSharp;
using CefSharp.WinForms;
using IdleAuto.Logic.ViewModel;
using IdleAuto.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Logic.Serivce
{
    /// <summary>
    /// 页面载入过程中涉及的处理逻辑
    /// </summary>
    public class PageLoadService
    {
        public static string LoginPage = "Login";
        public static string HomePage = "Home/Index";

        #region 载入js
        public static void LoadJsByUrl(ChromiumWebBrowser browser)
        {
            var url = browser.Address;
            if (ContainsUrl(url, LoginPage))
            {
                var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "login.js");
                LoadJs(jsPath, browser);
            }
            if (ContainsUrl(url, HomePage))
            {
                var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "login.js");
                LoadJs(jsPath, browser);
            }

        }


        private static void LoadJs(string path, ChromiumWebBrowser bro)
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

        #endregion

        #region 载入替换cookie


        public static async void SaveCookieAndCache(ChromiumWebBrowser bro)
        {
            string stroagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", CurrentUser.User.Username + ".json");
            string cookiePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", CurrentUser.User.Username + ".txt");
            await DevToolUtil.SaveCookiesAsync(bro, cookiePath);
            await DevToolUtil.SaveLocalStorageAsync(bro, stroagePath);
        }

        public static async void LoadCookieAndCache(ChromiumWebBrowser bro)
        {
            string stroagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", CurrentUser.User.Username + ".json");
            string cookiePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", CurrentUser.User.Username + ".txt");
            if (File.Exists(cookiePath))
            {
                await DevToolUtil.LoadCookiesAsync(bro, cookiePath);
            }
            if (File.Exists(stroagePath))
            {
                await DevToolUtil.LoadLocalStorageAsync(bro, stroagePath);
            }


        }
        #endregion

        /// <summary>
        /// 检测url是否有指定key
        /// </summary>
        /// <param name="url"></param>
        /// <param name="keyPage"></param>
        /// <returns></returns>
        private static Boolean ContainsUrl(string url, string keyPage)
        {
            return url.IndexOf(keyPage) > -1;
        }
    }
}
