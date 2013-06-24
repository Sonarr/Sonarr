'use strict';

define([
    'app',
    'marionette',
    'Settings/Quality/Profile/EditQualityProfileView',
    'Mixins/AsModelBoundView'
], function (App, Marionette, EditProfileView, AsModelBoundView) {

    var view = Marionette.ItemView.extend({
        template: 'Settings/Quality/Profile/QualityProfileTemplate',
        tagName : 'tr',

        ui: {
            'progressbar': '.progress .bar'
        },

        events: {
            'click .x-edit'  : 'edit',
            'click .x-remove': 'removeQuality'
        },

        edit: function () {
            var view = new EditProfileView({ model: this.model});
            App.modalRegion.show(view);
        },

        removeQuality: function () {
            var view = new NzbDrone.Series.Delete.DeleteSeriesView({ model: this.model });
            NzbDrone.modalRegion.show(view);
        }
    });

    return AsModelBoundView.call(view);
});
