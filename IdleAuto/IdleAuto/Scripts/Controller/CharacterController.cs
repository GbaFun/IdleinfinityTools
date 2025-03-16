﻿using CefSharp;
using IdleAuto.Db;
using IdleAuto.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql;
using CefSharp.WinForms;
using System.Data;
using IdleAuto.Scripts.Controller;
using IdleAuto.Scripts.Wrap;

namespace IdleAuto.Scripts.Controller
{
    public class CharacterController : BaseController
    {
        static string[] Names = new string[]{
        "草泥马", "苦力怕", "烈焰人", "远古守卫", "恶魂", "幻翼", "史莱姆", "唤魔者", "凋零", "女巫", "掠夺者", "猪灵",
        "钻石", "煤矿石", "铁矿石", "铜矿石", "青金石", "金矿石", "黑曜石", "绿宝石", "石英石", "萤石", "下届合金", "紫水晶",
        "基岩", "圆石", "安山岩", "闪长岩", "花岗岩", "深板岩", "砂岩", "凝灰岩", "下届岩", "菌岩", "玄武岩", "灵魂沙",
        "时运", "无限火矢", "节肢杀手", "精准采集", "水下呼吸", "亡灵杀手", "水下速掘", "冰霜行者", "海之眷顾", "饵钓", "经验修补", "横扫之刃",
        "要塞", "废弃矿道", "沙漠神殿", "远古城市", "丛林神庙", "林地府邸", "雪屋", "海底神殿", "堡垒遗迹", "林中小屋", "掠夺前哨战", "末地城",
        "竹林", "暖水海洋", "蘑菇岛", "樱花树林", "风袭丘陵", "诡异森林", "冰刺之地", "风蚀恶地", "溶洞", "繁茂洞穴", "深暗之域", "绯红森林",
        "傻子", "农民", "渔夫", "牧羊人", "制箭师", "武器匠", "图书管理员", "皮匠", "屠夫", "石匠", "制图师", "盔甲匠",
        "工作台", "堆肥桶", "木桶", "织布机", "制箭台", "砂轮", "讲台", "炼药锅", "烟熏炉", "切石机", "制图台", "高炉",
        "酿造台", "治疗药水", "再生药水", "水肺药水", "虚弱药水", "喷溅药水", "力量药水", "迅捷药水", "夜视药水", "隐身药水", "跳跃药水", "缓降药水",
        "打火石", "木屋", "TNT", "铁轨机", "刷线机", "地狱门", "刷怪笼", "附魔台", "末影珍珠", "熔岩桶", "烟花火箭", "火焰弹"
    };





        /// <summary>
        /// 是否在自动初始化状态
        /// </summary>
        public bool IsAutoInit { get; set; }

        /// <summary>
        /// 角色总数
        /// </summary>
        public int CharCount { get; set; }
        /// <summary>
        /// 起名冲突加值
        /// </summary>
        public int CharNameSeed { get; set; }

        public RoleModel CurRole { get; set; }

        public int CurRoleIndex { get; set; }

        /// <summary>
        /// 修车角色当前地图等级
        /// </summary>
        private int _curMapLv { get; set; }

        /// <summary>
        /// 目标地图等级
        /// </summary>
        private int _targetMapLv { get; set; }

        private bool _isNeedDungeon { get; set; }




        /// <summary>
        /// 起号前置
        /// </summary>
        private static string PrefixName = ConfigUtil.GetAppSetting("PrefixName");

        /// <summary>
        /// 同步来源
        /// </summary>
        private static string FilterSource = ConfigUtil.GetAppSetting("FilterSource");




        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public void OnDungeonRequired(params object[] args)
        {
            var data = args[0].ToObject<Dictionary<string, object>>();
            var isSuccess = data["isSuccess"];
            var isNeedDungeon = data["isNeedDungeon"];
            _isNeedDungeon = bool.Parse(isNeedDungeon.ToString());
        }

