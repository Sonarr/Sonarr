'use strict';
define(
    [
        'app',
        'marionette',
        'Series/Delete/DeleteSeriesView',
        'Quality/QualityProfileCollection',
        'Mixins/AsModelBoundView',
        'Mixins/AutoComplete'
    ], function (App, Marionette, DeleteSeriesView, QualityProfiles, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template: 'Series/Edit/EditSeriesTemplate',

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
                    App.vent.trigger(App.Commands.CloseModalCommand);
                });
            },

            onRender: function () {
                this.ui.path.autoComplete('/directories');
            },

            _removeSeries: function () {
                var view = new DeleteSeriesView({ model: this.model });
                App.modalRegion.show(view);
            }
        });


        return AsModelBoundView.apply(view);
    });
