var vent = require('vent');
var Marionette = require('marionette');
var Profiles = require('../../Profile/ProfileCollection');
var LanguageProfiles = require('../../LanguageProfile/LanguageProfileCollection');
var AsModelBoundView = require('../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../Mixins/AsValidatedView');
var AsEditModalView = require('../../Mixins/AsEditModalView');
require('../../Mixins/TagInput');
require('../../Mixins/FileBrowser');

var view = Marionette.ItemView.extend({
    template : 'Series/Edit/EditSeriesViewTemplate',

    ui : {
        profile         : '.x-profile',
        languageProfile : '.x-language-profile',
        path            : '.x-path',
        tags            : '.x-tags'
    },

    events : {
        'click .x-remove' : '_removeSeries'
    },

    initialize : function() {
        this.model.set('profiles', Profiles);
        this.model.set('languageProfiles', LanguageProfiles);
    },

    onRender : function() {
        this.ui.path.fileBrowser();
        this.ui.tags.tagInput({
            model    : this.model,
            property : 'tags'
        });
    },

    _onBeforeSave : function() {
        var profileId = this.ui.profile.val();
        var languageProfileId = this.ui.languageProfile.val();
        this.model.set({ profileId : profileId });
        this.model.set({ languageProfileId : languageProfileId });
    },

    _onAfterSave : function() {
        this.trigger('saved');
        vent.trigger(vent.Commands.CloseModalCommand);
    },

    _removeSeries : function() {
        vent.trigger(vent.Commands.DeleteSeriesCommand, { series : this.model });
    }
});

AsModelBoundView.call(view);
AsValidatedView.call(view);
AsEditModalView.call(view);

module.exports = view;