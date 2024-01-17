$(document).ready(function () {
    $.validator.setDefaults({
		highlight: function (element) {
			$(element).closest('.form-group').removeClass('has-success').addClass('has-error');
		},
		unhighlight: function (element) {
		    $(element).closest('.form-group').removeClass('has-error').addClass('has-success');
		},
		errorElement: 'span',
		errorClass: 'help-block',
		errorPlacement: function (error, element) {
		    if (element.is('input:radio')) {
		        error.insertAfter(element.closest('div'));
		    } else if (element.parent('.input-group').length) {
				error.insertAfter(element.parent());
			} else {
				error.insertAfter(element);
			}
		}, success: function (errorElement) {
		    errorElement.remove();
		}
	});
});

$.validator.addMethod("require_from_group", function (value, element, options) {
    var $fields = $(options[1], element.form),
		$fieldsFirst = $fields.eq(0),
		validator = $fieldsFirst.data("valid_req_grp") ? $fieldsFirst.data("valid_req_grp") : $.extend({}, this),
		isValid = $fields.filter(function () {
		    return validator.elementValue(this);
		}).length >= options[0];

    // Store the cloned validator for future validation
    $fieldsFirst.data("valid_req_grp", validator);

    // If element isn't being validated, run each require_from_group field's validation rules
    if (!$(element).data("being_validated")) {
        $fields.data("being_validated", true);
        $fields.each(function () {
            validator.element(this);
        });
        $fields.data("being_validated", false);
    }
    return isValid;
}, $.validator.format("حداقل {0} مورد باید تکملی شود"));

$.validator.addMethod("dateValidate", function (value) {
	if (value == '') return true;
	var items = value.split('/');
	var year = Number(items[0]);
	var month = Number(items[1]);
	var day = Number(items[2]);

	if (day <= 0 || day > 31 || month <= 0 || month > 12 || year <= 0) return false;
	if (month > 6 && day > 30) return false;
	return true;
}, "تاریخ صحیح وارد نمایید.");


$.validator.addMethod("dateRequired", function (value) {
	return (value != '' && value != '____/__/__');
}, "تکمیل این فیلد الزامی است.");


$.validator.addMethod("nationalCodeValidate", function (code) {
	var L = code.length;

	if (L < 8 || parseInt(code, 10) == 0) return false;
	code = ('0000' + code).substr(L + 4 - 10);
	if (parseInt(code.substr(3, 6), 10) == 0) return false;
	var c = parseInt(code.substr(9, 1), 10);
	var s = 0;
	for (var i = 0; i < 9; i++)
		s += parseInt(code.substr(i, 1), 10) * (10 - i);
	s = s % 11;
	return (s < 2 && c == s) || (s >= 2 && c == (11 - s));
}, "کد ملی وارد شده صحیح نمی باشد.");


$.validator.addMethod("exactlength", function (value, element, param) {
	return this.optional(element) || value.length == param;
}, $.validator.format("فقط {0} کاراکتر وارد نمایید."));