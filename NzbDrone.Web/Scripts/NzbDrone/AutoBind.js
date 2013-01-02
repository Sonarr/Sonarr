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
        
        if ($(this).hasClass('jquery-accordion-collapse-all'))
            $(this).accordion("activate", false);
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
    
    $('.jQuery-datepicker').livequery(function () {
        $(this).datepicker({
            dateFormat: "yy-mm-dd"
        });
    });
    
    $('[data-status="Downloading"]').livequery(function () {
        $(this).removeClass(function (index, css) {
            return (css.match(/\bicon-\S+/g) || []).join(' ');
        });

        $(this).addClass('icon-download-alt');
    });

    $('[data-status="Ready"]').livequery(function() {
        $(this).removeClass(function (index, css) {
            return (css.match(/\bicon-\S+/g) || []).join(' ');
        });

        $(this).addClass('icon-play');
        
    });
    
    $('[data-status="AirsToday"]').livequery(function () {
        $(this).removeClass(function (index, css) {
            return (css.match(/\bicon-\S+/g) || []).join(' ');
        });

        $(this).addClass('icon-time');

    });
    
    $('[data-status="NotAired"]').livequery(function () {
        $(this).removeClass(function (index, css) {
            return (css.match(/\bicon-\S+/g) || []).join(' ');
        });

        $(this).addClass('icon-calendar');

    });
    
    $('[data-status="Missing"]').livequery(function () {
        $(this).removeClass(function (index, css) {
            return (css.match(/\bicon-\S+/g) || []).join(' ');
        });

        $(this).addClass('icon-sign-blank');

    });
    
    $('.infoBox, .successBox, .warningBox, .errorBox, .validationBox').livequery(function () {
        $(this).prepend('<i class="icon-certificate"></i>');
    });
});