        public CharacterController(BroWindow win) : base(win)
        {
            _isNeedDungeon = false;
            _eventMa = win.GetEventMa();
            _eventMa.SubscribeEvent(emEventType.OnDungeonRequired, OnDungeonRequired);
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="count">当前有多少角色</param>
        /// <returns></returns>
        private async Task<string> CreateName()
        {
            await Task.Delay(50);
            var curName = AccountController.Instance.User.Username;
            var index = AccountCfg.Instance.Accounts.FindIndex(p => p.Username == curName);
            if (!String.IsNullOrWhiteSpace(PrefixName))
            {

                var name = $@"{PrefixName}{24 * index + CharCount + CharNameSeed}";
                P.Log(name);
                return name;
            }

            var name1 = Names[12 * index + CharCount] + (CharNameSeed == 0 ? "" : CharNameSeed.ToString());
            P.Log(name1);
            return name1;
        }
        /// <summary>
        /// 生成种族和职业
        /// </summary>
        /// <returns></returns>

        private Tuple<int, int> CreateRaceAndType(List<RoleModel> roles)
        {
            var lastJob = roles.Count == 0 ? null : roles.Last();
            if (lastJob == null || lastJob.Job == emJob.死骑)
            {
                return new Tuple<int, int>(1, 5);
            }
            if (lastJob.Job == emJob.骑士)
            {
                return new Tuple<int, int>(8, 4);
            }
            if (lastJob.Job == emJob.死灵)
            {
                return new Tuple<int, int>(12, 5);
            }
            //if (roles.Where(p => p.Job == emJob.武僧).Count() < 2)
            //{
            //    return new Tuple<int, int>(10, 5);
            //}
            return null;
        }



        /// <summary>
        /// 起名冲突
        /// </summary>
        /// <param name="args"></param>
        private void OnCharNameConflict(params object[] args)
        {
            CharNameSeed += 1;
        }



        private async Task StartDungeon(ChromiumWebBrowser bro, RoleModel role)
        {
            //开始秘境
            int dungeonLv = GetDungeonLv(_curMapLv);
            if (dungeonLv != _curMapLv) await _win.SignalCallback("charReload",  () =>
                {
                     SwitchTo(dungeonLv);
                });

            await _win.SignalCallback("charReload",  () =>
            {
                _browser.LoadUrl($"https://www.idleinfinity.cn/Map/Dungeon?id={role.RoleId}");
            });



            if (role.Level >= 30)
            {
                await AutoDungeon();
                return;
            }

            await _win.SignalCallback("DungeonEnd", async () =>
            {
                var a = await _browser.EvaluateScriptAsync("_map.startExplore();");
                var r = a.Result;
            });

            await Task.Delay(2000);
            //再次尝试直接抵达
            await _win.SignalCallback("charReload", () =>
            {
                _browser.LoadUrl($"https://www.idleinfinity.cn/Map/Detail?id={role.RoleId}");
            });
            await Task.Delay(2000);
            await _win.SignalCallback("charReload", async () =>
            {
                await SwitchTo(_curMapLv + 10);
            });
            await Task.Delay(2000);
            await SwitchMap(bro, role);

        }
        private async Task AutoDungeon()
        {
            await _win.SignalCallback("startAuto", () =>
            {
                _win.CallJs("_map.autoDungeon();");
            });
            await Task.Delay(2000);
        }

        /// <summary>
        /// 开始
        /// </summary>
        public async void StartInit()
        {
            IsAutoInit = true;
            _browser = await GetBrowserAsync();
            await StartAutoJob();
        }

        public void Stop()
        {
            IsAutoInit = false;

        }
        /// <summary>
        /// 开始自动执行
        /// </summary>
        private async Task StartAutoJob()
        {

            if (IsAutoInit)
            {  //建号
                await CreateRole();
                await GoToMakeGroup();
                await MakeUnion();
                //工会拉人结束 开始组队
                await MakeGroup();
                Stop();
                return;
            }
        }


        private async Task GoToMakeGroup()
        {
            if (_browser.Address.IndexOf("Character/Group") == -1)
            {
                _browser.Load($@"https://www.idleinfinity.cn/Character/Group?id={AccountController.Instance.User.Roles[0].RoleId}");
                await JsInit();
            }

        }

        /// <summary>
        /// 继续建队 跳三个角色作为队长
        /// </summary>
        /// <returns></returns>
        private async Task ContinueMakeGroup()
        {
            int nextIndex = AccountController.Instance.User.Roles.FindIndex(p => p.RoleId == AccountController.Instance.CurRole.RoleId) + 3;
            if (nextIndex > 11) return;
            int roleId = AccountController.Instance.User.Roles[nextIndex].RoleId;
            _browser.Load($@"https://www.idleinfinity.cn/Character/Group?id={roleId}");
            await JsInit();

        }


        /// <summary>
        /// 拉队伍
        /// </summary>
        /// <returns>true拉队伍结束需要换一组</returns>
        private async Task MakeGroup()
        {
            var existMembers = await GetExistGroupMember();
            if (existMembers.Length == 3 && AccountController.Instance.User.Roles.FindIndex(p => p.RoleId == AccountController.Instance.CurRole.RoleId) < 9)
            {
                await ContinueMakeGroup();
                existMembers = await GetExistGroupMember();
            }
            if (existMembers.Length == 3 && AccountController.Instance.User.Roles.FindIndex(p => p.RoleId == AccountController.Instance.CurRole.RoleId) == 9)
            {
                return;
            }

            var hasGroup = await HasGroup();
            if (!hasGroup)
            {
                await CreateGroup();

            }
            else
            {
                await AddGroupMember();
                //一个队伍三个人 加完一次就结束了
            }
            await MakeGroup();

        }

        /// <summary>
        /// 拉工会
        /// </summary>
        /// <returns>true拉工会结束 </returns>
        private async Task MakeUnion()
        {
            var hasUnion = await HasUnion();
            if (!hasUnion)
            {
                //创建工会
                await CreateUnion();
            }

            var existMember = await GetExistUnionMember();
            if (AccountController.Instance.User.Roles == null || existMember == null)
            {
                Console.WriteLine("roles空");
            }
            var notInUnionMember = AccountController.Instance.User.Roles.Where(p => !existMember.Contains(p.RoleName)).FirstOrDefault();
            if (notInUnionMember != null)
            {
                await AddUnionMember(notInUnionMember);
                await MakeUnion();
            }


        }

        /// <summary>
        /// 是否有工会
        /// </summary>
        /// <returns></returns>
        private async Task<Boolean> HasUnion()
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.hasUnion();");
                return d.Result.ToObject<Boolean>();
            }
            else return false;
        }

