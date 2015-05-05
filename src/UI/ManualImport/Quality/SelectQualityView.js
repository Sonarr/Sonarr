var _ = require('underscore');
var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template  : 'ManualImport/Quality/SelectQualityViewTemplate',

    ui : {
        select : '.x-select-quality',
        proper : 'x-proper'
    },

    initialize : function(options) {
        this.qualities = options.qualities;

        this.templateHelpers = {
            qualities: this.qualities
        };
    },

    selectedQuality : function () {
        var selected = parseInt(this.ui.select.val(), 10);
        var proper = this.ui.proper.prop('checked');

        var quality = _.find(this.qualities, function(q) {
            return q.id === selected;
        });


        return {
            quality  : quality,
            revision : {
                version : proper ? 2 : 1,
                real    : 0
            }
        };
    }
});