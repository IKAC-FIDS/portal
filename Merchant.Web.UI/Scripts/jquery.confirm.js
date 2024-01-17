// <![CDATA[
(function ($) {
    $.showConfirm = function (options) {
        var defaults = {
            caption: 'تائيد عمليات',
            body: 'آيا عمليات درخواستي اجرا شود؟',
            onConfirm: null,
            onCancel: null,
            confirmText: 'تائيد',
            closeText: 'انصراف'
        };

        options = $.extend(defaults, options);
        var confirmContainer = "#confirmContainer";
        var html = '<div class="modal fade" id="confirmContainer">' +
                   '<div class="modal-dialog"><div class="modal-content">' +
                   '<div class="modal-body">'
                        + options.body + '</div>' +
                   '<div class="modal-footer"> <div class="pull-left">'
                        + '<button type="button" class="btn btn-primary" id="confirmBtn">' + options.confirmText + '</button>'
                    + '<button type="button" class="btn btn-default" id="cancelBtn" data-dismiss="modal">' + options.closeText + '</button></div></div></div></div></div>';

        $(confirmContainer).remove();
        $(html).appendTo('body');
        $(confirmContainer).modal({
            backdrop: 'static',
            keyboard: false
        });
        $(confirmContainer).modal('show');

        $('#confirmBtn', confirmContainer).click(function () {
            if (options.onConfirm)
                options.onConfirm();
            $(confirmContainer).modal('hide');
        });

        $('#cancelBtn', confirmContainer).click(function () {
            if (options.onCancel)
                options.onCancel();
            $(confirmContainer).modal('hide');
        });
    };

    $.showDialog = function (options) {
        var defaults = {
            body: '',
            onClose: null,
            closeText: 'بستن'
        };

        options = $.extend(defaults, options);
        var messageContainer = "#messageContainer";
        var html = '<div class="modal fade" id="messageContainer">' +
                   '<div class="modal-dialog"><div class="modal-content">' +
                   '<div class="modal-body">'
                        + options.body + '</div>' +
                   '<div class="modal-footer"> <div class="pull-left"><button type="button" class="btn btn-primary" id="closeBtn">' + options.closeText + '</button></div></div></div></div></div>';

        $(messageContainer).remove();
        $(html).appendTo('body');
        $(messageContainer).modal('show');

        $(messageContainer).on('hidden.bs.modal', function () {
            if (options.onClose)
                options.onClose();
        });

        $('#closeBtn', messageContainer).click(function () {
            if (options.onConfirm)
                options.onConfirm();
            $(messageContainer).modal('hide');
        });
    };

    $.showImportantConfirm = function (options) {
        var defaults = {
            caption: 'تائيد عمليات',
            body: 'آيا عمليات درخواستي اجرا شود؟',
            onConfirm: null,
            onCancel: null,
            confirmText: 'تائيد',
            closeText: 'انصراف'
        };

        options = $.extend(defaults, options);
        var confirmContainer = "#confirmContainer";
        var html = '<div class="modal fade" id="confirmContainer">' +
            '<div class="modal-dialog"><div class="modal-content">' +
            '<div class="modal-body">'
            + options.body + '<hr /> <div class="checkbox"><label><input id="ReadAndAccept" type="checkbox">اطلاعات فوق را بررسی کرده و تایید می نمایم</label></div> </div>' +
            '<div class="modal-footer"> <div class="pull-left">'
            + '<button type="button" class="btn btn-primary" id="confirmBtn" disabled>' + options.confirmText + '</button>'
            + '<button type="button" class="btn btn-default" id="cancelBtn" data-dismiss="modal">' + options.closeText + '</button></div></div></div></div></div> <script type="text/javascript">$("#ReadAndAccept").change(function () {$("#confirmBtn").prop("disabled", !this.checked);});</script>';

        $(confirmContainer).remove();
        $(html).appendTo('body');
        $(confirmContainer).modal({
            backdrop: 'static',
            keyboard: false
        });
        $(confirmContainer).modal('show');

        $('#confirmBtn', confirmContainer).click(function () {
            if (options.onConfirm)
                options.onConfirm();
            $(confirmContainer).modal('hide');
        });

        $('#cancelBtn', confirmContainer).click(function () {
            if (options.onCancel)
                options.onCancel();
            $(confirmContainer).modal('hide');
        });
    };

})(jQuery);
// ]]>