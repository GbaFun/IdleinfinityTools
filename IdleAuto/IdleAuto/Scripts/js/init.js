﻿

let _init = {};
(function () {


    async function init() {
        try {
            await CefSharp.BindObjectAsync("Bridge");
        }
        catch (e) {
            console.log("Error:", e);
        }
    }
    var dataMap = {}//存储dataid对应装备的数据 

    init().then(async () => {
        Bridge.invokeEvent('OnJsInited', 'init');
        if (location.href.indexOf("Home/Index") > -1) {
            Bridge.invokeEvent('OnAccountCheck', getAccountName());
        }
    })

    //创建角色
    async function createRole(data) {
        var data = {
            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
            name: data.name,
            type: data.type,
            race: data.race
        };
        //debugger;
        POST_Message("Create", data, "post", 1500).then((r) => {
            location.href = "Home/Index";
        }).catch((e) => {
            if (e.responseText.indexOf("角色名称已经存在") > -1) {
                Bridge.invokeEvent('OnCharNameConflict');
            }
            if (e.status == 200) {
                location.href = "../Home/Index";
            }
        })
    }
    async function getRoleInfo() {
        var roles = [];
        $(".col-sm-6.col-md-4").each(function () {
            var role = {};
            role.RoleId = $(this).data("id");
            role.RoleName = $(this).find("span.sort-item.name").text();
            var d = $(this).find("div.media-body").children("div").first();
            console.log(d);
            console.log(d.text());
            role.RoleInfo = d.text();
            roles.push(role);
        });
        return roles;
    }
    async function hasUnion(data) {
        var hasUnion = $("span:contains('公会')").next().text().indexOf("创建并邀请") >-1;
        return !hasUnion;
    }

    async function hasGroup(data) {
        var d = $("span:contains('PVE')").next().text().indexOf("创建并邀请") > -1;
        return !d;
    }

    async function createUnion(data) {
        var data = {
            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
            id: data.firstRoleId,
            type: 2,
            cname: data.cname,
            gname: data.gname
        };
        //debugger;
        POST_Message("GroupCreate", data, "post", 1500).then((r) => {
            location.reload();
        }).catch((e) => {
            location.reload();
        })
    }
    async function createGroup(data) {
        var data = {
            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
            id: data.roleid,
            type: 0,
            cname: data.cname,
            gname: data.gname
        };
        //debugger;
        POST_Message("GroupCreate", data, "post", 1500).then((r) => {
            location.reload();
        }).catch((e) => {
            location.reload();
        })
    }

    async function addUnionMember(data) {
        var data = {
            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
            id: data.firstRoleId,
            cname: data.cname,
            gid: $($($(".panel-inverse")[2]).find("a[data-gid]")[2]).attr("data-gid")
        };
        //debugger;
        POST_Message("GroupAddChar", data, "post", 1500).then((r) => {
            location.reload();
        }).catch((e) => {
            location.reload();
        })
    }

    async function addGroupMember(data) {
        var data = {
            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
            id: data.roleid,
            cname: data.cname,
            gid: $($($(".panel-inverse")[0]).find("a[data-gid]")[0]).attr("data-gid")
        };
      
        POST_Message("GroupAddChar", data, "post", 1500).then((r) => {
           
            location.reload();
        }).catch((e) => {
            
            location.reload();
        })
    }

    async function getExistUnionMember() {
        return $($(".panel-body")[2]).find('span.name').map((index, item) => {
            return item.innerText;
        }).get();
    }

    async function getExistGroupMember() {
        return $($(".panel-body")[0]).find('span.name').map((index, item) => {
            return item.innerText;
        }).get();
    }

     function getAccountName() {
        var name = $(".panel-heading")[0].innerText.match(/^(.*?) 已验证/);
        return name[1];
    }




    _init.createRole = createRole;
    _init.getRoleInfo = getRoleInfo;
    _init.hasUnion = hasUnion;
    _init.createUnion = createUnion;
    _init.getExistUnionMember = getExistUnionMember;
    _init.addUnionMember = addUnionMember;
    _init.hasGroup = hasGroup;
    _init.createGroup = createGroup;
    _init.addGroupMember = addGroupMember;
    _init.getExistGroupMember = getExistGroupMember;
})();
