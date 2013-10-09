'use strict';
define(
    [
        'vent',
        'marionette',
        'backbone',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView',
        'underscore'
    ], function (vent, Marionette, Backbone, AsModelBoundView, AsValidatedView, _) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Quality/Profile/EditQualityProfileTemplate',

            ui: {
                cutoff: '.x-cutoff'
            },

            events: {
                'click .x-save'             : '_saveQualityProfile',
                'dblclick .x-available-list': '_moveQuality',
                'dblclick .x-allowed-list'  : '_moveQuality'
            },

            initialize: function (options) {
                this.profileCollection = options.profileCollection;
            },

            _moveQuality: function (event) {

                var quality;
                var qualityId = event.target.value;
                var availableCollection = new Backbone.Collection(this.model.get('available'));
                availableCollection.comparator = function (model) {
                    return model.get('weight');
                };

                var allowedCollection = new Backbone.Collection(this.model.get('allowed'));
                allowedCollection.comparator = function (model) {
                    return model.get('weight');
                };

                if (availableCollection.get(qualityId)) {
                    quality = availableCollection.get(qualityId);
                    availableCollection.remove(quality);
                    allowedCollection.add(quality);
                }
                else if (allowedCollection.get(qualityId)) {
                    quality = allowedCollection.get(qualityId);

                    allowedCollection.remove(quality);
                    availableCollection.add(quality);
                }
                else {
                    throw 'couldnt find quality id ' + qualityId;
                }

                this.model.set('available', availableCollection.toJSON());
                this.model.set('allowed', allowedCollection.toJSON());

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
