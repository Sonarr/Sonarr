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

            var container = element.parents('.modal-body');
            if (container.length === 0) {
                container = 'body';
            }

            element.tooltip({
                container: container
            });

            element.tooltip('show');
        });
    });
