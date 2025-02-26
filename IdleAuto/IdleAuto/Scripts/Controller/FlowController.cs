﻿using IdleAuto.Scripts.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Controller
{
    /// <summary>
    /// 流程控制 写静态方法
    /// </summary>
    public class FlowController
    {
        public static async Task StartMapSwitch()
        {
            var user = AccountController.Instance.User;
            var window = await TabManager.Instance.TriggerAddBroToTap(user);
            await window.CharController.StartSwitchMap(window.GetBro(), window.User);
            window.Close();
        }

        /// <summary>
        /// 加点
        /// </summary>
        /// <returns></returns>
        public static async Task StartAddSkill()
        {
            for (int i = 0; i < AccountCfg.Instance.Accounts.Count; i++)
            {
                var account = AccountCfg.Instance.Accounts[i];
                if (account.AccountName == "铁矿石") continue;
                var user = new UserModel(account);
                var window = await TabManager.Instance.TriggerAddBroToTap(user);
                await window.CharController.StartAddSkill(window.GetBro(), window.User);
                window.Close();
            }


        }

        public static async Task RefreshCookie()
        {
            BroTabManager.Instance.ClearBrowsers();
            foreach (var account in AccountCfg.Instance.Accounts)
            {
                AccountController.Instance.User = new UserModel(account);
                var window = await TabManager.Instance.TriggerAddBroToTap(AccountController.Instance.User);
                await Task.Delay(2000);
                window.Close();
            }

        }
    }
}
