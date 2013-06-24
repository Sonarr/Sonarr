'use strict';

define(
    [
        'marionette'
    ], function (Marionette) {
        return Marionette.ItemView.extend({
            template: 'Calendar/UpcomingItemTemplate',
            tagName : 'div'
        });
    });
