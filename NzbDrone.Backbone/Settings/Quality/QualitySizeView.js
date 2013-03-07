'use strict';

define([
        'app',
        'Quality/QualitySizeCollection'

], function () {

    NzbDrone.Settings.Quality.QualitySizeView = Backbone.Marionette.ItemView.extend({
        template: 'Settings/Quality/QualitySizeTemplate',

        ui: {
            slider: '.slider',
            thirtyMinuteSize: '.thirty-minute-size',
            sixtyMinuteSize: '.sixty-minute-size'
        },

        events: {
            'slide .slider': 'slide'
        },

        initialize: function (options) {
            this.qualityProfileCollection = options.qualityProfiles;
        },

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);

            var self = this;
            this.ui.slider.slider({
                min: 0,
                max: 200,
                step: 1,
                value: self.model.get('maxSize'),
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