        /// <summary>
        /// 是否有工会
        /// </summary>
        /// <returns></returns>
        private async Task<Boolean> HasGroup()
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.hasGroup();");
                return d.Result.ToObject<Boolean>();
            }
            else return false;
        }

        /// <summary>
        /// 创建工会
        /// </summary>
        /// <returns></returns>
        private async Task CreateUnion()
        {
            var data = new Dictionary<string, object>();
            data["firstRoleId"] = AccountController.Instance.User.Roles[0].RoleId;
            data["cname"] = AccountController.Instance.User.Roles[1].RoleName;
            data["gname"] = AccountController.Instance.User.Roles[0].RoleName + "★";
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.createUnion({data.ToLowerCamelCase()});");
                await JsInit();
            }

        }

        /// <summary>
        /// 创建工会
        /// </summary>
        /// <returns></returns>
        private async Task CreateGroup()
        {

            var data = new Dictionary<string, object>();
            data["roleid"] = AccountController.Instance.CurRole.RoleId;
            int nextIndex = AccountController.Instance.User.Roles.FindIndex(p => p.RoleId == AccountController.Instance.CurRole.RoleId) + 1;
            data["cname"] = AccountController.Instance.User.Roles[nextIndex].RoleName;
            data["gname"] = AccountController.Instance.CurRole.RoleName + "★";
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.createGroup({data.ToLowerCamelCase()});");
                await JsInit();
            }

        }

        /// <summary>
        /// 工会加人
        /// </summary>
        /// <returns></returns>
        private async Task AddUnionMember(RoleModel r)
        {

            var data = new Dictionary<string, object>();
            data["firstRoleId"] = AccountController.Instance.User.Roles[0].RoleId;
            data["cname"] = r.RoleName;
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.addUnionMember({data.ToLowerCamelCase()});");
                await JsInit();
            }

        }
        /// <summary>
        /// 组队加人
        /// </summary>
        /// <returns></returns>
        private async Task AddGroupMember()
        {
            var existMembers = await GetExistGroupMember();
            if (existMembers.Length == 3) return;
            var data = new Dictionary<string, object>();
            data["roleid"] = AccountController.Instance.CurRole.RoleId;
            int nextIndex = AccountController.Instance.User.Roles.FindIndex(p => p.RoleId == AccountController.Instance.CurRole.RoleId) + 2;
            data["cname"] = AccountController.Instance.User.Roles[nextIndex].RoleName;
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.addGroupMember({data.ToLowerCamelCase()});");
                await JsInit();
            }

        }

        /// <summary>
        /// 查找在工会中的人员
        /// </summary>
        /// <returns></returns>
        private async Task<string[]> GetExistUnionMember()
        {

            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.getExistUnionMember();");
                return d.Result.ToObject<string[]>();

            }
            else return new string[] { };

        }

        /// <summary>
        /// 查找在队伍中的人员
        /// </summary>
        /// <returns></returns>
        private async Task<string[]> GetExistGroupMember()
        {

            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.getExistGroupMember();");
                return d.Result.ToObject<string[]>();
            }
            else return new string[] { };

        }


        /// <summary>
        /// 获取人物属性
        /// </summary>
        /// <returns></returns>
        public async Task<CharAttributeModel> GetCharAtt()
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.getAttribute();");
                return d.Result?.ToObject<CharAttributeModel>();
            }
            else return null;

        }

        /// <summary>
        /// 获取人物技能
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, SkillModel>> GetSkillInfo()
        {

            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.getSkillInfo();");
                return d.Result?.ToObject<Dictionary<string, SkillModel>>();
            }
            else return null;

        }


        /// <summary>
        /// 获取人物技能
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, SkillModel>> GetSkillConfig()
        {

            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.getSkillConfig();");
                return d.Result?.ToObject<Dictionary<string, SkillModel>>();
            }
            else return null;

        }

        /// <summary>
        /// 获取人物技能
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetSkillGroup()
        {

            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.getSkillGroup();");
                return d.Result?.ToObject<List<string>>();
            }
            else return null;

        }

        /// <summary>
        /// 获取人物技能
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetRoleId()
        {

            var d = await _browser.EvaluateScriptAsync($@"_char.cid");
            return d.Result.ToObject<int>();

        }


        /// <summary>
        /// 创建角色
        /// </summary>
        /// <returns></returns>
        public async Task CreateRole()
        {
            if (_browser.Address.IndexOf(PageLoadHandler.HomePage) > -1)
            {
                var roles = await GetRoles();
                CharCount = roles == null ? 0 : roles.Count;
                if (CharCount < 12)
                {
                    //不满12个号去建号
                    _browser.LoadUrl("https://www.idleinfinity.cn/Character/Create");
                    await JsInit();
                    var data = new Dictionary<string, object>();
                    data["name"] = await CreateName();
                    var info = CreateRaceAndType(roles);
                    data["race"] = info.Item2;
                    data["type"] = info.Item1;
                    if (_browser.CanExecuteJavascriptInMainFrame)
                    {
                        var d = await _browser.EvaluateScriptAsync($@"_init.createRole({data.ToLowerCamelCase()});");
                        await JsInit();
                    }
                    await CreateRole();
                }
            }



        }

        /// <summary>
        /// 读取home页所有角色
        /// </summary>
        /// <returns></returns>
        public async Task<List<RoleModel>> GetRoles()
        {

            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.getRoleInfo();");
                return d.Result?.ToObject<List<RoleModel>>();
            }
            return null;
        }

        protected async Task<ChromiumWebBrowser> GetBrowserAsync()
        {

            var accName = AccountController.Instance.User.AccountName;
            var seed = await BroTabManager.Instance.TriggerAddTabPage(accName, $"https://www.idleinfinity.cn/Home/Index");
            _broSeed = seed;
            return BroTabManager.Instance.GetBro(seed);
        }

        #region 属性点

        /// <summary>
        /// 获取人物属性
        /// </summary>
        /// <returns></returns>
        public async Task<CharBaseAttributeModel> GetCharBaseAtt()
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.getSimpleAttribute();");
                return d.Result?.ToObject<CharBaseAttributeModel>();
            }
            else return null;

        }

        public async Task<CharBaseAttributeModel> GetAttributeSimpleInfo(ChromiumWebBrowser bro, RoleModel role)
        {
            _browser = bro;
            int roleid = role.RoleId;
            //不在详细页先去详细页读取属性
            if (bro.Address.IndexOf(PageLoadHandler.CharDetail) == -1)
            {
                _browser.LoadUrl($"https://www.idleinfinity.cn/Character/Detail?id={roleid}");
                await JsInit();
            }

            var info = await GetCharBaseAtt();
            return info;
        }

        /// <summary>
        /// 保存人物属性加点
        /// </summary>
        /// <returns></returns>
        public async Task SaveCharAtt(CharBaseAttributeModel data)
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                await _browser.EvaluateScriptAsync($@"_char.attributeSave({data.ToLowerCamelCase()});");
                return;
            }
            else return;
        }
        /// <summary>
        /// 重置人物属性加点
        /// </summary>
        /// <returns></returns>
        public async Task ResetCharAtt()
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                //P.Log($"save att:{data.StrAdd}");
                await _browser.EvaluateScriptAsync($@"_char.attributeReset();");
                return;
            }
            else return;
        }

        #endregion  

        #region 技能
        public async Task StartAddSkill(ChromiumWebBrowser bro, UserModel user)
        {
            _browser = bro;
            for (int i = 0; i < user.Roles.Count; i++)
            {
                var role = user.Roles[i];
                await AddSkillPoints(_browser, role);
                await Task.Delay(2000);
            }

        }

        public async Task AddSkillPoints(ChromiumWebBrowser bro, RoleModel role)
        {
            _browser = bro;
            int roleid = role.RoleId;
            //不在详细页先去详细页读取属性
            await _win.SignalCallback("charReload", () =>
            {
                _browser.LoadUrl($"https://www.idleinfinity.cn/Skill/Config?id={roleid}&e=1");
            });

            //判断下当前技能合不合适 合适就跳过
            var curSkill = await GetSkillConfig();
            List<string> curGroupSkill = await GetSkillGroup();//当前携带的技能数组
            var skillConfig = SkillPointCfg.Instance.GetSkillPoint(role.Job, role.Level);
            var targetSkillPoint = GetTargetSkillPoint(role.Level, skillConfig);
            var r = CheckRoleSkill(curSkill, targetSkillPoint, curGroupSkill);
            var isNeedRest = r.Item1;
            var isNeedAdd = r.Item2;
            var isNeedSetGroup = r.Item3;


            P.Log("开始重置技能加点！", emLogType.AutoEquip);
            if (isNeedRest) await _win.SignalCallback("charReload", async () =>
               {
                   await SkillRest();
               });

            //重置一定加点
            if (isNeedAdd || isNeedRest) await _win.SignalCallback("charReload", async () =>
              {
                  await SkillSave(targetSkillPoint, skillConfig.JobName);
              });

            var groupid = GetSkillGroup(skillConfig);
            if (isNeedSetGroup) await _win.SignalCallback("charReload", async () =>
              {
                  await SkillGroupSave(groupid);
              });



            if (bro.Address.IndexOf(PageLoadHandler.CharDetail) == -1)
            {
                await _win.SignalCallback("charReload", () =>
                {
                    _browser.LoadUrl($"https://www.idleinfinity.cn/Character/Detail?id={roleid}");

                });

            }
            var s = await _browser.EvaluateScriptAsync("_char.hasKey()");
            var hasKey = s.Result.ToObject<bool>();

            if (!hasKey) await _win.SignalCallback("charReload", async () =>
            {
                await SkillKeySave(skillConfig.KeySkillId);
            });

        }
        private async Task SkillRest()
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.skillReset();");

            }
        }

        /// <summary>
        /// 保存技能并返回需要确认的技能组
        /// </summary>
        /// <param name="targetSkill"></param>
        /// <param name="jobname"></param>
        /// <returns></returns>
        private async Task SkillSave(Dictionary<string, int> targetSkill, string jobname)
        {
            P.Log("开始技能加点！", emLogType.AutoEquip);

            List<string> strList = new List<string>();

            targetSkill.ToList().ForEach(p =>
            {
                var s = IdleSkillCfg.Instance.GetIdleSkill(jobname, p.Key);
                strList.Add($"{s.Id}|{p.Value}");

            });
            var skillStr = string.Join(",", strList);
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("sid", skillStr);
            P.Log($"技能加点详情:{skillStr}", emLogType.AutoEquip);
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.skillSave({data.ToLowerCamelCase()});");
                if (!d.Success)
                {
                    P.Log($"技能加点失败：{d.Message}", emLogType.AutoEquip);
                }
                else
                {
                    P.Log($"技能加点成功！", emLogType.AutoEquip);
                }

            }

        }

        private string GetSkillGroup(SkillPoint config)
        {
            List<string> list = new List<string>();
            config.GroupSkill.ForEach(p =>
            {
                var s = IdleSkillCfg.Instance.GetIdleSkill(config.JobName, p);
                list.Add(s.Id.ToString());
            });
            return string.Join(",", list);
        }

        /// <summary>
        /// 选技能组
        /// </summary>
        /// <param name="targetSkill"></param>
        /// <param name="jobname"></param>
        /// <returns></returns>
        private async Task SkillGroupSave(string sid)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("sid", sid);
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.skillGroupSave({data.ToLowerCamelCase()});");
                if (!d.Success)
                {
                    P.Log($"保存携带技能失败：{d.Message}", emLogType.AutoEquip);
                }
                else
                {
                    P.Log($"保存携带技能成功！", emLogType.AutoEquip);
                }

            }
        }
        /// <summary>
        /// 获取配置匹配当前等级合适的技能字典
        /// </summary>
        /// <param name="curLv"></param>
        /// <param name="config"></param>
        /// <param name="curSkillList"></param>
        /// <returns>返回一个技能分配字典</returns>
        private Dictionary<string, int> GetTargetSkillPoint(int curLv, SkillPoint config)
        {
            int pointCount = curLv;//技能点数等于人物等级
            P.Log($"开始计算技能加点！技能点总数：{pointCount}", emLogType.AutoEquip);
            var targetSkillDic = new Dictionary<String, int>();
            foreach (var item in config.RemainSkill)
            {
                var name = item.Key;
                var lv = item.Value;
                targetSkillDic.Add(name, lv);
                pointCount -= lv;
            }
            foreach (var p in config.PrioritySkill)
            {
                if (pointCount == 0) break;

                var name = p;
                var idleSkill = IdleSkillCfg.Instance.GetIdleSkill(config.JobName, p);
                var maxSkillPoint = idleSkill.CurLvMaxPoint(curLv);//当前等级能加到max是多少
                if (!targetSkillDic.ContainsKey(name)) targetSkillDic.Add(name, 0);
                int addPoint = maxSkillPoint - targetSkillDic[name];//减去remain中已经配置的点剩下需要加多少点
                if (pointCount <= addPoint)
                {
                    //剩余点数不够加到最大了就按照剩余加
                    targetSkillDic[name] += pointCount;
                    pointCount = 0;//加完了
                }
                else
                {
                    targetSkillDic[name] += addPoint;
                    pointCount -= addPoint;
                }

            }
            return targetSkillDic;

        }

        /// <summary>
        /// 判断一下技能是否合适
        /// </summary>
        /// <param name="skills"></param>
        /// <param name="targetSkillDic"></param>
        /// <returns></returns>
        private Tuple<bool, bool, bool> CheckRoleSkill(Dictionary<string, SkillModel> skills, Dictionary<string, int> targetSkillDic, List<string> curGroupSkill)
        {
            var noZeroSkill = skills.Where(p => p.Value.Lv > 0).ToDictionary(kv => kv.Key, kv => kv.Value);
            if (noZeroSkill.Count == 0)
            {
                return new Tuple<bool, bool, bool>(false, true, true);
            }
            if (noZeroSkill.Keys.Count != targetSkillDic.Keys.Count)
            {
                return new Tuple<bool, bool, bool>(true, true, true);
            }
            bool isNeedReset = false;//需要重置
            bool isNeedAdd = false;//需要加点
            bool isNeedSetGroup = false;//需要重设携带技能
            var groupList = curGroupSkill.Where(p => p != "普通攻击" && p != "基础法术" && p != "空缺").ToList();
            if (groupList.Count == 0)
            {
                isNeedSetGroup = true;
            }
            foreach (var item in groupList)
            {
                if (!targetSkillDic.ContainsKey(item))
                {
                    isNeedSetGroup = true;
                    break;
                }
            }
            foreach (var item in targetSkillDic)
            {
                //加的技能不包含在目标技能
                if (!noZeroSkill.ContainsKey(item.Key))
                {
                    //当前技能没点目标技能 
                    isNeedReset = true;
                    break;
                }

                //比较下数值
                if (noZeroSkill[item.Key].Lv != item.Value)
                {
                    if (noZeroSkill[item.Key].Lv > item.Value)//如果已经加的技能大于计算出的技能则需要重置才能加点
                    {
                        isNeedReset = true;
                        break;
                    }
                    isNeedAdd = true;
                }

            }
            return new Tuple<bool, bool, bool>(isNeedReset, isNeedAdd, isNeedSetGroup);
        }

        /// <summary>
        /// 保存Key技能
        /// </summary>
        /// <param name="targetSkill"></param>
        /// <param name="jobname"></param>
        /// <returns></returns>
        private async Task SkillKeySave(int skillId)
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.skillKeySave({skillId});");
            }
        }
        #endregion

        #region 切图

        public async Task StartSwitchMap(ChromiumWebBrowser bro, UserModel user)
        {
            _browser = bro;
            int unFinishIndex = 0;
            var list = FreeDb.Sqlite.Select<TaskProgress>().Where(p => p.UserName == user.AccountName && p.IsEnd == false).ToList();
            var unfinishedTask = list.Count > 0 ? list[0] : null;
            if (unfinishedTask == null)
            {
                unfinishedTask = new TaskProgress() { IsEnd = false, Type = emTaskType.MapSwitch, UserName = user.AccountName, Roleid = user.Roles[0].RoleId };
                var rows = FreeDb.Sqlite.InsertOrUpdate<TaskProgress>().SetSource(unfinishedTask).ExecuteAffrows();
            }
            else
            {
                var index = user.Roles.FindIndex(p => p.RoleId == unfinishedTask.Roleid);
                unFinishIndex = index;

            }

            //查询未完成的任务
            for (int i = unFinishIndex; i < user.Roles.Count; i++)
            {
                RoleModel role = user.Roles[i];

                await Task.Delay(1000);
                await SwitchMap(_browser, role);
                await Task.Delay(1000);
                if (unfinishedTask != null)
                {
                    unfinishedTask.Roleid = role.RoleId;

                }
                if (i == user.Roles.Count - 1)
                {
                    unfinishedTask.IsEnd = true;

                }
                var one = FreeDb.Sqlite.Select<TaskProgress>().Where(p => p.UserName == user.AccountName && p.IsEnd == false).First();
                unfinishedTask.Id = one.Id;
                FreeDb.Sqlite.InsertOrUpdate<TaskProgress>().SetSource(unfinishedTask).ExecuteAffrows();
            }
        }
        public async Task SwitchMap(ChromiumWebBrowser bro, RoleModel role)
        {
            int roleid = role.RoleId;
            await _win.SignalCallback("charReload", () =>
            {
                _browser.LoadUrl($"https://www.idleinfinity.cn/Map/Detail?id={roleid}");
            });
            var curMapLv = await GetCurMapLv();
            _curMapLv = curMapLv;
            //检查是否层数合适
            var setting = MapSettingCfg.Instance.GetSetting(role.Level);
            if (setting == null || !setting.CanSwitch(role.Level, curMapLv)) return;

            int targetLv = setting.MapLv; //setting.MapLv;
            _targetMapLv = targetLv;
            var r = await _win.CallJs("_map.canSwitch()");
            var canSwitch = r.Result.ToObject<bool>();
            if (!canSwitch) return;
            await _win.SignalRaceCallBack(new string[] { "charReload" }, async () =>
           {
               await SwitchTo(targetLv);

           });

            if (_isNeedDungeon)
            {
                _isNeedDungeon = false;//进来了就重置
                await StartDungeon(bro, role);
            }

        }

        private async Task SwitchTo(int targetLv)
        {
            var d = await _browser.EvaluateScriptAsync($@"_char.mapSwitch({targetLv});");
        }




        private async Task<int> GetCurMapLv()
        {
            var d = await _browser.EvaluateScriptAsync($@"_char.getCurMapLv();");
            return d.Result.ToObject<int>();
        }

        private int GetDungeonLv(int curLv)
        {
            double result = (double)curLv / 10.0;
            return int.Parse(Math.Ceiling(result).ToString()) * 10;
        }
        #endregion
        #region 复制过滤

        public async Task StartSyncFilter(ChromiumWebBrowser bro, UserModel user)
        {
            _browser = bro;

            await Task.Delay(1000);
            RoleModel role = user.Roles[0];
            await _win.SignalCallback("charReload", () =>
            {
                _browser.LoadUrl("https://www.idleinfinity.cn/Config/Query?id=");
            });
            await _win.SignalCallback("charReload", async () =>
            {
                await CopyConfig();
            });

        }
        private async Task CopyConfig()
        {
            var data = new Dictionary<string, object>();
            data.Add("name", FilterSource);
            var d = await _browser.EvaluateScriptAsync($@"_char.copyConfig({data.ToLowerCamelCase()});");
            Console.WriteLine(d.Message);
        }
        #endregion

    }
}


/* 加点调用测试
//var info = await CharacterController.Instance.GetAttributeSimpleInfo(BroTabManager.Instance.GetBro(BroTabManager.Instance.GetFocusID()), AccountController.Instance.User.Roles[1]);
//P.Log($"point:{info.Point}--csa:{info.StrAdd}");
//if (info != null && info.Point > 0)
//{
//    info.StrAdd += info.Point;
//    await CharacterController.Instance.SaveCharAtt(info);
//}
*/
