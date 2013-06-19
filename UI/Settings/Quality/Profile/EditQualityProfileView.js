'use strict';
define(['app', 'marionette', 'Mixins/AsModelBoundView'], function (App, Marionette, AsModelBoundView) {

    var view = Marionette.ItemView.extend({
        template: 'Settings/Quality/Profile/EditQualityProfileTemplate',

        events: {
            'click .x-save'             : 'saveQualityProfile',
            'dblclick .x-available-list': '_moveQuality',
            'dblclick .x-allowed-list'  : '_moveQuality'
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

        saveQualityProfile: function () {
            //Todo: Make sure model is updated with Allowed, Cutoff, Name

            this.model.save();
            this.trigger('saved');
            App.modalRegion.closeModal();
        }
    });

    return AsModelBoundView.call(view);

});
