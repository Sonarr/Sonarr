'use strict';
define(
    [
        'vent',
        'marionette',
        'Quality/QualityProfileCollection',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView',
        'Mixins/AutoComplete'
    ], function (vent, Marionette, QualityProfiles, AsModelBoundView, AsValidatedView) {

        var view = Marionette.ItemView.extend({
            template: 'Series/Edit/EditSeriesViewTemplate',

            ui: {
                qualityProfile: '.x-quality-profile',
                path          : '.x-path'
            },

            events: {
                'click .x-save'  : '_saveSeries',
                'click .x-remove': '_removeSeries'
            },


            initialize: function () {
                this.model.set('qualityProfiles', QualityProfiles);
            },


            _saveSeries: function () {

                var self = this;
                var qualityProfileId = this.ui.qualityProfile.val();
                this.model.set({ qualityProfileId: qualityProfileId});

                this.model.save().done(function () {
                    self.trigger('saved');
                    vent.trigger(vent.Commands.CloseModalCommand);
                });
            },

            onRender: function () {
                this.ui.path.autoComplete('/directories');
            },

            _removeSeries: function () {
                vent.trigger(vent.Commands.DeleteSeriesCommand, {series:this.model});
            }
        });


        AsModelBoundView.apply(view);
        return AsValidatedView.apply(view);
    });
