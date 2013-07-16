'use strict';
define(
    [
        'handlebars',
        'Quality/QualityProfileCollection',
        'underscore'
    ], function (Handlebars, QualityProfileCollection, _) {

        Handlebars.registerHelper('qualityProfile', function (profileId) {

            var profile = QualityProfileCollection.get(profileId);

            if (profile) {
                return new Handlebars.SafeString('<span class="label quality-profile-lable">' + profile.get("name") + '</span>');
            }

            return undefined;

        });

    });
