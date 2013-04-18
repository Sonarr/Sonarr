"use strict";
define(['app'], function () {

    NzbDrone.Shared.Toolbar.ToolbarModel = Backbone.Model.extend({

        defaults: {
            "route"  : '/nzbdrone/route',
            "title"  : 'Title Goes Here',
            "tooltip": undefined
        }
    });
});