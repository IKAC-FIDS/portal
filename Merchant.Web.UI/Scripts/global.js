function isValidNationalCode(nationalCode) {
    nationalCode = $.trim(nationalCode);

    if (nationalCode === '')
        return false;

    if (nationalCode.length != 10)
        return false;

    if (isNaN(parseInt(nationalCode, 10)))
        return false;

    var allDigitEqual = ["0000000000", "1111111111", "2222222222", "3333333333", "4444444444", "5555555555", "6666666666", "7777777777", "8888888888", "9999999999"];
    if (allDigitEqual.indexOf(nationalCode) >= 0) return false;

    var num0 = parseInt(nationalCode[0]) * 10;
    var num2 = parseInt(nationalCode[1]) * 9;
    var num3 = parseInt(nationalCode[2]) * 8;
    var num4 = parseInt(nationalCode[3]) * 7;
    var num5 = parseInt(nationalCode[4]) * 6;
    var num6 = parseInt(nationalCode[5]) * 5;
    var num7 = parseInt(nationalCode[6]) * 4;
    var num8 = parseInt(nationalCode[7]) * 3;
    var num9 = parseInt(nationalCode[8]) * 2;
    var a = parseInt(nationalCode[9]);

    var b = (((((((num0 + num2) + num3) + num4) + num5) + num6) + num7) + num8) + num9;
    var c = b % 11;

    return (((c < 2) && (a == c)) || ((c >= 2) && ((11 - c) == a)));
}

function convertToPersianNumbers(number) {
    var result = String(number).replace(/0/g, '۰')
        .replace(/1/g, '۱')
        .replace(/2/g, '۲')
        .replace(/3/g, '۳')
        .replace(/4/g, '۴')
        .replace(/5/g, '۵')
        .replace(/6/g, '۶')
        .replace(/7/g, '۷')
        .replace(/8/g, '۸')
        .replace(/9/g, '۹');

    return result;
};

function convertToLatinNumbers(number) {
    var result = String(number).replace(/۰/g, '0')
        .replace(/۱/g, '1')
        .replace(/۲/g, '2')
        .replace(/۳/g, '3')
        .replace(/۴/g, '4')
        .replace(/۵/g, '5')
        .replace(/۶/g, '6')
        .replace(/۷/g, '7')
        .replace(/۸/g, '8')
        .replace(/۹/g, '9');

    return result;
};

$.fn.digits = function () {
    return this.each(function () {
        $(this).text($(this).text().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,"));
    });
}

function convertToPersianNumbersThreeDigitsSeparated(number) {
    
    console.log('ccccccccc2ccccc',number,Math.abs(number % 1 ) )
    var negative = false ;
    if(number < 0 )
        negative = true;
    if(Math.abs(number % 1 ) >= 0.5)
    {
      
        number = Math.ceil(Math.abs(number))
    }
    else {
        
        number = Math.floor(Math.abs(number))
        
    }
    if(negative)
    {
        number = - number;
    }
     
    if(number ==  null )
        return "";
    var result = String(number).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,")
        .replace(/0/g, '۰')
        .replace(/1/g, '۱')
        .replace(/2/g, '۲')
        .replace(/3/g, '۳')
        .replace(/4/g, '۴')
        .replace(/5/g, '۵')
        .replace(/6/g, '۶')
        .replace(/7/g, '۷')
        .replace(/8/g, '۸')
        .replace(/9/g, '۹');

    return result;
}

function toPersianDate(value) {
    var date = new Date(value);
    var persianDate = Intl.DateTimeFormat("fa", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit"
    }).format(date);

    return persianDate;
}

function downloadUrl(url, data) {
    url = url + (data ? ('?' + jQuery.param(data)) : '');
    var $frame = $("<iframe id='downloadFrame' src='" + url + "' name='printIframe' />");
    $frame.css('display', 'none');
    $frame.appendTo("body");
    setTimeout(function () { $frame.remove(); }, 15000);
};