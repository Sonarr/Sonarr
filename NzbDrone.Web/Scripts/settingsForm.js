$(document).ready(function () {
    var options = {
        target: '#result',
        beforeSubmit: showRequest,
        success: showResponse,
        type: 'post',
        resetForm: false
    };
    $('#form').ajaxForm(options);
});

function showRequest(formData, jqForm, options) {
    $("#result").empty().html('Saving...');
    $("#form :input").attr("disabled", true);
    $('#saveAjax').show();
}

function showResponse(responseText, statusText, xhr, $form) {
    $("#result").empty().html(responseText);
    $("#form :input").attr("disabled", false);
    $('#saveAjax').hide();
}