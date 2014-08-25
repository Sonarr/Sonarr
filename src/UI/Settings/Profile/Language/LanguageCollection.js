'use strict';
define(
    [
        'backbone',
        'Settings/Profile/Language/LanguageModel'
    ], function (Backbone, LanguageModel) {

        var LanuageCollection = Backbone.Collection.extend({
            model: LanguageModel,
            url  : window.NzbDrone.ApiRoot + '/language'
        });

        var languages = new LanuageCollection();

        languages.fetch();

        return languages;
    });
