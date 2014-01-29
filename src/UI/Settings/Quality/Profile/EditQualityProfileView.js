'use strict';
define(
    [
        'vent',
        'marionette',
        'backbone',
        'Settings/Quality/Profile/QualitySortableCollectionView',
        'Settings/Quality/Profile/EditQualityProfileItemView',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView',
        'underscore'
    ], function (vent, Marionette, Backbone, BackboneSortableCollectionView, EditQualityProfileItemView, AsModelBoundView, AsValidatedView, _) {

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
                                                
                this.allowedCollection = new Backbone.Collection(_.toArray(this.model.get('items')).reverse());
            },
            
            onRender: function() {                  
                var MyCollectionView = BackboneSortableCollectionView.extend({
                    events : {
                            // Backbone.CollectionView used mousedown for the click event, which interferes with the sortable.
                            "click li, td" : "_listItem_onMousedown",
                            "dblclick li, td" : "_listItem_onDoubleClick",
                            "click" : "_listBackground_onClick",
                            "click ul.collection-list, table.collection-list" : "_listBackground_onClick",
                            "keydown" : "_onKeydown"
                    }
                });
                var listViewAllowed = new MyCollectionView({
                        el              : this.ui.allowed,
                        modelView       : EditQualityProfileItemView,
                        selectable      : true,
                        selectMultiple  : true,
                        clickToSelect   : true,
                        clickToToggle   : true,
                        sortable        : true,
                        sortableOptions : {
                            handle: '.x-drag-handle'
                        },
                        collection : this.allowedCollection
                    });

                listViewAllowed.setSelectedModels(this.allowedCollection.filter(function(item) { return item.get('allowed') === true; }));

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
                this.model.set('items', this.allowedCollection.toJSON().reverse());
                
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
