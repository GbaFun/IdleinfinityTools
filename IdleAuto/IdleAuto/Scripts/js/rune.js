//������Ĳ��
function loadStorePlugin() {
    if (location.href.indexOf("Equipment/Material") == -1) {
        return;
    }

    var compandMode = false;
    var autoCompandMode = false;
    var storedRuneCounts = {}; // �洢ÿ�������������
    var storedCompandCounts = {}; // �ϳ�ÿ��������ı�������

    function showChange() {
        var lastCompandRuneId = parseInt(localStorage.getItem('lastCompandRuneId'));
        //ˢ��ҳ��ǰ��������һ�����������߼����ҷ��ļ��û��������
        if (!lastCompandRuneId.isNaN && lastCompandRuneId >= 0 && lastCompandRuneId < 33) {
            autoCompandMode = true;
        }

        console.log("�ϴ��Զ���������id��" + lastCompandRuneId + "----�Ƿ�����Զ�����ģʽ��" + autoCompandMode);
        //һ������ģʽ
        if (autoCompandMode) {
            var curCompandRuneId = lastCompandRuneId + 1;
            $('.col-xs-12.col-sm-4.col-md-3.equip-container').each(function () {
                var runeName = $(this).find('p:first .equip-name .artifact:nth-child(2)').text().trim(); // ��ȡ�������Ƶĵڶ��� span
                var runeCount = parseInt($(this).find('p:first .artifact').last().text().trim()); // ��ȡ��������

                var storedCompandCounts = JSON.parse(localStorage.getItem('storedCompandCounts'));

                var storedCount = storedCompandCounts[runeName];
                if (storedCount.isNaN || storedCount == undefined)
                    storedCount = 0;
                var regexResult = runeName.match(/-(\d+)#/);
                var count = 0;
                if (runeCount > storedCount) {
                    count = runeCount - storedCount;
                    count = count - count % 2;
                }

                if (regexResult[1] == curCompandRuneId) {
                    localStorage.setItem('lastCompandRuneId', regexResult[1]);
                    if (count > 1) {
                        //����������Ϣ
                        compandStore(regexResult[1], count);
                    }
                    else {
                        showChange();
                    }
                    // return;
                }
            });
            compandMode = true;
        }

        //����������������ģʽ
        if (compandMode) {
            // localStorage.setItem('storedCompandCounts', JSON.stringify(""));
            //Ĭ�ϱ�������
            var storedCompandCounts = JSON.parse(localStorage.getItem('storedCompandCounts')) || storedCompandDefault;
            $('.panel-heading:contains("����") .rasdsky').remove();

            var confirmButton = $('<a class="btn btn-xs btn-default" id="confirmButton">ȷ��</a>');
            var cancleButton = $('<a class="btn btn-xs btn-default" id="cancleButton">�˳�</a>');

            // ����ť����һ�� div �У�����ӵ� panel-heading ��
            var buttonContainer = $('<div class="pull-right rasdsky"></div>');
            buttonContainer.append(confirmButton);
            buttonContainer.append(cancleButton);

            $('.panel-heading:contains("����")').append(buttonContainer);
            confirmButton.click(function () {
                $('.col-xs-12.col-sm-4.col-md-3.equip-container').each(function () {
                    var runeName = $(this).find('p:first .equip-name .artifact:nth-child(2)').text().trim(); // ��ȡ�������Ƶĵڶ��� span
                    var runeCount = parseInt($(this).find('p:first .artifact').last().text().trim()); // ��ȡ��������

                    var _inputElement = $(this).find('.rasdsky-input:first');
                    var cnt = parseInt(_inputElement.val());
                    if (storedCompandCounts.hasOwnProperty(runeName)) {
                        if (!cnt.isNaN && cnt != undefined) {
                            storedCompandCounts[runeName] = cnt;
                        }
                    }
                    else {
                        storedCompandCounts[runeName] = 0;
                    }
                    // var storedCount = storedCompandCounts[runeName];
                    // var regexResult = runeName.match(/-(\d+)#/);
                    // if (runeCount > storedCount) {
                    //     var count = runeCount - storedCount;
                    //     count = count - count % 2;
                    //     compandStore(regexResult[1], count);
                    // }
                });
                localStorage.setItem('storedCompandCounts', JSON.stringify(storedCompandCounts)); // �洢�� localStorage 

                //����һ������ģʽ
                autoCompandMode = true;
                localStorage.setItem('lastCompandRuneId', 0);
                showChange();
            });
            cancleButton.click(function () {
                compandMode = false;
                autoCompandMode = false;
                showChange();
            });

            $('.col-xs-12.col-sm-4.col-md-3.equip-container').each(function () {
                var runeName = $(this).find('p:first .equip-name .artifact:nth-child(2)').text().trim(); // ��ȡ�������Ƶĵڶ��� span

                var retainCount = 0;
                if (storedCompandCounts.hasOwnProperty(runeName)) {
                    retainCount = storedCompandCounts[runeName];
                    if (retainCount == undefined || retainCount.isNaN)
                        retainCount = 0;
                }
                $(this).find('.rasdsky').remove();

                var t = ($('<span>').text('  ������').css('color', 'grey'));

                var inputElement = $('<input class="rasdsky-input">').css({
                    "color": "grey",
                    "width": 120,
                    "height": 21,
                });
                inputElement.val(retainCount);
                var p = $('<p class="rasdsky">');
                p.append(t);
                p.append(inputElement);

                $(this).append(p)

            });
        }
        //�鿴�䶯����ģʽ
        else {

            $('.panel-heading:contains("����") .rasdsky').remove();
            var storedRuneCounts = JSON.parse(localStorage.getItem('storedRuneCounts')) || {};
            console.log(storedRuneCounts);
            var storedTime = localStorage.getItem('storedTime');

            // ����ť����һ�� div �У�����ӵ� panel-heading ��
            var buttonContainer2 = $('<div class="pull-right rasdsky"></div>');

            // ����չʾ�洢ʱ���Ԫ��
            var timeDiv = $('<div class="pull-left rasdsky"></div>');
            var timeElement = $('<p>').text('�洢ʱ��: ' + storedTime);
            timeDiv.append(timeElement);
            // �����洢���������İ�ť
            var saveButton = $('<a class="btn btn-xs btn-default" id="saveButton">�洢</a>');
            // �����������ĵİ�ť
            var compandButton = $('<a class="pull-right btn btn-xs btn-default" id="compandButton">����</a>');

            buttonContainer2.append(timeDiv);
            buttonContainer2.append(saveButton);
            buttonContainer2.append(compandButton);

            $('.panel-heading:contains("����")').append(buttonContainer2);

            saveButton.click(function () {
                var storedRuneCounts = {};

                $('.col-xs-12.col-sm-4.col-md-3.equip-container').each(function () {

                    var runeName = $(this).find('p:first .equip-name .artifact:nth-child(2)').text().trim(); // ��ȡ�������Ƶĵڶ��� span

                    var runeCount = parseInt($(this).find('p:first .artifact').last().text().trim()); // ��ȡ��������
                    // �������Ƿ�ɹ�������� NaN ��������С�ڵ��� 20 ���������洢
                    var regexResult = runeName.match(/-(\d+)#/);
                    if (regexResult && parseInt(regexResult[1]) >= 1) {
                        // �������Ƿ�ɹ�������� NaN ����Ϊ 0
                        if (isNaN(runeCount)) {
                            runeCount = 0;
                        }
                        storedRuneCounts[runeName] = runeCount; // �洢��������
                    }
                });

                var currentTime = new Date().toLocaleString(); // ��ȡ��ǰʱ��
                localStorage.setItem('storedRuneCounts', JSON.stringify(storedRuneCounts)); // �洢�� localStorage
                localStorage.setItem('storedTime', currentTime); // �洢ʱ�䵽 localStorage
                alert("�Ѵ洢����", function () { });
            });

            compandButton.click(function () {
                compandMode = true;
                showChange();
            });

            $('.col-xs-12.col-sm-4.col-md-3.equip-container').each(function () {

                var $pElement = $(this).find('p:first'); // ��ȡ��ǰ�����µĵ�һ�� <p> Ԫ��

                var runeName = $(this).find('p:first .equip-name .artifact:nth-child(2)').text().trim(); // ��ȡ�������Ƶĵڶ��� span

                var currentRuneCount = parseInt($(this).find('p:first .artifact').last().text().trim()); // ��ȡ��������

                if (storedRuneCounts.hasOwnProperty(runeName)) {
                    var storedCount = storedRuneCounts[runeName];
                    var changeCount = currentRuneCount - storedCount; // ���������䶯
                    if (changeCount !== undefined) {
                        var changeText = '  (' + storedCount + ' -> ' + currentRuneCount + ')'; // ���ݱ䶯�������ɶ�Ӧ�ı�

                        $(this).find('.rasdsky').remove();
                        // ���䶯����ƴ�ӵ�������Ϣ����󣬲�Ϊ <p> ��ǩ��Ӷ�Ӧ����ʽ
                        $(this).find('p:first .artifact:last').append($('<span class="rasdsky">').text(changeText).css('color', (changeCount > 0) ? 'red' : (changeCount < 0) ? 'green' : 'white')); // Ϊ <p> ��ǩ�����ɫ��ʽ);)
                    }
                }
            });
        }
    }
    showChange();

    // function compandStore(rune, count) {
    //     var t = 1500;
    //     POST_Message("RuneUpgrade", MERGE_Form({
    //         rune: rune,
    //         count: count,
    //     }), "html", t, function (result) {
    //         compandMode = true;
    //         // location.reload();
    //     }, function (request, state, ex) {
    //         // console.log(result)
    //     })
    // }
    function compandStore(rune, count) {
        var data = MERGE_Form({
            rune: rune,
            count: count
        });
        POST_Message("RuneUpgrade", data, "html", 2000)
            .then(r => {
                compandMode = true;
                location.reload();
            })
            .catch(r => { console.log(r) });
    }
}