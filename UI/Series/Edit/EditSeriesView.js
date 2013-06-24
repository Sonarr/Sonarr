﻿'use strict';
define(
    [
        'App',
        'marionette',
        'Series/Delete/DeleteSeriesView',
        'Quality/QualityProfileCollection',
        'Mixins/AsModelBoundView'
    ], function (App, Marionette, DeleteSeriesView, QualityProfiles, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template: 'Series/Edit/EditSeriesTemplate',

            ui: {
                progressbar    : '.progress .bar',
                qualityProfile : '.x-quality-profile',
                backlogSettings: '.x-backlog-setting'
            },

            events: {
                'click .x-save'  : 'saveSeries',
                'click .x-remove': 'removeSeries'
            },


            initialize: function () {
                this.model.set('qualityProfiles', QualityProfiles);
            },


            saveSeries: function () {
                //Todo: Get qualityProfile + backlog setting from UI
                var qualityProfile = this.ui.qualityProfile.val();
                var qualityProfileText = this.ui.qualityProfile.children('option:selected').text();
                var backlogSetting = this.ui.backlogSettings.val();

                this.model.set({ qualityProfileId: qualityProfile, backlogSetting: backlogSetting, qualityProfileName: qualityProfileText });

                this.model.save();
                this.trigger('saved');
                App.modalRegion.closeModal();
            },

            removeSeries: function () {
                var view = new DeleteSeriesView({ model: this.model });
                App.modalRegion.show(view);
            }
        });
        return AsModelBoundView.apply(view);
    });
