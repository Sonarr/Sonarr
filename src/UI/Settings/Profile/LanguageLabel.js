'use strict';
define(
    [
        'underscore',
        'handlebars',
        'Settings/Profile/Language/LanguageCollection'
    ], function (_, Handlebars, LanguageCollection) {
        Handlebars.registerHelper('languageLabel', function () {

            var wantedLanguage = this.language;

            var language = LanguageCollection.find(function (lang) {
                return lang.get('nameLower') === wantedLanguage;
            });

            var result = '<span class="label label-primary">' + language.get('name') + '</span>';

            return new Handlebars.SafeString(result);
        });
    });
