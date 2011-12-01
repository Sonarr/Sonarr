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

});