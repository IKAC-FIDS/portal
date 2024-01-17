// <![CDATA[
(function ($) {
    toastr.options = {
        "closeButton": false,
        "debug": false,
        "positionClass": "toast-bottom-full-width",
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "0",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut",
        "progressBar": true
    };

    $.processMessage = function (appResult) {
        if (appResult.messages && appResult.messages.length > 0) {
            for (var i = 0; i < appResult.messages.length; i++) {
                var msg = appResult.messages[i];
                for (var j = 0; j < msg.messages.length; j++) {
                    switch (msg.messageType) {
                        case 0:
                            toastr.success(msg.messages[j], msg.title);
                            break;
                        case 1:
                            toastr.info(msg.messages[j], msg.title);
                            break;
                        case 2:
                            toastr.warning(msg.messages[j], msg.title);
                            break;
                        case 3:
                            toastr.options.timeOut = 0;
                            toastr.options.extendedTimeOut = 0;
                            toastr.error(msg.messages[j], msg.title);
                            break;
                    }
                }
            }
		}
    };
})(jQuery);
// ]]>