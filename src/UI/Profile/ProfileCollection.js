'use strict';
define(
    [
        'backbone',
        'Profile/ProfileModel'
    ], function (Backbone, ProfileModel) {

        var ProfileCollection = Backbone.Collection.extend({
            model: ProfileModel,
            url  : window.NzbDrone.ApiRoot + '/profile'
        });

        var profiles = new ProfileCollection();

        profiles.fetch();

        return profiles;
    });
