(function ($) {
    $.fn.enableCheckboxRangeSelection = function () {
        var lastCheckbox = null;
        var $spec = this;
        $spec.unbind("click.checkboxrange");
        $spec.bind("click.checkboxrange", function (e) {
            if (lastCheckbox != null && (e.shiftKey || e.metaKey)) {
                $spec.slice(
                  Math.min($spec.index(lastCheckbox), $spec.index(e.target)),
                  Math.max($spec.index(lastCheckbox), $spec.index(e.target)) + 1
                ).prop('checked', e.target.checked);
            }
            lastCheckbox = e.targety()
        });
    };
})(jQuery);

$(document).ready(function () {
    while($('#logo span').height() > $('#logo').height()) {
        $('#logo span').css('font-size', (parseInt($('#logo span').css('font-size')) - 1) + "px" );
    };
});