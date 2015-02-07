var _ = require('underscore');
var vent = require('vent');
var AppLayout = require('../../../AppLayout');
var Marionette = require('marionette');
var DeleteView = require('./RemotePathMappingDeleteView');
var CommandController = require('../../../Commands/CommandController');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
var AsEditModalView = require('../../../Mixins/AsEditModalView');
require('../../../Mixins/FileBrowser');
require('bootstrap');

module.exports = (function(){
    var view = Marionette.ItemView.extend({
        template     : 'Settings/DownloadClient/RemotePathMapping/RemotePathMappingEditViewTemplate',
        ui           : {
            path      : '.x-path',
            modalBody : '.modal-body'
        },
        _deleteView  : DeleteView,
        initialize   : function(options){
            this.targetCollection = options.targetCollection;
        },
        onShow       : function(){
            if(this.ui.path.length > 0) {
                this.ui.modalBody.addClass('modal-overflow');
            }
            this.ui.path.fileBrowser();
        },
        _onAfterSave : function(){
            this.targetCollection.add(this.model, {merge : true});
            vent.trigger(vent.Commands.CloseModalCommand);
        }
    });
    AsModelBoundView.call(view);
    AsValidatedView.call(view);
    AsEditModalView.call(view);
    return view;
}).call(this);