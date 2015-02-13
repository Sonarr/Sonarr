var _ = require('underscore');
var Marionette = require('marionette');
var HealthCollection = require('./HealthCollection');

module.exports = Marionette.ItemView.extend({
    tagName : 'span',

    initialize : function() {
        this.listenTo(HealthCollection, 'sync', this._healthSync);
        HealthCollection.fetch();
    },

    render : function() {
        this.$el.empty();

        if (HealthCollection.length === 0) {
            return this;
        }

        var count = HealthCollection.length;
        var label = 'label-warning';
        var errors = HealthCollection.some(function(model) {
            return model.get('type') === 'error';
        });

        if (errors) {
            label = 'label-danger';
        }

        this.$el.html('<span class="label {0}">{1}</span>'.format(label, count));
        return this;
    },

    _healthSync : function() {
        this.render();
    }
});