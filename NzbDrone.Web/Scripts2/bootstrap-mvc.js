
jQuery(document).ready(function ($) {

    $('span.field-validation-valid, span.field-validation-error').each(function () {
        $(this).addClass('help-inline');
    });

    $("button, input[type='submit'],input[type='button']").each(function () {
        $(this).addClass('btn btn-madmin');
    });
    
    $(".btn").each(function () {
        $(this).addClass('btn-madmin');
    });

    $("input[type='submit']").each(function () {
        $(this).addClass('btn-primary btn-large');
    });

    $('.validation-summary-errors').livequery(function () {
        $(this).addClass('alert alert-error');
    });

    $('.field-validation-error').livequery(function () {
        $('.control-group').each(function () {
            if ($(this).find('span.field-validation-error').length == 0) {
                $(this).removeClass('error');
            } else {
                $(this).addClass('error');
            }
        });
    });
});