var _ = require('underscore');
var $ = require('jquery');
var AppLayout = require('../../../AppLayout');
var Marionette = require('marionette');
var EditView = require('../Edit/DownloadClientEditView');

module.exports = Marionette.ItemView.extend({
    template  : 'Settings/DownloadClient/Add/DownloadClientAddItemViewTemplate',
    tagName   : 'li',
    className : 'add-thingy-item',

    events : {
        'click .x-preset' : '_addPreset',
        'click'           : '_add'
    },

    initialize : function(options) {
        this.targetCollection = options.targetCollection;
    },

    _addPreset : function(e) {
        var presetName = $(e.target).closest('.x-preset').attr('data-id');

        var presetData = _.where(this.model.get('presets'), { name : presetName })[0];

        this.model.set(presetData);

        this.model.set({
            id     : undefined,
            enable : true
        });

        var editView = new EditView({
            model            : this.model,
            targetCollection : this.targetCollection
        });

        AppLayout.modalRegion.show(editView);
    },

    _add : function(e) {
        if ($(e.target).closest('.btn,.btn-group').length !== 0 && $(e.target).closest('.x-custom').length === 0) {
            return;
        }

        this.model.set({
            id     : undefined,
            enable : true
        });

        var editView = new EditView({
            model            : this.model,
            targetCollection : this.targetCollection
        });

        AppLayout.modalRegion.show(editView);
    }
});