/// <reference path="../app.js" />

NzbDrone.Shared.ErrorCollection = Backbone.Collection.extend({

    model: NzbDrone.Shared.ErrorModel,

});


NzbDrone.Shared.ErrorModel = Backbone.Model.extend({

    defaults: {
        "title": "NO_TITLE",
        "message": "NO_MESSAGE",
    }
});
