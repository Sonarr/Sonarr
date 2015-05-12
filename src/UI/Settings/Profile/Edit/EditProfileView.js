var _ = require('underscore');
var Marionette = require('marionette');
var LanguageCollection = require('../Language/LanguageCollection');
var Config = require('../../../Config');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');

var view = Marionette.ItemView.extend({
    template : 'Settings/Profile/Edit/EditProfileViewTemplate',

    templateHelpers : function() {
        return {
            languages : LanguageCollection.toJSON()
        };
    },
});

AsValidatedView.call(view);

module.exports = AsModelBoundView.call(view);
