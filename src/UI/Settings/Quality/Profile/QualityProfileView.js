'use strict';

define(
    [
        'AppLayout',
        'marionette',
        'Settings/Quality/Profile/EditQualityProfileView',
        'Settings/Quality/Profile/DeleteView',
        'Series/SeriesCollection',
        'Mixins/AsModelBoundView',
        'Settings/Quality/Profile/AllowedLabeler',
        'bootstrap'
    ], function (AppLayout, Marionette, EditProfileView, DeleteProfileView, SeriesCollection, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Quality/Profile/QualityProfileTemplate',
            tagName : 'li',

            ui: {
                'progressbar' : '.progress .bar',
                'deleteButton': '.x-delete'
            },

            events: {
                'click .x-edit'  : '_editProfile',
                'click .x-delete': '_deleteProfile'
            },

            initialize: function () {
                this.listenTo(this.model, 'sync', this.render);
                this.listenTo(SeriesCollection, 'all', this._updateDisableStatus);
            },

            _editProfile: function () {
                var view = new EditProfileView({ model: this.model, profileCollection: this.model.collection });
                AppLayout.modalRegion.show(view);
            },

            _deleteProfile: function () {
                if (this._isQualityInUse()) {
                    return;
                }

                var view = new DeleteProfileView({ model: this.model });
                AppLayout.modalRegion.show(view);
            },

            onRender: function () {
                this._updateDisableStatus();
            },

            _updateDisableStatus: function () {
                if (this._isQualityInUse()) {
                    this.ui.deleteButton.addClass('disabled');
                    this.ui.deleteButton.attr('title', 'Can\'t delete quality profiles attached to a series.');
                }
                else {
                    this.ui.deleteButton.removeClass('disabled');
                    this.ui.deleteButton.attr('title', 'Delete Quality Profile');
                }
            },

            _isQualityInUse: function () {
                return SeriesCollection.where({'qualityProfileId': this.model.id}).length !== 0;

            }
        });

        return AsModelBoundView.call(view);
    });
