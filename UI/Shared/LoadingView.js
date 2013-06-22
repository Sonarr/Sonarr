'use strict';

define(['app'], function () {
    NzbDrone.Shared.LoadingView = Backbone.Marionette.ItemView.extend({
        template : 'Shared/LoadingTemplate',
        className: 'nz-loading row'
    });

    return  NzbDrone.Shared.LoadingView;
});
