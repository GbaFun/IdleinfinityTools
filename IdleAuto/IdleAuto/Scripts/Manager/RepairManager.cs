﻿using AttributeMatch;
using CefSharp;
using CefSharp.DevTools.FedCm;
using IdleAuto.Db;
using IdleAuto.Scripts.Controller;
using IdleAuto.Scripts.Model;
using IdleAuto.Scripts.Wrap;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

public class RepairManager : SingleManagerBase<RepairManager>
{
    public static object _lock = new object();

    public static string[] NanfangAccounts = ConfigUtil.GetAppSetting("南方账号").Split(',');
    public static string[] NainiuAccounts = ConfigUtil.GetAppSetting("奶牛账号").Split(',');
    public static readonly List<int> FcrSpeeds = new List<int> { 0, 25, 50, 75, 110, 145, 180 };
    public static long PublicFeilongId = long.Parse(ConfigUtil.GetAppSetting("feilong"));
    public static long PublicYonghengId = long.Parse(ConfigUtil.GetAppSetting("yongheng"));
    public static readonly string RepoAcc = ConfigUtil.GetAppSetting("repoAcc");
    public static readonly string RepoRole = ConfigUtil.GetAppSetting("repoRole");

    public static string RepairJob = ConfigUtil.GetAppSetting("RepairJob");

    public static bool IsCollectEquip = bool.Parse(ConfigUtil.GetAppSetting("isCollectEquip"));
    /// <summary>
    /// 一键修车（单账号）
    /// 盘库、技能加点、自动更换装备、剩余属性点分配
    /// 修车过程不会将背包物品收拢，不会清仓
    /// </summary>
    public async Task AutoRepair(UserModel account)
    {
        var repairJob = ConfigUtil.GetAppSetting("RepairJob");
        //大号跳过自动修车逻辑，需要手动修车
        if (account.AccountName == "铁矿石")
        {
            MessageBox.Show($"大号跳过自动修车逻辑，请手动修！");
            return;
        }

        BroWindow window = await TabManager.Instance.TriggerAddBroToTap(account);
        EquipController equipController = new EquipController(window);
        //window.GetBro().ShowDevTools();

        List<string> InterruptNames = new List<string>();
        //遍历账户下角色修车
        foreach (var role in account.Roles)
        {

            try
            {
                lock (_lock)
                {
                    var roleProgress = FreeDb.Sqlite.Select<TaskProgress>().Where(p => p.Type == emTaskType.AutoEquip && p.UserName == account.AccountName && p.Roleid == role.RoleId).ToList();
                    if (roleProgress != null && roleProgress.Count == 1 && roleProgress[0].IsEnd)
                        continue;
                }
                if (repairJob != "" && role.Job.ToString() != repairJob) continue;
                //如果当前角色的记录是已经完成修车状态，则本次修车跳过该角色
                //自动更换装备
                var curEquips = await equipController.AutoEquips(window, role);
                //技能加点
                await AddSkillPoint(window, role, curEquips);

                //角色剩余属性点分配
                await AddAttrPoint(window, role);
                TaskProgress progress = new TaskProgress()
                {
                    Roleid = role.RoleId,
                    UserName = account.AccountName,
                    Type = emTaskType.AutoEquip,
                    IsEnd = true
                };
                lock (_lock)
                {
                    var one = FreeDb.Sqlite.Select<TaskProgress>().Where(p => p.Type == emTaskType.AutoEquip && p.UserName == account.AccountName && p.Roleid == role.RoleId).First();
                    if (one != null)
                        progress.Id = one.Id;
                }

                FreeDb.Sqlite.InsertOrUpdate<TaskProgress>().SetSource(progress).ExecuteAffrows();
            }
            catch (Exception ex)
            {
                P.Log($"自动修车中断，中断角色{role.RoleName},中断原因：{ex}", emLogType.Error);
                InterruptNames.Add(role.RoleName);
            }
        }
        window.Close();
        if (InterruptNames.Count <= 0)
        {
            FreeDb.Sqlite.Delete<TaskProgress>().Where(p => p.Type == emTaskType.AutoEquip && p.UserName == account.AccountName).ExecuteAffrows();
        }
        else
        {
            //MessageBox.Show($"自动修车完成,但部分角色修车进程意外中断，中断角色列表：{string.Join("-", InterruptNames.ToArray())}");
        }
    }

