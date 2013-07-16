'use strict';
define(
    [
        'App',
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
                'click .x-save'  : 'saveSeries',
                'click .x-remove': 'removeSeries'
            },


            initialize: function () {
                this.model.set('qualityProfiles', QualityProfiles);
            },


            saveSeries: function () {

                var qualityProfileId = this.ui.qualityProfile.val();
                this.model.set({ qualityProfileId: qualityProfileId});

                this.model.save();
                this.trigger('saved');
                App.modalRegion.closeModal();

            },

            onRender: function () {
                this.ui.path.autoComplete('/directories');
            },

            removeSeries: function () {
                var view = new DeleteSeriesView({ model: this.model });
                App.modalRegion.show(view);
            }
        });


        return AsModelBoundView.apply(view);
    });
