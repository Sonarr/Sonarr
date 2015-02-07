'use strict';

var $ = require('jquery');
var vent = require('../../vent');
var Marionette = require('marionette');
var TemplatedCell = require('../../Cells/TemplatedCell');
var RemoveFromQueueView = require('./RemoveFromQueueView');

module.exports = TemplatedCell.extend({

    template  : 'Activity/Queue/QueueActionsCellTemplate',
    className : 'queue-actions-cell',

    events : {
        'click .x-remove' : '_remove',
        'click .x-import' : '_import',
        'click .x-grab'   : '_grab'
    },

    ui : {
        import : '.x-import',
        grab   : '.x-grab'
    },

    _remove : function() {
        var showBlacklist = this.model.get('status') !== 'Pending';

        vent.trigger(vent.Commands.OpenModalCommand, new RemoveFromQueueView({
            model         : this.model,
            showBlacklist : showBlacklist
        }));
    },

    _import : function() {
        var self = this;

        var promise = $.ajax({
            url  : window.NzbDrone.ApiRoot + '/queue/import',
            type : 'POST',
            data : JSON.stringify(this.model.toJSON())
        });

        $(this.ui.import).spinForPromise(promise);

        promise.success(function() {
            //find models that have the same series id and episode ids and remove them
            self.model.trigger('destroy', self.model);
        });
    },

    _grab : function() {
        var self = this;

        var promise = $.ajax({
            url  : window.NzbDrone.ApiRoot + '/queue/grab',
            type : 'POST',
            data : JSON.stringify(this.model.toJSON())
        });

        $(this.ui.grab).spinForPromise(promise);

        promise.success(function() {
            //find models that have the same series id and episode ids and remove them
            self.model.trigger('destroy', self.model);
        });
    }
});
