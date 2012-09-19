$(document).ready(function () {

    //All forms are ajax forms
    $("form").livequery(function () {

        var options = {
            type: 'post',
            resetForm: false
        };

        $(this).ajaxForm(options);

    });

    $('Form button').livequery(function () {
        $(this).removeAttr('disabled');
    });


    //All buttons are jQueryUI buttons
    $('button, input[type="button"], input[type="submit"], input[type="reset"]').livequery(function () {
        $(this).button();
    });

    $('.jquery-accordion').livequery(function () {
        $(this).accordion({
            autoHeight: false,
            collapsible: true
        });
    });

    $('.jquery-tabs').livequery(function () {
        $(this).show();

        $(this).tabs({
            fx: { opacity: 'toggle' },
            select: function (event, ui) {
                jQuery(this).css('height', jQuery(this).height());
                jQuery(this).css('overflow', 'hidden');
            },
            show: function (event, ui) {
                jQuery(this).css('height', 'auto');
                jQuery(this).css('overflow', 'visible');
            }
        });
    });
    
    $('.jQuery-dateTime').livequery(function () {
        $(this).datepicker({
            dateFormat: "yy-mm-dd"
        });
    });
});