'use strict';
define(
    [
        'vent',
        'marionette',
        'backbone',
        'backbone.collectionview',
        'Settings/Quality/Profile/EditQualityProfileItemView',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView',
        'underscore'
    ], function (vent, Marionette, Backbone, BackboneSortableCollectionView, EditQualityProfileItemView, AsModelBoundView, AsValidatedView, _) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Quality/Profile/EditQualityProfileTemplate',

            ui: {
                available: '.x-available-list',
                allowed  : '.x-allowed-list',
                cutoff   : '.x-cutoff'
            },
            
            events: {
                'click .x-save'             : '_saveQualityProfile',
                //'click .x-qualityitem'      : '_moveQuality',
            },

            initialize: function (options) {
                this.profileCollection = options.profileCollection;
                
                this.availableCollection = new Backbone.Collection(this.model.get('available'));
                this.availableCollection.comparator = function (model) { return -model.get('weight'); };
                this.availableCollection.sort();
                                
                this.allowedCollection = new Backbone.Collection(this.model.get('allowed').reverse());
            },
            
            onRender: function() {
                var listViewAvailable = new BackboneSortableCollectionView( {
                    el        : this.ui.available,
                    modelView : EditQualityProfileItemView,
                    selectable: false,
                    sortable  : false,
                    collection: this.availableCollection
                    });
                listViewAvailable.render();
                  
                var listViewAllowed = new BackboneSortableCollectionView( {
                    el        : this.ui.allowed,
                    modelView : EditQualityProfileItemView,
                    selectable: false,
                    sortable  : true,
                    sortableOptions : {
                        handle: ".x-drag-handle"
                    },
                    collection : this.allowedCollection
                  } );
                listViewAllowed.render();
                
                this.listenTo(listViewAvailable, "doubleClick", this._moveQuality);
                this.listenTo(listViewAllowed, "doubleClick", this._moveQuality);
                this.listenTo(listViewAllowed, "sortStop", this._updateModel);
            },

            _moveQuality: function (event) {

                var quality;
                var qualityId = event.get('id');
                
                if (this.availableCollection.get(qualityId)) {
                    quality = this.availableCollection.get(qualityId);
                    var idealIndex = 0;
                    var idealMismatches = 1000;
                    // Insert it at the best possible spot.
                    for (var i = 0; i <= this.allowedCollection.length; i++) {
                        var mismatches = 0;
                        for (var j = 0; j < i; j++) {
                           if (this.allowedCollection.at(j).get('weight') < quality.get('weight'))
                              mismatches++;
                        }
                        for (j = i; j < this.allowedCollection.length; j++) {
                           if (this.allowedCollection.at(j).get('weight') > quality.get('weight'))
                              mismatches++;
                        }
                        if (mismatches <= idealMismatches) {
                           idealIndex = i;
                           idealMismatches = mismatches;
                        }
                    }
                    
                    this.availableCollection.remove(quality);
                    this.allowedCollection.add(quality, {at: idealIndex});
                }
                else if (this.allowedCollection.get(qualityId)) {
                    quality = this.allowedCollection.get(qualityId);

                    this.allowedCollection.remove(quality);
                    this.availableCollection.add(quality);
                }
                else {
                    throw 'couldnt find quality id ' + qualityId;
                }
                
                this._updateModel();
            },
            
            _updateModel: function() {            
                this.model.set('available', this.availableCollection.toJSON().reverse());
                this.model.set('allowed', this.allowedCollection.toJSON().reverse());
                
                this.render();
            },
            
            _saveQualityProfile: function () {
                var self = this;
                var cutoff = _.findWhere(this.model.get('allowed'), { id: parseInt(this.ui.cutoff.val(), 10)});
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
