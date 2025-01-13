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
        var e = {};//装备对象
        e.etype = $(this).data('type');
        var equipItem = $(this).prev();
        e.equipName = equipItem.text().replace('【', '').replace('】', '');
        e.etypeName = equipItem.prev().text().replace('：', '');
        eMap[e.etype] = e;
    });

    return eMap;
}

function getPackageEquips() {
    console.log('start getPackageEquips');
    var eMap = {};
    var box = $('.panel-body.equip-bag')[0];
    $(box).children().each(function () {
        var e = {};//装备对象
        var equipItem = $(this).find('span:first');
        e.eid = equipItem.data('id');
        var equipContent = $(`.equip-content-container`).find(`[data-id="${e.eid}"]`); //#${ e.eid }.equip-content
        e.equipName = equipContent.find('p:first').text();
        e.etypeName = equipContent.find('p:first').next().text();
        if (!e.equipName.includes('秘境') &&
            !e.etypeName.includes('药水') &&
            !e.etypeName.includes('珠宝') &&
            !e.etypeName.includes('卡片') &&
            !e.etypeName.includes('宝箱')) {
            eMap[e.eid] = e;
        }
    });
    return eMap;
}
function getRepositoryEquips() {
    console.log('start getRepositoryEquips');
    var eMap = {};
    var box = $('.panel-body.equip-box')[0];
    $(box).children().each(function () {
        var e = {};//装备对象
        var equipItem = $(this).find('span:first');
        e.eid = equipItem.data('id');
        var equipContent = $(`.equip-content-container`).find(`[data-id="${e.eid}"]`); //#${ e.eid }.equip-content
        e.equipName = equipContent.find('p:first').text();
        e.etypeName = equipContent.find('p:first').next().text();
        if (!e.equipName.includes('秘境') &&
            !e.etypeName.includes('药水') &&
            !e.etypeName.includes('珠宝') &&
            !e.etypeName.includes('卡片') &&
            !e.etypeName.includes('宝箱')) {
            eMap[e.eid] = e;
        }
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
} function equipOff(cid, etype) {
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