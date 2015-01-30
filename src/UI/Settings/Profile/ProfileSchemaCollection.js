'use strict';

define(
    [
        'backbone',
        'Profile/ProfileModel'
    ], function (Backbone, ProfileModel) {

        return Backbone.Collection.extend({
            model: ProfileModel,
            url  : window.NzbDrone.ApiRoot + '/profile/schema'
        });
    });
