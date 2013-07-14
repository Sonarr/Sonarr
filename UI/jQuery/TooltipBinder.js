'use strict';
define(
    [
        'bootstrap'
    ], function () {
        $(document).on('mouseenter', '[title]', function () {

            var element = $(this);

            if (!element.attr('data-placement') && element.parents('.control-group').length > 0) {
                element.attr('data-placement', 'right');
            }

            element.tooltip('show');
        });
    });
