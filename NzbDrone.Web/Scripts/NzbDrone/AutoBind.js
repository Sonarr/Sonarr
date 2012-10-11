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
    
    $('[data-status]').livequery(function () {
        var status = $(this).attr('data-status');
        
        $(this).removeClass(function (index, css) {
            return (css.match(/\bicon-\S+/g) || []).join(' ');
        });

        if (status == 'Downloading') {
            $(this).addClass('icon-download-alt');
        }
        
        if (status == 'Ready') {
            $(this).addClass('icon-play');
        }
        
        if (status == 'AirsToday') {
            $(this).addClass('icon-time');
        }
        
        if (status == 'NotAired') {
            $(this).addClass('icon-calendar');
        }

        if (status == 'Missing') {
            $(this).addClass('icon-sign-blank');
        }
    });
});