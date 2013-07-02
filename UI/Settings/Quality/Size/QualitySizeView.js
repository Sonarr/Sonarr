'use strict';

define(['marionette', 'Mixins/AsModelBoundView', 'jquery.knob'], function (Marionette, AsModelBoundView) {

    var view = Marionette.ItemView.extend({
        template : 'Settings/Quality/Size/QualitySizeTemplate',
        tagName  : 'li',

        ui: {
            knob            : '.x-knob',
            thirtyMinuteSize: '.x-size-thirty',
            sixtyMinuteSize : '.x-size-sixty'
        },

        events: {
            'change .x-knob': '_changeMaxSize'
        },

        initialize: function (options) {
            this.qualityProfileCollection = options.qualityProfiles;
        },

        onRender: function () {
            this.ui.knob.knob({
                min          : 0,
                max          : 200,
                step         : 1,
                cursor       : 25,
                width        : 150,
                stopper      : true,
                displayInput : false
            });
        },

        _changeMaxSize: function () {
            var maxSize = this.model.get('maxSize');
            var bytes = maxSize * 1024 * 1024;
            var thirty = (bytes * 30).bytes(1);
            var sixty = (bytes * 60).bytes(1);

            if (parseInt(maxSize) === 0) {
                thirty = 'No Limit';
                sixty = 'No Limit';
            }

            this.ui.thirtyMinuteSize.html(thirty);
            this.ui.sixtyMinuteSize.html(sixty);
        }
    });

    return AsModelBoundView.call(view);
});
