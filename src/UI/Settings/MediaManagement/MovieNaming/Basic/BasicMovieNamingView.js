var _ = require('underscore');
var Marionette = require('marionette');
var Config = require('../../../../Config');
var BasicMovieNamingModel = require('./BasicMovieNamingModel');
var AsModelBoundView = require('../../../../Mixins/AsModelBoundView');

var view = Marionette.ItemView.extend({
    template : 'Settings/MediaManagement/MovieNaming/Basic/BasicMovieNamingViewTemplate',

    initialize : function(options) {
        this.namingModel = options.model;
        this.model = new BasicMovieNamingModel();

        this._parseNamingModel();

        this.listenTo(this.model, 'change', this._buildFormat);
        this.listenTo(this.namingModel, 'sync', this._parseNamingModel);
    },

    _parseNamingModel : function() {
        var standardFormat = this.namingModel.get('standardMovieFormat');

        var includeTitle = standardFormat.match(/\{Movie[-_. ]Title\}/i);
        var includeQuality = standardFormat.match(/\{Quality[-_. ]Title\}/i);
        if (includeQuality === null) {
            includeQuality = standardFormat.match(/\{Quality[-_. ]Full\}/i);
        }
        var replaceSpaces = standardFormat.indexOf(' ') === -1;
        var separator = standardFormat.match(/\}( - |\.-\.|\.| )|( - |\.-\.|\.| )\{/i);

        if (separator === null || separator[1] === '.-.') {
            separator = ' - ';
        } else {
            separator = separator[1];
        }

        this.model.set({
            includeMovieTitle   : includeTitle !== null,
            includeMovieQuality : includeQuality !== null,
            replaceSpaces       : replaceSpaces,
            separator           : separator
        }, { silent : true });
    },

    _buildFormat : function() {
        if (Config.getValueBoolean(Config.Keys.AdvancedSettings)) {
            return;
        }

        var standardMovieFormat = '';

        if (this.model.get('includeMovieTitle')) {
            if (this.model.get('replaceSpaces')) {
                standardMovieFormat += '{Movie.Title}';
            } else {
                standardMovieFormat += '{Movie Title}';
            }
        }

        if (this.model.get('includeMovieQuality')) {
            standardMovieFormat += this.model.get('separator');

            if (this.model.get('replaceSpaces')) {
                standardMovieFormat += ' {Quality.Title}';
            } else {
                standardMovieFormat += ' {Quality Title}';
            }
        }

        if (this.model.get('replaceSpaces')) {
            standardMovieFormat = standardMovieFormat.replace(/\s/g, '.');
        }

        this.namingModel.set('standardMovieFormat', standardMovieFormat);
    }
});

module.exports = AsModelBoundView.call(view);