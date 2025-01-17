﻿async function init() {
    try {
        console.log('equip.js start init');
        await CefSharp.BindObjectAsync("Bridge");
    }
    catch (e) {
        console.log("Error:", e);
    }
}

init().then((r) => {
    console.log('equip.js inited');
    Bridge.invokeEvent('OnJsInited', 'equip');
})

function getCurEquips() {
    console.log('start getCurEquips');
    var eMap = {};
    $('.sr-only.label.label-danger.equip-off').each(function () {
        var sortid = $(this).data('type');
        var __equip = $(this).prev();
        var id = __equip.data('id');
        var quality = __equip.data('type');
        var equipContent = $(`.equip-content-container`).find(`[data-id="${id}"]`);
        var content = equipContent.text();
        var e = getEquipInfo(id, sortid, quality, content);
        eMap[e.esort] = e;
    });

    return eMap;
}

function getPackageEquips() {
    console.log('start getPackageEquips');
    var eMap = {};
    var box = $('.panel-body.equip-bag')[0];
    $(box).children().each(function () {
        var equipItem = $(this).find('span:first');
        var id = equipItem.data('id');
        var quality = equipItem.data('type');
        var equipContent = $(`.equip-content-container`).find(`[data-id="${id}"]`);
        var content = equipContent.text();
        var e = getEquipInfo(id, 999, quality, content);
        eMap[e.eid] = e;
    });
    return eMap;
}

function getEquipInfo(eid, sortid, quality, content) {
    var e = {};
    e.eid = eid;
    e.esort = sortid;
    e.quality = quality;
    content = content.replace(/^\s*\n/gm, "")
    content = content.replace(/[ ]/g, "")
    e.content = content;
    var sc = content.split('\n');
    var name = sc[0].match(/(.*)★{0,1}\(\d*\)/);
    var baseName = "未知"
    if (quality == "set" || quality == "unique" || quality == "artifact") {
        if (sc[1] != "已绑定") baseName = sc[1];
        else baseName = sc[2];
    }
    else if (sc[2].includes("可以作为镶嵌物"))
        baseName = "珠宝";
    else if (name[1].includes("秘境"))
        baseName = "秘境";
    e.equipBaseName = baseName;
    e.equipName = name[1];
    e.isPerfect = sc[0].includes('★');
    e.isLocal = sc[1].includes("已绑定");
    return e;
}

function getRepositoryEquips() {
    console.log('start getRepositoryEquips');
    var eMap = {};
    var box = $('.panel-body.equip-box')[0];
    $(box).children().each(function () {
        var equipItem = $(this).find('span:first');
        var id = equipItem.data('id');
        var quality = equipItem.data('type');
        var equipContent = $(`.equip-content-container`).find(`[data-id="${id}"]`);
        var content = equipContent.text();
        var e = getEquipInfo(id, 999, quality, content);
        eMap[e.eid] = e;
    });
    return eMap;
}

function packageNext() {
    var i = $('.panel-body.equip-bag:first').next().find('a:contains("下页")');
    if (i.length == 0) {
        return false;
    }
    else {
        i[0].click();
        return true;
    }
}
function repositoryNext() {
    var i = $('.panel-body.equip-box:first').next().find('a:contains("下页")');
    if (i.length == 0) {
        return false;
    }
    else {
        i[0].click();
        return true;
    }
}

function equipOn(cid, eid) {
    console.log('start equipOn');
    var data = MERGE_Form({
        cid: cid,
        eid: eid,
    });
    POST_Message("EquipOn", data, "html", 2000)
        .then(r => {
            console.log("EquipOn success");
            location.reload();
        })
        .catch(r => {
            console.log(r);
        });
}
function equipOff(cid, etype) {
    console.log('start equipOff');
    var data = MERGE_Form({
        cid: cid,
        cet: etype,
    });
    POST_Message("EquipOff", data, "html", 2000)
        .then(r => {
            console.log("EquipOff success");
            location.reload();
        })
        .catch(r => {
            console.log(r);
        });
}