'use strict';
define(
    [
        'bootstrap'
    ], function () {
        $(document).on('mouseenter', '[title]', function () {
            $(this).tooltip('show');
        });
    });
