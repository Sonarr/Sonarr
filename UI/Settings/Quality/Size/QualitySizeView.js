'use strict';

define([
    'app',
    'Quality/QualitySizeCollection'

], function () {

    NzbDrone.Settings.Quality.Size.QualitySizeView = Backbone.Marionette.ItemView.extend({
        template : 'Settings/Quality/Size/QualitySizeTemplate',
        className: 'quality-size-item',

        ui: {
            slider          : '.slider',
            thirtyMinuteSize: '.thirty-minute-size',
            sixtyMinuteSize : '.sixty-minute-size'
        },

        events: {
            'slide .slider': 'slide'
        },

        initialize: function (options) {
            this.qualityProfileCollection = options.qualityProfiles;
        },

        onRender: function () {
            var self = this;
            this.ui.slider.slider({
                min    : 0,
                max    : 200,
                step   : 1,
                value  : self.model.get('maxSize'),
                tooltip: 'hide'
            });
        },

        slide: function (e) {
            var newSize = e.value;

            this.model.set({ maxSize: newSize, thirtyMinuteSize: newSize * 30, sixtyMinuteSize: newSize * 60 });

            this.ui.thirtyMinuteSize.html(newSize * 30);
            this.ui.sixtyMinuteSize.html(newSize * 60);
        }
    });
});