    public async Task AutoRepair(BroWindow window)
    {

        EquipController equipController = new EquipController(window);
        //window.GetBro().ShowDevTools();
        var account = window.User;
        //将挂机装备放入仓库
        //await EquipToRepository(window, equipController, account, true);
        //盘点仓库装备
        //await InventoryEquips(window, equipController, account);
        List<string> InterruptNames = new List<string>();
        //遍历账户下角色修车
        foreach (var role in account.Roles)
        {
            var repairJob = RepairManager.RepairJob;
            if (repairJob != "" && role.Job.ToString() != repairJob) continue;
            try
            {
                //如果当前角色的记录是已经完成修车状态，则本次修车跳过该角色
                var roleProgress = FreeDb.Sqlite.Select<TaskProgress>().Where(p => p.Type == emTaskType.AutoEquip && p.UserName == account.AccountName && p.Roleid == role.RoleId).ToList();
                if (roleProgress != null && roleProgress.Count == 1 && roleProgress[0].IsEnd)
                    continue;
                //自动更换装备
                var curEquips = await equipController.AutoEquips(window, role);

                //技能加点
                await AddSkillPoint(window, role, curEquips); ;

                //角色剩余属性点分配
                if (role.Job == emJob.死灵) await AddAttrPoint(window, role);
                TaskProgress progress = new TaskProgress()
                {
                    Roleid = role.RoleId,
                    UserName = account.AccountName,
                    Type = emTaskType.AutoEquip,
                    IsEnd = true
                };
                var one = FreeDb.Sqlite.Select<TaskProgress>().Where(p => p.Type == emTaskType.AutoEquip && p.UserName == account.AccountName && p.Roleid == role.RoleId).First();
                if (one != null)
                    progress.Id = one.Id;
                FreeDb.Sqlite.InsertOrUpdate<TaskProgress>().SetSource(progress).ExecuteAffrows();
            }
            catch (Exception ex)
            {
                P.Log($"自动修车中断，中断角色{role.RoleName},中断原因：{ex.Message}", emLogType.Error);
                InterruptNames.Add(role.RoleName);
            }
        }
        if (InterruptNames.Count <= 0)
        {
            FreeDb.Sqlite.Delete<TaskProgress>().Where(p => p.Type == emTaskType.AutoEquip && p.UserName == account.AccountName).ExecuteAffrows();
            //MessageBox.Show($"自动修车完成");
        }
        else
        {
            // MessageBox.Show($"自动修车完成,但部分角色修车进程意外中断，中断角色列表：{string.Join("-", InterruptNames.ToArray())}");
        }
    }

    /// <summary>
    /// 一键清仓
    /// 清仓
    /// </summary>
    public async Task ClearEquips(UserModel account)
    {
        try
        {
            BroWindow window = await TabManager.Instance.TriggerAddBroToTap(account);
            EquipController equipController = new EquipController(window);
            //window.GetBro().ShowDevTools();

            //清理仓库
            await equipController.ClearRepository(window);

            window.Close();
            MessageBox.Show($"一键清仓完成");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"一键清仓异常：{ex.Message}");
        }
    }

    /// <summary>
    /// 一键清仓
    /// 清仓
    /// </summary>
    public async Task ClearEquips(BroWindow win)
    {
        try
        {
            EquipController equipController = new EquipController(win);
            //window.GetBro().ShowDevTools();
            //清理仓库
            await equipController.ClearRepository(win);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"一键清仓异常：{ex.Message}");
        }
    }/// <summary>
     /// 一键收菜
     /// 收菜、清仓、盘库
     /// </summary>
    public async Task UpdateEquips(UserModel account)
    {
        try
        {
            BroWindow window = await TabManager.Instance.TriggerAddBroToTap(account);
            EquipController equipController = new EquipController(window);
            //window.GetBro().ShowDevTools();

            if (account.AccountName != "铁矿石")
            {
                //将挂机装备放入仓库
                await EquipToRepository(window, equipController, account, true);
            }
            //盘点仓库装备
            await InventoryEquips(window, equipController, account);

            window.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"一键收菜异常：{ex.Message}");
        }
    }

    public async Task UpdateEquips(BroWindow win)
    {
        bool isCollect = RepairManager.IsCollectEquip;
        var equipController = new EquipController(win);
        var account = win.User;
        if (isCollect)
        {
            //将挂机装备放入仓库
            await EquipToRepository(win, equipController, account, true);
        }
        //盘点仓库装备
        await InventoryEquips(win, equipController, account);
    }



    public async Task EquipToRepository(BroWindow win, EquipController controller, UserModel account, bool cleanWhenFull = false)
    {
        await controller.EquipsToRepository(win, account, cleanWhenFull);
    }
    public async Task InventoryEquips(BroWindow win, EquipController controller, UserModel account)
    {
        await controller.InventoryEquips(win, account, true);
    }
    public async Task AddSkillPoint(BroWindow win, RoleModel role, Dictionary<emEquipSort, EquipModel> curEquips)
    {
        var charControl = new CharacterController(win);
        await charControl.AddSkillPoints(role, curEquips);
    }

    public async Task AddAttrPoint(BroWindow win, RoleModel role)
    {
        await Task.Delay(1000);
        P.Log($"开始跳转{role.RoleName}的角色详情页");
        var result = await win.LoadUrlWaitJsInit(IdleUrlHelper.RoleUrl(role.RoleId), "char");
        if (result.Success)
        {
            P.Log($"跳转{role.RoleName}的角色详情页完成");

            var response3 = await win.CallJs($@"_char.getSimpleAttribute();");
            if (response3.Success)
            {
                var baseAttr = response3.Result.ToObject<CharBaseAttributeModel>();
                ///满足装备需求属性后仍有属性点剩余，自动加到体力值
                if (baseAttr.Point > 0)
                {
                    baseAttr.VitAdd += baseAttr.Point;
                    baseAttr.Point = 0;
                    await Task.Delay(1000);
                    var response8 = await win.CallJsWaitReload($@"_char.attributeSave({role.RoleId},{baseAttr.ToLowerCamelCase()});", "char");
                    if (response8.Success)
                    {
                        P.Log($"{role.RoleName}的属性加点完成", emLogType.AutoEquip);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 任一role取出整队
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public static List<GroupModel> GetGroup(RoleModel role)
    {
        var g1 = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.RoleId == role.RoleId).First();
        if (g1 == null)
        {
            throw new Exception("请先初始化组队信息");
        }
        var accName = g1.AccountName;
        return FreeDb.Sqlite.Select<GroupModel>().Where(p => p.AccountName == accName && p.TeamIndex == g1.TeamIndex).ToList();
    }
}