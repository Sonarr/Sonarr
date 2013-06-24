'use strict';
define(['app'], function () {

    NzbDrone.Shared.Toolbar.ActionTypes =
    {
        RouteTrigger: 'RouteTrigger',
        CallBack    : 'CallBack'
    };

    return Backbone.Model.extend({
        defaults: {
            'target' : '/nzbdrone/route',
            'title'  : '',
            'active' : false,
            'tooltip': undefined        }
    });
});
