var vent = require('vent');
var Marionette = require('marionette');
var Profiles = require('../../Profile/ProfileCollection');
var AsModelBoundView = require('../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../Mixins/AsValidatedView');
var AsEditModalView = require('../../Mixins/AsEditModalView');
require('../../Mixins/TagInput');
require('../../Mixins/FileBrowser');

var view = Marionette.ItemView.extend({
    template : 'Movie/Edit/EditMovieViewTemplate',

    ui : {
        profile : '.x-profile',
        path    : '.x-path',
        tags    : '.x-tags'
    },

    events : {
        'click .x-remove' : '_removeMovie'
    },

    initialize : function() {
        this.model.set('profiles', Profiles);
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
        this.model.set({ profileId : profileId });
    },

    _onAfterSave : function() {
        this.trigger('saved');
        vent.trigger(vent.Commands.CloseModalCommand);
    },

    _removeMovie : function() {
        vent.trigger(vent.Commands.DeleteMovieCommand, { movie : this.model });
    }
});

AsModelBoundView.call(view);
AsValidatedView.call(view);
AsEditModalView.call(view);

module.exports = view;