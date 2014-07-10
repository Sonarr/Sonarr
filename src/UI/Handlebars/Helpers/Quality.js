'use strict';
define(
    [
        'handlebars',
        'Quality/QualityProfileCollection'
    ], function (Handlebars, QualityProfileCollection) {

        Handlebars.registerHelper('qualityProfile', function (profileId) {

            var profile = QualityProfileCollection.get(profileId);

            if (profile) {
                return new Handlebars.SafeString('<span class="label label-default quality-profile-label">' + profile.get('name') + '</span>');
            }

            return undefined;

        });
    });
