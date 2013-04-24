"use strict";
define(['app', 'Shared/Toolbar/CommandView'], function () {
    NzbDrone.Shared.Toolbar.ButtonGroupView = Backbone.Marionette.CollectionView.extend({
        className: 'btn-group',
        itemView : NzbDrone.Shared.Toolbar.CommandView
    });
});




