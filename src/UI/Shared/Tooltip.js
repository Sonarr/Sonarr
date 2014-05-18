'use strict';
define(
    [
        'jquery'
    ], function ($) {
        return {

            appInitializer: function () {
                console.log('starting signalR');

                $('body').tooltip({
                    selector: '[title]',
                    container: 'body'
                });

                $(document).click(function(e) {

                    if ($(e.target).attr('title') !== undefined) {
                        return;
                    }

                    $('.tooltip').remove();
                });

                return this;
            }
        };
    });
