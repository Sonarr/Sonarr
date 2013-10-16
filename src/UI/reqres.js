define(
    [
        'marionette',
        'backbone'
    ], function (Marionette, Backbone) {
        'use strict';
        var reqres = new Backbone.Wreqr.RequestResponse();

        reqres.Requests = {
            GetEpisodeFileById: 'GetEpisodeFileById'
        };

        return reqres;
    });
