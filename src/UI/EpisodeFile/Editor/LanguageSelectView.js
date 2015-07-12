var _ = require('underscore');
var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template : 'EpisodeFile/Editor/LanguageSelectViewTemplate',

    ui : {
        select : '.x-select'
    },

    events : {
        'change .x-select' : '_changeSelect'
    },

    initialize : function (options) {
        this.languages = options.languages;

        this.templateHelpers = {
            languages : this.languages
        };
    },

    _changeSelect : function () {
        var value =  this.ui.select.val();

        if (value === 'choose') {
            return;
        }

        var language = _.find(this.languages, { 'id': parseInt(value) });

        this.trigger('seasonedit:language', { selected : language });
        this.ui.select.val('choose');
    }
});