'use strict';

define(
    [
        'marionette',
        'Mixins/AsModelBoundView',
        'filesize',
        'jquery-ui'
    ], function (Marionette, AsModelBoundView, fileSize) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Quality/Definition/QualityDefinitionTemplate',
            className: 'row',

            ui: {
                title              : '.x-title',
                sizeSlider         : '.x-slider',
                thirtyMinuteMinSize: '.x-min-thirty',
                sixtyMinuteMinSize : '.x-min-sixty',
                thirtyMinuteMaxSize: '.x-max-thirty',
                sixtyMinuteMaxSize : '.x-max-sixty'
            },

            events: {
                'change .x-title': '_updateTitle',
                'slide .x-slider': '_updateSize'
            },

            initialize: function (options) {
                this.qualityProfileCollection = options.qualityProfiles;
                this.filesize = fileSize;
            },

            onRender: function () {
                this.ui.sizeSlider.slider({
                    range       : true,
                    min         : 0,
                    max         : 200,
                    values      : [ this.model.get('minSize'), this.model.get('maxSize') ],
                });     
                
                this._changeSize();     
            },
            
            _updateTitle: function() {
                this.model.set('title', this.ui.title.val());
            },

            _updateSize: function (event, ui) {
                this.model.set('minSize', ui.values[0]);
                this.model.set('maxSize', ui.values[1]);
                
                this._changeSize();
            },
            
            _changeSize: function () {
                var minSize = this.model.get('minSize');
                var maxSize = this.model.get('maxSize');

                {
                    var minBytes = minSize * 1024 * 1024;
                    var minThirty = fileSize(minBytes * 30, 1, false);
                    var minSixty = fileSize(minBytes * 60, 1, false);
                    
                    this.ui.thirtyMinuteMinSize.html(minThirty);
                    this.ui.sixtyMinuteMinSize.html(minSixty);
                }
                
                {
                    var maxBytes = maxSize * 1024 * 1024;
                    var maxThirty = fileSize(maxBytes * 30, 1, false);
                    var maxSixty = fileSize(maxBytes * 60, 1, false);
                    
                    this.ui.thirtyMinuteMaxSize.html(maxThirty);
                    this.ui.sixtyMinuteMaxSize.html(maxSixty);
                }
                
                /*if (parseInt(maxSize, 10) === 0) {
                    thirty = 'No Limit';
                    sixty = 'No Limit';
                }*/
            }
        });

        return AsModelBoundView.call(view);
    });
