(function ($) {
    $.fn.extend({
        createAddress: function (data, shaparakFieldId, previousShaparakAddress) {
            var $container = $(this);
            var id = $container.attr('id');
            $container.attr('id', id + 'Container');

            $('#' + id + 'Container').on('keyup keypress', function (e) {
                var keyCode = e.keyCode || e.which;
                if (keyCode === 13) {
                    $("button.btn-add", '#' + id + 'Container').trigger('click');
                    return false;
                }
            });

            $container.append('<div id="AddressComponent">\
                                    <div id="NewAddressLine"></div>\
                                    <ul class="list-inline" style="margin-top: 8px;" id="AddressLineList"></ul>' +
                                    (shaparakFieldId ? '<div class="hidden"><b>آدرس شاپرکی کد شده: </b><p style="display: inline;" id="' + shaparakFieldId + 'Text' + '"></p></div>' : '') +
                              '</div>\
                               <input type="hidden" id="' + id + '" name="' + id + '" />' +
                               (shaparakFieldId ? '<input type="hidden" id="' + shaparakFieldId + '" name="' + shaparakFieldId + '" />' : ''));

            var newAddressLine = '<div class="form-inline">\
                                    <label class="control-label" style="margin-left: 7px;">پیشوند اول</label>' +
                                    createPrefixSelect(1)
                                + '<label class="control-label" style="margin-left: 7px; margin-right: 7px;">پیشوند دوم</label>' +
                                    createPrefixSelect(2)
                                + '<input type="text" id="AdTitle" style="margin-right: 7px;" class="form-control" placeholder="عنوان" maxlength="15" />\
                                    <button class="btn btn-success btn-add">+</button>\
                            </div>';

            $('#NewAddressLine', $container).empty().append(newAddressLine);

            if (previousShaparakAddress) {
                previousShaparakAddress = previousShaparakAddress.replace(/&amp;/g, '&');
                $('#' + shaparakFieldId).val(previousShaparakAddress);
                var splitedShaparakAddressArray = previousShaparakAddress.split('،');
                $.each(splitedShaparakAddressArray, function (i, item) {
                    var splitResult = item.split('&');
                    var address;
                    var shouldAppendCloseButton = splitedShaparakAddressArray.length - 1 === i;
                    var secondPrefixPriority;
                    if (splitResult.length === 3) {
                        address = $('#FirstPrefixId option[value="' + splitResult[0] + '"]', $container).text() + ' ' + $('#SecondPrefixId option[value="' + splitResult[1] + '"]', $container).text() + ' ' + splitResult[2];
                        var firstPrefixPriority = $('#FirstPrefixId option[value="' + splitResult[0] + '"]', $container).data('priority');
                        secondPrefixPriority = $('#SecondPrefixId option[value="' + splitResult[1] + '"]', $container).data('priority');
                        $('#AddressLineList', $container).append('<li data-first-prefix-priority="' + firstPrefixPriority + '" data-second-prefix-priority="' + secondPrefixPriority + '"><div class="chip"><span id="section"></span>' + (shouldAppendCloseButton ? '<i class="fa fa-close remove material-icons btn-remove"></i>' : '') + '<span class="hidden" id="ShaparakAddressText">' + splitedShaparakAddressArray[i] + '</span><span id="AddressText">' + address + '</span></div></li>');
                    } else if (splitResult.length === 2) {
                        secondPrefixPriority = $('#SecondPrefixId option[value="' + splitResult[0] + '"]', $container).data('priority');
                        address = $('#SecondPrefixId option[value="' + splitResult[0] + '"]', $container).text() + ' ' + splitResult[1];
                        $('#AddressLineList', $container).append('<li data-first-prefix-priority="" data-second-prefix-priority="' + secondPrefixPriority + '"><div class="chip"><span id="section"></span>' + (shouldAppendCloseButton ? '<i class="fa fa-close remove material-icons btn-remove"></i>' : '') + '<span class="hidden" id="ShaparakAddressText">' + splitedShaparakAddressArray[i] + '</span><span id="AddressText">' + address + '</span></div></li>');
                    }
                });

                $('#' + id, $container).val(getAddress());
                $('#' + shaparakFieldId).val(getShaparakAddressFormat());
                $('#' + shaparakFieldId + 'Text').empty().append(getShaparakAddressFormat());

                var firstPrefixPriority = $('#AddressComponent ul li', $container).last().data('first-prefix-priority');
                var secondPrefixPriority = $('#AddressComponent ul li', $container).last().data('second-prefix-priority');
                newAddressLine = '<div class="form-inline">\
                                        <label class="control-label" style="margin-left: 7px;">پیشوند اول</label>' +
                    createPrefixSelect(1, firstPrefixPriority) +
                    '<label class="control-label" style="margin-left: 7px; margin-right: 7px;">پیشوند دوم</label>' +
                    createPrefixSelect(2, secondPrefixPriority) +
                    '<input type="text" id="AdTitle" style="margin-right: 7px;" class="form-control" placeholder="عنوان" maxlength="15" />\
                                        <button class="btn btn-success btn-add">+</button>\
                                </div>';
                $('#NewAddressLine', $container).empty().append(newAddressLine);
            }

            $('#AddressComponent', $container).on('click', '.btn-add', function () {
                var $that = $(this);
                var selectedSecondPrefixValue = $that.closest('div.form-inline').find('#SecondPrefixId').val();
                
                var selectedTitle = $that.closest('div.form-inline').find('#AdTitle').val();

                if (!selectedSecondPrefixValue || !selectedTitle) {
                    alert('پیشوند دوم و عنوان را وارد نمایید.');
                    return false;
                }

                var selectedSecondPrefixText = $that.closest('div.form-inline').find('#SecondPrefixId option:selected').text();
                var selectedFirstPrefixPriority = $that.closest('div.form-inline').find('#FirstPrefixId option:selected').data('priority');
                var selectedFirstPrefixId = $that.closest('div.form-inline').find('#FirstPrefixId').val();
                var selectedSecondPrefixPriority = $that.closest('div.form-inline').find('#SecondPrefixId option:selected').data('priority');
                var selectedSecondPrefixId = $that.closest('div.form-inline').find('#SecondPrefixId').val();

                var address;
                var shaparakAddress;
                if ($that.closest('div.form-inline').find('#FirstPrefixId option:selected').val()) {
                    address = $that.closest('div.form-inline').find('#FirstPrefixId option:selected').text() + ' ' + selectedSecondPrefixText + ' ' + selectedTitle;
                    shaparakAddress = selectedFirstPrefixId + '&' + selectedSecondPrefixId + '&' + selectedTitle;
                } else {
                    address = selectedSecondPrefixText + ' ' + selectedTitle;
                    shaparakAddress = selectedSecondPrefixId + '&' + selectedTitle;
                }

                $('#AddressLineList li', $container).each(function () {
                    $(this).find('i.fa').remove();
                });

                $('#AddressLineList', $container).append('<li data-first-prefix-priority="' + selectedFirstPrefixPriority + '" data-second-prefix-priority="' + selectedSecondPrefixPriority + '"><div class="chip"><span id="section"></span><i class="fa fa-close remove material-icons btn-remove"></i><span class="hidden" id="ShaparakAddressText">' + shaparakAddress + '</span><span id="AddressText">' + address + '</span></div></li>');

                var newAddressLine = '<div class="form-inline">\
                                        <label class="control-label" style="margin-left: 7px;">پیشوند اول</label>' +
                                            createPrefixSelect(1, selectedFirstPrefixPriority) +
                                        '<label class="control-label" style="margin-left: 7px; margin-right: 7px;">پیشوند دوم</label>' +
                                            createPrefixSelect(2, selectedSecondPrefixPriority) +
                                       '<input type="text" id="AdTitle" style="margin-right: 7px;" class="form-control" placeholder="عنوان" maxlength="15" />\
                                        <button class="btn btn-success btn-add">+</button>\
                                </div>';

                $('#NewAddressLine', $container).empty().append(newAddressLine);
                $('#' + id, $container).val(getAddress());
                if (shaparakFieldId) {
                    $('#' + shaparakFieldId).val(getShaparakAddressFormat());
                    $('#' + shaparakFieldId + 'Text').empty().append(getShaparakAddressFormat());
                }
                return true;
            });

            $('#AddressComponent', $container).on('click', '.btn-remove', function () {
                $(this).closest('li').remove();
                $('#AddressComponent ul li', $container).last().find('#section').html('<i class="fa fa-close remove material-icons btn-remove"></i>');
                $('#' + id, $container).val(getAddress());
                $('#' + shaparakFieldId + 'Text').empty().append(getShaparakAddressFormat());

                if (shaparakFieldId) {
                    $('#' + shaparakFieldId).val(getShaparakAddressFormat());
                }

                var firstPrefixPriority = $('#AddressComponent ul li', $container).last().data('first-prefix-priority');
                var secondPrefixPriority = $('#AddressComponent ul li', $container).last().data('second-prefix-priority');
                var newAddressLine = '<div class="form-inline">\
                                        <label class="control-label" style="margin-left: 7px;">پیشوند اول</label>' +
                                            createPrefixSelect(1, firstPrefixPriority) +
                                        '<label class="control-label" style="margin-left: 7px; margin-right: 7px;">پیشوند دوم</label>' +
                                            createPrefixSelect(2, secondPrefixPriority) +
                                       '<input type="text" id="AdTitle" style="margin-right: 7px;" class="form-control" placeholder="عنوان" maxlength="15" />\
                                        <button class="btn btn-success btn-add">+</button>\
                                </div>';

                $('#NewAddressLine', $container).empty().append(newAddressLine);
            });

            function getAddress() {
                var addressArray = [];
                $('#AddressComponent ul li', $container).map(function (index, item) {
                    addressArray.push($(item).find('#AddressText').text());
                }).get();

                return addressArray.join('،');
            }

            function getShaparakAddressFormat() {
                var shaparakAddressArray = [];
                $('#AddressComponent ul li', $container).map(function (index, item) {
                    shaparakAddressArray.push($(item).find('#ShaparakAddressText').text());
                }).get();
                
                return shaparakAddressArray.join('،');
            }

            function createPrefixSelect(prefixTypeCode, lastPriority) {
                var newdata = $.grep(data, function (element) {
                    return element.PrefixTypeCode === prefixTypeCode;
                });

                if (lastPriority) {
                    newdata = $.grep(newdata, function (element) {
                        return element.PriorityNumber >= lastPriority;
                    });
                }

                var selectTagId = prefixTypeCode === 1 ? 'FirstPrefixId' : 'SecondPrefixId';
                var selectTag = '<select class="form-control" id="' + selectTagId + '"><option data-priority="" value="">انتخاب نمایید</option>';
                $.each(newdata, function (index, item) {
                    selectTag += '<option data-priority="' + item.PriorityNumber + '" value="' + item.Id + '">' + item.Title + '</option>';
                });
                selectTag += '</select>';

                return selectTag;
            }
        }
    });
})(jQuery);