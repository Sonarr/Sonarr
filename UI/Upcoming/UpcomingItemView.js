'use strict';

define([
        'app',
        'Upcoming/UpcomingCollection'

], function () {
    NzbDrone.Upcoming.UpcomingItemView = Backbone.Marionette.ItemView.extend({
        template: 'Upcoming/UpcomingItemTemplate',
        tagName: 'tr',

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
        }
    })
})