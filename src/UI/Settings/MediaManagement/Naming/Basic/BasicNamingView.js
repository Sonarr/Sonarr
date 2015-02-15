var _ = require('underscore');
var Marionette = require('marionette');
var Config = require('../../../../Config');
var NamingSampleModel = require('../NamingSampleModel');
var BasicNamingModel = require('./BasicNamingModel');
var AsModelBoundView = require('../../../../Mixins/AsModelBoundView');

var view = Marionette.ItemView.extend({
    template : 'Settings/MediaManagement/Naming/Basic/BasicNamingViewTemplate',

    ui : {
        namingOptions        : '.x-naming-options',
        singleEpisodeExample : '.x-single-episode-example',
        multiEpisodeExample  : '.x-multi-episode-example',
        dailyEpisodeExample  : '.x-daily-episode-example'
    },

    initialize : function(options) {
        this.namingModel = options.model;
        this.model = new BasicNamingModel();

        this._parseNamingModel();

        this.listenTo(this.model, 'change', this._buildFormat);
        this.listenTo(this.namingModel, 'sync', this._parseNamingModel);
    },

    _parseNamingModel : function() {
        var standardFormat = this.namingModel.get('standardEpisodeFormat');

        var includeSeriesTitle = standardFormat.match(/\{Series[-_. ]Title\}/i);
        var includeEpisodeTitle = standardFormat.match(/\{Episode[-_. ]Title\}/i);
        var includeQuality = standardFormat.match(/\{Quality[-_. ]Title\}/i);
        var numberStyle = standardFormat.match(/s?\{season(?:\:0+)?\}[ex]\{episode(?:\:0+)?\}/i);
        var replaceSpaces = standardFormat.indexOf(' ') === -1;
        var separator = standardFormat.match(/\}( - |\.-\.|\.| )|( - |\.-\.|\.| )\{/i);

        if (separator === null || separator[1] === '.-.') {
            separator = ' - ';
        } else {
            separator = separator[1];
        }

        if (numberStyle === null) {
            numberStyle = 'S{season:00}E{episode:00}';
        } else {
            numberStyle = numberStyle[0];
        }

        this.model.set({
            includeSeriesTitle  : includeSeriesTitle !== null,
            includeEpisodeTitle : includeEpisodeTitle !== null,
            includeQuality      : includeQuality !== null,
            numberStyle         : numberStyle,
            replaceSpaces       : replaceSpaces,
            separator           : separator
        }, { silent : true });
    },

    _buildFormat : function() {
        if (Config.getValueBoolean(Config.Keys.AdvancedSettings)) {
            return;
        }

        var standardEpisodeFormat = '';
        var dailyEpisodeFormat = '';

        if (this.model.get('includeSeriesTitle')) {
            if (this.model.get('replaceSpaces')) {
                standardEpisodeFormat += '{Series.Title}';
                dailyEpisodeFormat += '{Series.Title}';
            } else {
                standardEpisodeFormat += '{Series Title}';
                dailyEpisodeFormat += '{Series Title}';
            }

            standardEpisodeFormat += this.model.get('separator');
            dailyEpisodeFormat += this.model.get('separator');
        }

        standardEpisodeFormat += this.model.get('numberStyle');
        dailyEpisodeFormat += '{Air-Date}';

        if (this.model.get('includeEpisodeTitle')) {
            standardEpisodeFormat += this.model.get('separator');
            dailyEpisodeFormat += this.model.get('separator');

            if (this.model.get('replaceSpaces')) {
                standardEpisodeFormat += '{Episode.Title}';
                dailyEpisodeFormat += '{Episode.Title}';
            } else {
                standardEpisodeFormat += '{Episode Title}';
                dailyEpisodeFormat += '{Episode Title}';
            }
        }

        if (this.model.get('includeQuality')) {
            if (this.model.get('replaceSpaces')) {
                standardEpisodeFormat += ' {Quality.Title}';
                dailyEpisodeFormat += ' {Quality.Title}';
            } else {
                standardEpisodeFormat += ' {Quality Title}';
                dailyEpisodeFormat += ' {Quality Title}';
            }
        }

        if (this.model.get('replaceSpaces')) {
            standardEpisodeFormat = standardEpisodeFormat.replace(/\s/g, '.');
            dailyEpisodeFormat = dailyEpisodeFormat.replace(/\s/g, '.');
        }

        this.namingModel.set('standardEpisodeFormat', standardEpisodeFormat);
        this.namingModel.set('dailyEpisodeFormat', dailyEpisodeFormat);
        this.namingModel.set('animeEpisodeFormat', standardEpisodeFormat);
    }
});

module.exports = AsModelBoundView.call(view);