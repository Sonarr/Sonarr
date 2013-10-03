'use strict';
define(
    [
        'bootstrap'
    ], function () {

        $('body').tooltip({
            selector: '[title]'
        });
    });
