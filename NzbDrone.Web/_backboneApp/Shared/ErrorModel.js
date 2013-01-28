/// <reference path="../app.js" />

NzbDrone.Shared.ErrorCollection = Backbone.Collection.extend({

    model: NzbDrone.Shared.ErrorModel,

});


NzbDrone.Shared.ErrorModel = Backbone.Model.extend({

    mutators: {
        pre: function () {
            return this.get('message').lines().lenght > 1;
        }
    },


    defaults: {
        "title": "NO_TITLE",
        "message": "NO_MESSAGE",
    }
});
