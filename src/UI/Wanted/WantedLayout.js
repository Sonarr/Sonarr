'use strict';
define(
    [
        'marionette',
        'backbone',
        'backgrid',
        'Wanted/Missing/MissingLayout',
        'Wanted/Cutoff/CutoffUnmetLayout'
    ], function (Marionette, Backbone, Backgrid, MissingLayout, CutoffUnmetLayout) {
        return Marionette.Layout.extend({
            template: 'Wanted/WantedLayoutTemplate',

            regions: {
                content      : '#content'
                //missing    : '#missing',
                //cutoff     : '#cutoff'
            },

            ui: {
                missingTab : '.x-missing-tab',
                cutoffTab  : '.x-cutoff-tab'
            },

            events: {
                'click .x-missing-tab' : '_showMissing',
                'click .x-cutoff-tab'  : '_showCutoffUnmet'
            },

            initialize: function (options) {
                if (options.action) {
                    this.action = options.action.toLowerCase();
                }
            },

            onShow: function () {
                switch (this.action) {
                    case 'cutoff':
                        this._showCutoffUnmet();
                        break;
                    default:
                        this._showMissing();
                }
            },

            _navigate: function (route) {
                Backbone.history.navigate(route);
            },

            _showMissing: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.content.show(new MissingLayout());
                this.ui.missingTab.tab('show');
                this._navigate('/wanted/missing');
            },

            _showCutoffUnmet: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.content.show(new CutoffUnmetLayout());
                this.ui.cutoffTab.tab('show');
                this._navigate('/wanted/cutoff');
            }
        });
    });
