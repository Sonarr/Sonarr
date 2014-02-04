'use strict';
define(
    [
        'underscore',
        'vent',
        'marionette',
        'backbone',
        'Settings/Quality/Profile/Edit/EditQualityProfileItemView',
        'Settings/Quality/Profile/Edit/QualitySortableCollectionView',
        'Settings/Quality/Profile/Edit/EditQualityProfileView',
        'Config'
    ], function (_, vent, Marionette, Backbone, EditQualityProfileItemView, QualitySortableCollectionView, EditQualityProfileView, Config) {

        return Marionette.Layout.extend({
            template: 'Settings/Quality/Profile/Edit/EditQualityProfileLayoutTemplate',

            regions: {
                fields   : '#x-fields',
                qualities: '#x-qualities'
            },

            events: {
                'click .x-save'  : '_saveQualityProfile',
                'click .x-cancel': '_cancelQualityProfile'
            },

            initialize: function (options) {
                this.profileCollection = options.profileCollection;
                this.itemsCollection = new Backbone.Collection(_.toArray(this.model.get('items')).reverse());
            },

            onShow: function () {
                this.fieldsView = new EditQualityProfileView({ model: this.model });
                this._showFieldsView();

                this.sortableListView = new QualitySortableCollectionView({
                    selectable      : true,
                    selectMultiple  : true,
                    clickToSelect   : true,
                    clickToToggle   : true,
                    sortable        : Config.getValueBoolean(Config.Keys.AdvancedSettings, false),

                    sortableOptions : {
                        handle: '.x-drag-handle'
                    },

                    collection: this.itemsCollection,
                    model     : this.model
                });

                this.sortableListView.setSelectedModels(this.itemsCollection.filter(function(item) { return item.get('allowed') === true; }));
                this.qualities.show(this.sortableListView);

                this.listenTo(this.sortableListView, 'selectionChanged', this._selectionChanged);
                this.listenTo(this.sortableListView, 'sortStop', this._updateModel);
            },
            
            _selectionChanged: function(newSelectedModels, oldSelectedModels) {
                var addedModels = _.difference(newSelectedModels, oldSelectedModels);
                var removeModels = _.difference(oldSelectedModels, newSelectedModels);
                
                _.each(removeModels, function(item) { item.set('allowed', false); });
                _.each(addedModels, function(item) { item.set('allowed', true); });
                
                this._updateModel();
            },
            
            _updateModel: function() {            
                this.model.set('items', this.itemsCollection.toJSON().reverse());

                this._showFieldsView();
            },
            
            _saveQualityProfile: function () {
                var self = this;
                var cutoff = this.fieldsView.getCutoff();
                this.model.set('cutoff', cutoff);

                var promise = this.model.save();

                if (promise) {
                    promise.done(function () {
                        self.profileCollection.add(self.model, { merge: true });
                        vent.trigger(vent.Commands.CloseModalCommand);
                    });
                }
            },

            _cancelQualityProfile: function () {
                if (!this.model.has('id')) {
                    vent.trigger(vent.Commands.CloseModalCommand);
                    return;
                }

                var promise = this.model.fetch();

                if (promise) {
                    promise.done(function () {
                        vent.trigger(vent.Commands.CloseModalCommand);
                    });
                }
            },

            _showFieldsView: function () {
                this.fields.show(this.fieldsView);
            }
        });
    });
