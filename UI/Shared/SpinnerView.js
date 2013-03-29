define(['app'], function () {
    NzbDrone.Shared.SpinnerView = Backbone.Marionette.ItemView.extend({
        template : 'Shared/SpinnerTemplate',
        className: 'nz-spinner row'
    });
});


