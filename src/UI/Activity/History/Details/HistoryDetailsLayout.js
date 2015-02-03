var $ = require('jquery');
var vent = require('../../../vent');
var Marionette = require('marionette');
var HistoryDetailsView = require('./HistoryDetailsView');

module.exports = Marionette.Layout.extend({
    template      : 'Activity/History/Details/HistoryDetailsLayoutTemplate',
    regions       : {bodyRegion : '.modal-body'},
    events        : {"click .x-mark-as-failed" : '_markAsFailed'},
    onShow        : function(){
        this.bodyRegion.show(new HistoryDetailsView({model : this.model}));
    },
    _markAsFailed : function(){
        var url = window.NzbDrone.ApiRoot + '/history/failed';
        var data = {id : this.model.get('id')};
        $.ajax({
            url  : url,
            type : 'POST',
            data : data
        });
        vent.trigger(vent.Commands.CloseModalCommand);
    }
});