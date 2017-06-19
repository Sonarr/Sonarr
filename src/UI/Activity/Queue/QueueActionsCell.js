'use strict';

var $ = require('jquery');
var _ = require('underscore');
var vent = require('../../vent');
var TemplatedCell = require('../../Cells/TemplatedCell');
var RemoveFromQueueView = require('./RemoveFromQueueView');

module.exports = TemplatedCell.extend({

    template  : 'Activity/Queue/QueueActionsCellTemplate',
    className : 'queue-actions-cell',

    events : {
        'click .x-remove'        : '_remove',
        'click .x-manual-import' : '_manualImport',
        'click .x-grab'          : '_grab'
    },

    ui : {
        import : '.x-import',
        grab   : '.x-grab'
    },

    _remove : function() {
        var status = this.model.get('status');
        var showBlacklist = status !== 'Delay' && status !== 'DownloadClientUnavailable';

        vent.trigger(vent.Commands.OpenModalCommand, new RemoveFromQueueView({
            model         : this.model,
            showBlacklist : showBlacklist
        }));
    },

    _manualImport : function () {
        vent.trigger(vent.Commands.ShowManualImport,
            {
                downloadId: this.model.get('downloadId'),
                title: this.model.get('title')
            });
    },

    _grab : function() {
        var self = this;
        var data = _.omit(this.model.toJSON(), 'series', 'episode');

        var promise = $.ajax({
            url  : window.NzbDrone.ApiRoot + '/queue/grab',
            type : 'POST',
            data : JSON.stringify(data)
        });

        this.$(this.ui.grab).spinForPromise(promise);

        promise.success(function() {
            //find models that have the same series id and episode ids and remove them
            self.model.trigger('destroy', self.model);
        });
    }
});
