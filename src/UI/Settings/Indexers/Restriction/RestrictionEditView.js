var _ = require('underscore');
var vent = require('vent');
var AppLayout = require('../../../AppLayout');
var Marionette = require('marionette');
var DeleteView = require('./RestrictionDeleteView');
var CommandController = require('../../../Commands/CommandController');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
var AsEditModalView = require('../../../Mixins/AsEditModalView');
require('../../../Mixins/TagInput');
require('bootstrap');
require('bootstrap.tagsinput');

var view = Marionette.ItemView.extend({
    template : 'Settings/Indexers/Restriction/RestrictionEditViewTemplate',

    ui : {
        required : '.x-required',
        ignored  : '.x-ignored',
        tags     : '.x-tags'
    },

    _deleteView : DeleteView,

    initialize : function(options) {
        this.targetCollection = options.targetCollection;
    },

    onRender : function() {
        this.ui.required.tagsinput({
            trimValue : true,
            tagClass  : 'label label-success'
        });

        this.ui.ignored.tagsinput({
            trimValue : true,
            tagClass  : 'label label-danger'
        });

        this.ui.tags.tagInput({
            model    : this.model,
            property : 'tags'
        });
    },

    _onAfterSave : function() {
        this.targetCollection.add(this.model, { merge : true });
        vent.trigger(vent.Commands.CloseModalCommand);
    }
});

AsModelBoundView.call(view);
AsValidatedView.call(view);
AsEditModalView.call(view);
module.exports = view;