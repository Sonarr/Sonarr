'use strict';
define(
    [
        'underscore',
        'marionette',
        'Health/HealthCollection'
    ], function (_, Marionette, HealthCollection) {
        return Marionette.ItemView.extend({
            initialize: function () {
                this.listenTo(HealthCollection, 'sync', this._healthSync);
                HealthCollection.fetch();
            },

            render: function () {
                this.$el.empty();

                if (HealthCollection.length === 0) {
                    return this;
                }

                var count = HealthCollection.length;
                var label = 'label-warning';
                var errors = HealthCollection.some(function (model) {
                    return model.get('type') === 'error';
                });

                if (errors) {
                    label = 'label-important';
                }

                this.$el.html('<span class="label pull-right {0}">{1}</span>'.format(label, count));
                return this;
            },

            _healthSync: function () {
                this.render();
            }
        });
    });
