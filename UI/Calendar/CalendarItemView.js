'use strict';

define([
    'app',
    'Calendar/CalendarCollection'

], function () {
    NzbDrone.Calendar.CalendarItemView = Backbone.Marionette.ItemView.extend({
        template : 'Calendar/CalendarItemTemplate',
        tagName  : 'div',

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
        }
    });
});