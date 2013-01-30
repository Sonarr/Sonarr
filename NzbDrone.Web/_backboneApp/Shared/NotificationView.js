/// <reference path="../app.js" />
/// <reference path="NotificationModel.js" />

NzbDrone.Shared.NotificationItemView = Backbone.Marionette.ItemView.extend({
    template: "Shared/NotificationTemplate",

    events: {
        'click .x-close': 'kill'
    },

    kill: function () {

        var self = this;

        $.Deferred(function () {
            self.$el.slideUp('slow');
        }).done(function () {
            self.model.destroy();
        });


    }
});

NzbDrone.Shared.NotificationCollectionView = Backbone.Marionette.CollectionView.extend({

    itemView: NzbDrone.Shared.NotificationItemView,

    initialize: function () {
        this.collection = new NzbDrone.Shared.NotificationCollection();
    },
});

NzbDrone.addInitializer(function (options) {
    NzbDrone.notificationRegion.show(NzbDrone.Shared.NotificationCollectionView.Instance);
});


NzbDrone.Shared.NotificationCollectionView.Instance = new NzbDrone.Shared.NotificationCollectionView();

window.onerror = function (msg, url, line) {

    try {
        var model = new NzbDrone.Shared.NotificationModel();

        var a = document.createElement('a');
        a.href = url;

        model.set('title', a.pathname.split('/').pop() + ' : ' + line);
        model.set('message', msg);
        model.set('level', 'error');
        NzbDrone.Shared.NotificationCollectionView.Instance.collection.add(model);
    } catch (error) {
        alert('Couldn\'t report JS error. ' + error);
    }

    var suppressErrorAlert = false;
    return suppressErrorAlert;
};

$(document).ajaxError(function (event, xmlHttpRequest, ajaxOptions) {

    if (xmlHttpRequest.status >= 200 && xmlHttpRequest.status <= 300) {
        return;
    }
    var model = new NzbDrone.Shared.NotificationModel();
    model.set('title', ajaxOptions.type + " " + ajaxOptions.url + " : " + xmlHttpRequest.statusText);
    model.set('message', xmlHttpRequest.responseText);
    model.set('level', 'error');
    NzbDrone.Shared.NotificationCollectionView.Instance.collection.add(model);

    var suppressErrorAlert = false;
    return suppressErrorAlert;
});