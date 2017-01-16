var Handlebars = require('handlebars');
var ProfileCollection = require('../../Profile/ProfileCollection');

Handlebars.registerHelper('profile', function(profileId) {
    var profile = ProfileCollection.get(profileId);

    if (profile) {
        return new Handlebars.SafeString('<span class="label label-default profile-label">' + profile.get('name') + '</span>');
    }

    return undefined;
});