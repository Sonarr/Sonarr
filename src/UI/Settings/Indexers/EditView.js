'use strict';

define(
    [
        'vent',
        'marionette',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView',
        'underscore'
    ], function (vent, Marionette, AsModelBoundView, AsValidatedView, _) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Indexers/EditTemplate',

            ui: {
                activity: '.x-activity'
            },

            events: {
                'click .x-save'        : '_save',
                'click .x-save-and-add': '_saveAndAdd',
                'click .x-cancel'      : '_cancel'
            },

            initialize: function (options) {
                this.indexerCollection = options.indexerCollection;
            },

            _save: function () {
                this.ui.activity.html('<i class="icon-nd-spinner"></i>');

                var self = this;
                var promise = this.model.saveSettings();

                if (promise) {
                    promise.done(function () {
                        self.indexerCollection.add(self.model, { merge: true });
                        vent.trigger(vent.Commands.CloseModalCommand);
                    });

                    promise.fail(function () {
                        self.ui.activity.empty();
                    });
                }
            },

            _saveAndAdd: function () {
                this.ui.activity.html('<i class="icon-nd-spinner"></i>');

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

                    promise.fail(function () {
                        self.ui.activity.empty();
                    });
                }
            },

            _cancel: function () {
                if (this.model.isNew()) {
                    this.model.destroy();
                    vent.trigger(vent.Commands.CloseModalCommand);
                }
            }
        });

        AsModelBoundView.call(view);
        AsValidatedView.call(view);

        return view;
    });
