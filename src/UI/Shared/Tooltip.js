'use strict';
define(
    [
        'jquery'
    ], function ($) {
        return {

            appInitializer: function () {
                console.log('starting signalR');

                $('body').tooltip({
                    selector: '[title]'
                });

                return this;
            }
        };
    });
