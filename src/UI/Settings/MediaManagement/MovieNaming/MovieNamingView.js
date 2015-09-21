var _ = require('underscore');
var Marionette = require('marionette');
var NamingSampleModel = require('../Naming/NamingSampleModel');
var BasicMovieNamingView = require('./Basic/BasicMovieNamingView');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');

module.exports = (function() {
    var view = Marionette.Layout.extend({
        template                            : 'Settings/MediaManagement/MovieNaming/MovieNamingViewTemplate',
        ui                                  : {
            namingOptions            : '.x-move-naming-options',
            renameMoviesCheckbox     : '.x-rename-movies',
            standardMovieExample     : '.x-movie-example',
            namingTokenHelper        : '.x-naming-token-helper',
            movieFolderExample       : '.x-movie-folder-example'
        },
        events                              : {
            'change .x-rename-movies'       : '_setFailedDownloadOptionsVisibility',
            'click .x-show-wizard'          : '_showWizard',
            'click .x-naming-token-helper a': '_addToken',
        },
        regions                             : { basicNamingRegion : '.x-basic-movie-naming' },
        onRender                            : function() {
            if (!this.model.get('renameEpisodes')) {
                this.ui.namingOptions.hide();
            }
            var basicNamingView = new BasicMovieNamingView({ model : this.model });
            this.basicNamingRegion.show(basicNamingView);
            this.namingSampleModel = new NamingSampleModel();
            this.listenTo(this.model, 'change', this._updateSamples);
            this.listenTo(this.namingSampleModel, 'sync', this._showSamples);
            this._updateSamples();
        },
        _setFailedDownloadOptionsVisibility : function() {
            var checked = this.ui.renameMoviesCheckbox.prop('checked');
            if (checked) {
                this.ui.namingOptions.slideDown();
            } else {
                this.ui.namingOptions.slideUp();
            }
        },
        _updateSamples                      : function() {
            this.namingSampleModel.fetch({ data : this.model.toJSON() });
        },
        _showSamples                        : function() {
            this.ui.standardMovieExample.html(this.namingSampleModel.get('standardMovieExample'));
            this.ui.movieFolderExample.html(this.namingSampleModel.get('movieFolderExample'));
        },
        _addToken                           : function(e) {
            e.preventDefault();
            e.stopPropagation();
            var target = e.target;
            var token = '';
            var input = this.$(target).closest('.x-helper-input').children('input');
            if (this.$(target).attr('data-token')) {
                token = '{{0}}'.format(this.$(target).attr('data-token'));
            } else {
                token = this.$(target).attr('data-separator');
            }
            input.val(input.val() + token);
            input.change();
            this.ui.namingTokenHelper.removeClass('open');
            input.focus();
        }
    });
    AsModelBoundView.call(view);
    AsValidatedView.call(view);
    return view;
}).call(this);