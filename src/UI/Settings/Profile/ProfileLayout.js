'use strict';

define(
    [
        'marionette',
        'Profile/ProfileCollection',
        'Settings/Profile/ProfileCollectionView',
        'Settings/Profile/Delay/DelayProfileLayout',
        'Settings/Profile/Delay/DelayProfileCollection',
        'Settings/Profile/Language/LanguageCollection'
    ], function (Marionette, ProfileCollection, ProfileCollectionView, DelayProfileLayout, DelayProfileCollection, LanguageCollection) {
        return Marionette.Layout.extend({
            template: 'Settings/Profile/ProfileLayoutTemplate',

            regions: {
                profile     : '#profile',
                delayProfile : '#delay-profile'
            },

            initialize: function (options) {
                this.settings = options.settings;
                ProfileCollection.fetch();

                this.delayProfileCollection = new DelayProfileCollection();
                this.delayProfileCollection.fetch();
            },

            onShow: function () {
                this.profile.show(new ProfileCollectionView({collection: ProfileCollection}));
                this.delayProfile.show(new DelayProfileLayout({collection: this.delayProfileCollection}));
            }
        });
    });

