var Marionette = require('marionette');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
require('jquery-ui');
var FormatHelpers = require('../../../Shared/FormatHelpers');

var view = Marionette.ItemView.extend({
    template  : 'Settings/Quality/Definition/QualityDefinitionItemViewTemplate',
    className : 'row',
    
    slider    : {
        min  : 0,
        max  : 200,
        step : 0.1
    },

    ui : {
        sizeSlider          : '.x-slider',
        thirtyMinuteMinSize : '.x-min-thirty',
        sixtyMinuteMinSize  : '.x-min-sixty',
        thirtyMinuteMaxSize : '.x-max-thirty',
        sixtyMinuteMaxSize  : '.x-max-sixty'
    },

    events : {
        'slide .x-slider' : '_updateSize'
    },

    initialize : function(options) {
        this.profileCollection = options.profiles;
    },

    onRender : function() {
        if (this.model.get('quality').id === 0) {
            this.$el.addClass('row advanced-setting');
        }

        this.ui.sizeSlider.slider({
            range  : true,
            min    : this.slider.min,
            max    : this.slider.max,
            step   : this.slider.step,
            values : [
                this.model.get('minSize') || this.slider.min,
                this.model.get('maxSize') || this.slider.max
            ]
        });

        this._changeSize();
    },

    _updateSize : function(event, ui) {
        var minSize = ui.values[0];
        var maxSize = ui.values[1];
    
        if (maxSize === this.slider.max) {
            maxSize = null;
        }
    
        this.model.set('minSize', minSize);
        this.model.set('maxSize', maxSize);

        this._changeSize();
    },

    _changeSize : function() {
        var minSize = this.model.get('minSize') || this.slider.min;
        var maxSize = this.model.get('maxSize') || null;
        {
            var minBytes = minSize * 1024 * 1024;
            var minThirty = FormatHelpers.bytes(minBytes * 30, 2);
            var minSixty = FormatHelpers.bytes(minBytes * 60, 2);

            this.ui.thirtyMinuteMinSize.html(minThirty);
            this.ui.sixtyMinuteMinSize.html(minSixty);
        }

        {
            if (maxSize === 0 || maxSize === null) {
                this.ui.thirtyMinuteMaxSize.html('Unlimited');
                this.ui.sixtyMinuteMaxSize.html('Unlimited');
            } else {
                var maxBytes = maxSize * 1024 * 1024;
                var maxThirty = FormatHelpers.bytes(maxBytes * 30, 2);
                var maxSixty = FormatHelpers.bytes(maxBytes * 60, 2);

                this.ui.thirtyMinuteMaxSize.html(maxThirty);
                this.ui.sixtyMinuteMaxSize.html(maxSixty);
            }
        }
    }
});

view = AsModelBoundView.call(view);

module.exports = view;