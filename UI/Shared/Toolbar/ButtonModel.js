"use strict";
define(['app'], function () {

    NzbDrone.Shared.Toolbar.ActionTypes =
    {
        RouteTrigger: 'RouteTrigger',
        CallBack    : 'CallBack'
    };

    NzbDrone.Shared.Toolbar.ButtonModel = Backbone.Model.extend({
        defaults: {
            'target' : '/nzbdrone/route',
            'title'  : '',
            'active' : false,
            'tooltip': undefined        }
    });
});
