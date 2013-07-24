'use strict';

define(
    [
        'app',
        'marionette',
        'Mixins/AsModelBoundView'
    ], function (App, Marionette, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Indexers/EditTemplate',

            events: {
                'click .x-save'        : '_save',
                'click .x-save-and-add': '_saveAndAdd'
            },

            initialize: function (options) {
                this.indexerCollection = options.indexerCollection;
            },

            _save: function () {
                var self = this;
                var promise = this.model.saveSettings();

                if (promise) {
                    promise.done(function () {
                        self.indexerCollection.add(self.model, { merge: true });
                        App.vent.trigger(App.Commands.CloseModalCommand);
                    });
                }
            },

            _saveAndAdd: function () {
                var self = this;
                var promise = this.model.saveSettings();

                if (promise) {
                    promise.done(function () {
                        self.indexerCollection.add(self.model, { merge: true });

                        self.model.set({
                            id    : undefined,
                            name  : '',
                            enable: false
                        });

                        _.each(self.model.get('fields'), function (value, key, list) {
                            self.model.set('fields.' + key + '.value', '');
                        });
                    });
                }
            }
        });

        return AsModelBoundView.call(view);
    });
