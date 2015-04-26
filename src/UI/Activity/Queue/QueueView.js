var _ = require('underscore');
var Marionette = require('marionette');
var QueueCollection = require('./QueueCollection');

module.exports = Marionette.ItemView.extend({
    tagName : 'span',

    initialize : function() {
        this.listenTo(QueueCollection, 'sync', this.render);
        QueueCollection.fetch();
    },

    render : function() {
        this.$el.empty();

        if (QueueCollection.length === 0) {
            return this;
        }

        var count = QueueCollection.fullCollection.length;
        var label = 'label-info';

        var errors = QueueCollection.fullCollection.some(function(model) {
            return model.has('trackedDownloadStatus') && model.get('trackedDownloadStatus').toLowerCase() === 'error';
        });

        var warnings = QueueCollection.fullCollection.some(function(model) {
            return model.has('trackedDownloadStatus') && model.get('trackedDownloadStatus').toLowerCase() === 'warning';
        });

        if (errors) {
            label = 'label-danger';
        } else if (warnings) {
            label = 'label-warning';
        }

        this.$el.html('<span class="label {0}">{1}</span>'.format(label, count));
        return this;
    }
});