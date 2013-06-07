'use strict';

define([
    'app',
    'Calendar/UpcomingCollection'

], function () {
    NzbDrone.Calendar.UpcomingItemView = Backbone.Marionette.ItemView.extend({
        template : 'Calendar/UpcomingItemTemplate',
        tagName  : 'div'
    });
});