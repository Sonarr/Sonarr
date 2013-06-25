'use strict';

define(['marionette', 'Mixins/AsModelBoundView', 'jquery.knob'], function (Marionette, AsModelBoundView) {

    var view = Marionette.ItemView.extend({
        template : 'Settings/Quality/Size/QualitySizeTemplate',
        tagName  : 'li',

        ui: {
            knob            : '.x-knob',
            thirtyMinuteSize: '.thirty-minute-size',
            sixtyMinuteSize : '.sixty-minute-size'
        },

        events: {
            'change .x-knob': '_changeMaxSize'
        },

        initialize: function (options) {
            this.qualityProfileCollection = options.qualityProfiles;
        },

        onRender: function () {
            this.ui.knob.knob({
                min         : 0,
                max         : 200,
                step        : 10,
                cursor      : 25,
                width       : 100,
                stopper     : true
            });
        },

        _changeMaxSize: function (e, value) {
            this.model.set({
                maxSize: value
            });

            this.ui.thirtyMinuteSize.html(value * 30);
            this.ui.sixtyMinuteSize.html(value * 60);
        }
    });

    return AsModelBoundView.call(view);
});
