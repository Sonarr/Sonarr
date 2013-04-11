'use strict';

define([
    'app',
    'Upcoming/UpcomingCollection'

], function () {
    NzbDrone.Upcoming.UpcomingItemView = Backbone.Marionette.ItemView.extend({
        template: 'Upcoming/UpcomingItemTemplate',
        tagName : 'tr',
    });
});