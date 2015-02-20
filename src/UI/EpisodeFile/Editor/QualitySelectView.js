var _ = require('underscore');
var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template : 'EpisodeFile/Editor/QualitySelectViewTemplate',

    ui : {
        select : '.x-select'
    },

    events : {
        'change .x-select' : '_changeSelect'
    },

    initialize : function (options) {
        this.qualities = options.qualities;

        this.templateHelpers = {
            qualities : this.qualities
        };
    },

    _changeSelect : function () {
        var value =  this.ui.select.val();

        if (value === 'choose') {
            return;
        }

        var quality = _.find(this.qualities, { 'id': parseInt(value) });

        this.trigger('seasonedit:quality', { selected : quality });
        this.ui.select.val('choose');
    }
});