(function ($) {
    $.closeModal = function (modalResult, data) {
        var $modalElement = $('#modalContainer');

        var closeEvent = $.Event('close');
        $modalElement.trigger(closeEvent, [modalResult, data]);
        if (closeEvent.isDefaultPrevented()) {
            return false;
        }

        $modalElement.modal('hide');

        return this;
    };

    $.showModal = function (options) {
        var defaults = {
            url: null,
            data: null,
            modalSize: 'normal',
            onClose: null
        };

        options = $.extend(defaults, options);

        var $mainContainer = $('<div class="modal fade in" id="modalContainer" tabindex="-1"><div class="modal-dialog modal-' + $.trim(options.modalSize) + '"><div id="modalContent" class="modal-content"></div></div></div>');

        $('#modalContainer').remove();
        $('.modal-backdrop').remove();

        $mainContainer.appendTo('body');
        $('#modalContainer').modal({ keyboard: true }, 'show');
        $('#modalContent').html('<div class="modal-loading"><i class="fa fa-spin fa-cog"></i> <p>در حال بارگذاری اطلاعات...</p></div>');

        $.ajax({
            type: 'GET',
            url: options.url,
            cache: false,
            data: options.data,
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            error: function () {
                toastr.error('متاسفانه خطایی رخ داده است.');
                $mainContainer.modal('hide');
            },
            success: function (data) {
                $('#modalContent').html(data);

                //if (data.success) {
                //    $('#modalContent').html(data);
                //} else {
                //    $('#modalContainer').remove();
                //    $('.modal-backdrop').remove();

                //    if (data.authenticationIsRequired) {
                //        toastr.warning('متاسفانه شما دسترسی لازم را ندارید..');
                //    } else {
                //        debugger;
                //        toastr.error('متاسفانه خطایی رخ داده است.');
                //    }
                //}
            }
        });

        if (options.onClose != null) {
            $mainContainer.on('close.modal', function (e, formResult, data) {
                options.onClose(formResult, data);
            });
        }
    };
})(jQuery);
