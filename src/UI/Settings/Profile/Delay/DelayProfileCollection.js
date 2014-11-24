'use strict';
define(
    [
        'backbone',
        'Settings/Profile/Delay/DelayProfileModel'
    ], function (Backbone, DelayProfileModel) {

        return Backbone.Collection.extend({
            model: DelayProfileModel,
            url  : window.NzbDrone.ApiRoot + '/delayprofile'
        });
    });
