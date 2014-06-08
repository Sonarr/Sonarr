'use strict';

define(
    [
        'marionette',
        'Profile/ProfileCollection',
        'Settings/Profile/ProfileCollectionView',
        'Settings/Profile/Language/LanguageCollection'
    ], function (Marionette, ProfileCollection, ProfileCollectionView, LanguageCollection) {
        return Marionette.Layout.extend({
            template: 'Settings/Profile/ProfileLayoutTemplate',

            regions: {
                profile : '#profile'
            },

            initialize: function (options) {
                this.settings = options.settings;
                ProfileCollection.fetch();
            },

            onShow: function () {
                this.profile.show(new ProfileCollectionView({collection: ProfileCollection}));
            }
        });
    });

