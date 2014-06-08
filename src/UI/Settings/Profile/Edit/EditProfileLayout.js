'use strict';
define(
    [
        'underscore',
        'vent',
        'AppLayout',
        'marionette',
        'backbone',
        'Settings/Profile/Edit/EditProfileItemView',
        'Settings/Profile/Edit/QualitySortableCollectionView',
        'Settings/Profile/Edit/EditProfileView',
        'Settings/Profile/DeleteProfileView',
        'Series/SeriesCollection',
        'Config'
    ], function (_,
                 vent,
                 AppLayout,
                 Marionette,
                 Backbone,
                 EditProfileItemView,
                 QualitySortableCollectionView,
                 EditProfileView,
                 DeleteView,
                 SeriesCollection,
                 Config) {

        return Marionette.Layout.extend({
            template: 'Settings/Profile/Edit/EditProfileLayoutTemplate',

            regions: {
                fields   : '#x-fields',
                qualities: '#x-qualities'
            },

            ui: {
                deleteButton: '.x-delete'
            },

            events: {
                'click .x-save'   : '_saveProfile',
                'click .x-cancel' : '_cancelProfile',
                'click .x-delete' : '_delete'
            },

            initialize: function (options) {
                this.profileCollection = options.profileCollection;
                this.itemsCollection = new Backbone.Collection(_.toArray(this.model.get('items')).reverse());
                this.listenTo(SeriesCollection, 'all', this._updateDisableStatus);
            },

            onRender: function () {
                this._updateDisableStatus();
            },

            onShow: function () {
                this.fieldsView = new EditProfileView({ model: this.model });
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
            
            _saveProfile: function () {
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

            _cancelProfile: function () {
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
            },

            _delete: function () {
                var view = new DeleteView({ model: this.model });
                AppLayout.modalRegion.show(view);
            },

            _updateDisableStatus: function () {
                if (this._isQualityInUse()) {
                    this.ui.deleteButton.addClass('disabled');
                    this.ui.deleteButton.attr('title', 'Can\'t delete a profile that is attached to a series.');
                }
                else {
                    this.ui.deleteButton.removeClass('disabled');
                }
            },

            _isQualityInUse: function () {
                return SeriesCollection.where({'profileId': this.model.id}).length !== 0;
            }
        });
    });
