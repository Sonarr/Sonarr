$(document).ready(function () {
    var options = {
        type: 'post',
        resetForm: false
    };
    $('#form').ajaxForm(options);
    $('#save_button').removeAttr('disabled');
});