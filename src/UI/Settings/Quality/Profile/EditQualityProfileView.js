'use strict';
define(
    [
        'underscore',
        'vent',
        'marionette',
        'backbone',
        'Settings/Quality/Profile/QualitySortableCollectionView',
        'Settings/Quality/Profile/EditQualityProfileItemView',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView',
        'Config'
    ], function (_, vent, Marionette, Backbone, QualitySortableCollectionView, EditQualityProfileItemView, AsModelBoundView, AsValidatedView, Config) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Quality/Profile/EditQualityProfileViewTemplate',

            ui: {
                allowed  : '.x-allowed-list',
                cutoff   : '.x-cutoff'
            },
            
            events: {
                'click .x-save': '_saveQualityProfile'
            },

            initialize: function (options) {
                this.profileCollection = options.profileCollection;
                                                
                this.itemsCollection = new Backbone.Collection(_.toArray(this.model.get('items')).reverse());
            },
            
            onRender: function() {                  

                var listViewAllowed = new QualitySortableCollectionView({
                        el              : this.ui.allowed,
                        modelView       : EditQualityProfileItemView,
                        selectable      : true,
                        selectMultiple  : true,
                        clickToSelect   : true,
                        clickToToggle   : true,
                        sortable        : Config.getValueBoolean(Config.Keys.AdvancedSettings, false),
                        collection : this.itemsCollection
                    });

                listViewAllowed.setSelectedModels(this.itemsCollection.filter(function(item) { return item.get('allowed') === true; }));

                listViewAllowed.render();
                
                this.listenTo(listViewAllowed, 'selectionChanged', this._selectionChanged);
                this.listenTo(listViewAllowed, 'sortStop', this._updateModel);
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
                
                this.render();
            },
            
            _saveQualityProfile: function () {
                var self = this;
                var cutoff = _.findWhere(_.pluck(this.model.get('items'), 'quality'), { id: parseInt(self.ui.cutoff.val(), 10)});
                this.model.set('cutoff', cutoff);

                var promise = this.model.save();

                if (promise) {
                    promise.done(function () {
                        self.profileCollection.add(self.model, { merge: true });
                        vent.trigger(vent.Commands.CloseModalCommand);
                    });
                }
            }
        });

        AsValidatedView.call(view);
        return AsModelBoundView.call(view);

    });
