(function () {
    alert(1)
    var dataMap = {}//�洢dataid��Ӧװ�������� 

    loadPriceSuffix();
    if (location.href.indexOf("Auction/Query") == -1) return;
    //��ÿ��������Ʒ�����׺
    function loadPriceSuffix() {
        var auctionEquipMap = {};
        //ҳ����ʾ��ÿ����Ʒspan��ǩ
        var container = $(".equip-container .equip-name ");
        $.each(container, (index, item) => {
            var dataid = $(item).attr("data-id");
            auctionEquipMap[dataid] = item;//����data-id�� ������item����ǩ������ĺ�׺
        })
        var equipContainer = $(".equip-content");
        $.each(equipContainer, (index, item) => {
            var dataid = $(item).attr("data-id");
            var e = {};//װ������
            var priceContainer = $(item).find(".equip-price");
            var priceText = priceContainer.text();
            var canDirectBuy = priceText.indexOf("һ�ڼ�") > -1;
            var goldCoin = priceText.match(/(\d+)(?=���)/g);
            var goldCoinPrice;//���
            if (goldCoin != null) {
                goldCoinPrice = goldCoin[0];
            }
            var rune = priceText.match(/(\d+)(?=#)/g);
            var runePriceArr = [];//���ļ�
            var runeCountArr = [];//��������
            if (rune != null) {
                for (let i = 0; i < rune.length; i++) {
                    var runePrice = rune[i];
                    var runeCountText = priceContainer.find(".physical");
                    var runeCount = runeCountText[i].innerText.match(/\d+/g)[0];
                    runePriceArr.push(runePrice);
                    runeCountArr.push(runeCount);
                }

            }
            //��Ҫ������ĺ�׺��span
            var matchSpan = auctionEquipMap[dataid];
            var suffixStr = generatePriceSuffix(goldCoin, goldCoinPrice, runePriceArr, runeCountArr);
            $(matchSpan).parent().append($("<span>", {
                text: suffixStr,
                style: "color:#ff8281"
            }));
            //��ȡװ������
            var eTitle = $(item).find(".equip-title").text().match(/.*(?=\()/g)[0];
            e.eTitle = eTitle;
            e.goldCoinPrice = goldCoinPrice ? goldCoinPrice * 1 : 0;
            e.runePriceArr = runePriceArr;
            e.runeCountArr = runeCountArr;
            dataMap[dataid] = e;
        });
        console.log(dataMap);
    }
    function generatePriceSuffix(goldCoin, goldCoinPrice, runePriceArr, runeCountArr) {
        var str = "";
        str += (goldCoin != null ? goldCoinPrice + "��" : "")
        runePriceArr.forEach((item, index) => {
            var runePrice = item;
            var runeCount = runeCountArr[index];
            str += runePrice + "# *" + runeCount + " ";
        });
        return str;
    }
})();