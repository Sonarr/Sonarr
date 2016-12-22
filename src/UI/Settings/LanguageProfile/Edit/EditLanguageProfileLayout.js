var _ = require('underscore');
var vent = require('vent');
var AppLayout = require('../../../AppLayout');
var Marionette = require('marionette');
var Backbone = require('backbone');
var LanguageSortableCollectionView = require('./LanguageSortableCollectionView');
var EditProfileView = require('./EditLanguageProfileView');
var DeleteView = require('../DeleteLanguageProfileView');
var SeriesCollection = require('../../../Series/SeriesCollection');
var Config = require('../../../Config');
var AsEditModalView = require('../../../Mixins/AsEditModalView');

var view = Marionette.Layout.extend({
    template : 'Settings/LanguageProfile/Edit/EditLanguageProfileLayoutTemplate',

    regions : {
        fields    : '#x-fields',
        languages : '#x-languages'
    },

    ui : {
        deleteButton : '.x-delete'
    },

    _deleteView : DeleteView,

    initialize : function(options) {
        this.profileCollection = options.profileCollection;
        this.itemsCollection = new Backbone.Collection(_.toArray(this.model.get('languages')).reverse());
        this.listenTo(SeriesCollection, 'all', this._updateDisableStatus);
    },

    onRender : function() {
        this._updateDisableStatus();
    },

    onShow : function() {
        this.fieldsView = new EditProfileView({ model : this.model });
        this._showFieldsView();
        var advancedShown = Config.getValueBoolean(Config.Keys.AdvancedSettings, false);

        this.sortableListView = new LanguageSortableCollectionView({
            selectable     : true,
            selectMultiple : true,
            clickToSelect  : true,
            clickToToggle  : true,
            sortable       : advancedShown,

            sortableOptions : {
                handle : '.x-drag-handle'
            },

            visibleModelsFilter : function(model) {
                return model.get('language').id !== 0 || advancedShown;
            },

            collection : this.itemsCollection,
            model      : this.model
        });

        this.sortableListView.setSelectedModels(this.itemsCollection.filter(function(item) {
            return item.get('allowed') === true;
        }));
        this.languages.show(this.sortableListView);

        this.listenTo(this.sortableListView, 'selectionChanged', this._selectionChanged);
        this.listenTo(this.sortableListView, 'sortStop', this._updateModel);
    },

    _onBeforeSave : function() {
        var cutoff = this.fieldsView.getCutoff();
        this.model.set('cutoff', cutoff);
    },

    _onAfterSave : function() {
        this.profileCollection.add(this.model, { merge : true });
        vent.trigger(vent.Commands.CloseModalCommand);
    },

    _selectionChanged : function(newSelectedModels, oldSelectedModels) {
        var addedModels = _.difference(newSelectedModels, oldSelectedModels);
        var removeModels = _.difference(oldSelectedModels, newSelectedModels);

        _.each(removeModels, function(item) {
            item.set('allowed', false);
        });
        _.each(addedModels, function(item) {
            item.set('allowed', true);
        });
        this._updateModel();
    },

    _updateModel : function() {
        this.model.set('languages', this.itemsCollection.toJSON().reverse());

        this._showFieldsView();
    },

    _showFieldsView : function() {
        this.fields.show(this.fieldsView);
    },

    _updateDisableStatus : function() {
        if (this._isProfileInUse()) {
            this.ui.deleteButton.addClass('disabled');
            this.ui.deleteButton.attr('title', 'Can\'t delete a profile that is attached to a series.');
        } else {
            this.ui.deleteButton.removeClass('disabled');
        }
    },

    _isProfileInUse : function() {
        return SeriesCollection.where({ 'languageProfileId' : this.model.id }).length !== 0;
    }
});
module.exports = AsEditModalView.call(view);
