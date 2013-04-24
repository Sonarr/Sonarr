"use strict";
define(['app'], function () {

    NzbDrone.Shared.Toolbar.CommandTypes =
    {
        RouteTrigger: 'RouteTrigger'
    };

    NzbDrone.Shared.Toolbar.CommandModel = Backbone.Model.extend({
        defaults: {
            'target'   : '/nzbdrone/route',
            'title'    : 'Title Goes Here',
            'alignment': 'left',
            'tooltip'  : undefined
        }
    });
});
