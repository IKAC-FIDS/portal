// <![CDATA[
(function ($) {
	$.fn.ajaxPost = function (options) {
		var defaults = {
			beforePostHandler: null,
			completeHandler: null,
			errorHandler: null
		};

		var options = $.extend(defaults, options);

		return this.each(function () {
			var $form = $(this);

			var $confirmButton = $('button[type="submit"]', $form);

			$confirmButton.click(function () {
			    $confirmButton.button('loading');

			    if (options.beforePostHandler) {
			        options.beforePostHandler(this);
			    }

			    $form.submit();

			    return false;
			});

			$form.ajaxForm({
				success: function (result) {
					$.processMessage(result);
					$confirmButton.button('reset');

					if (options.completeHandler) {
						options.completeHandler(result);
					}
				},
				error: function () {
				    $confirmButton.button('reset');
				    if (options.errorHandler) {
				        options.errorHandler();
				    }
				}
			});
		});
	};
})(jQuery);
// ]]>