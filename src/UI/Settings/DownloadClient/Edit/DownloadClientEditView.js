var vent = require('../../../vent');
var Marionette = require('marionette');
var DeleteView = require('../Delete/DownloadClientDeleteView');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
var AsEditModalView = require('../../../Mixins/AsEditModalView');
require('../../../Form/FormBuilder');
require('../../../Mixins/FileBrowser');
require('bootstrap');

module.exports = (function(){
    var view = Marionette.ItemView.extend({
        template           : 'Settings/DownloadClient/Edit/DownloadClientEditViewTemplate',
        ui                 : {
            path      : '.x-path',
            modalBody : '.modal-body'
        },
        events             : {
            'click .x-back' : '_back'
        },
        _deleteView        : DeleteView,
        initialize         : function(options){
            this.targetCollection = options.targetCollection;
        },
        onShow             : function(){
            if(this.ui.path.length > 0) {
                this.ui.modalBody.addClass('modal-overflow');
            }
            this.ui.path.fileBrowser();
        },
        _onAfterSave       : function(){
            this.targetCollection.add(this.model, {merge : true});
            vent.trigger(vent.Commands.CloseModalCommand);
        },
        _onAfterSaveAndAdd : function(){
            this.targetCollection.add(this.model, {merge : true});
            require('../Add/DownloadClientSchemaModal').open(this.targetCollection);
        },
        _back              : function(){
            require('../Add/DownloadClientSchemaModal').open(this.targetCollection);
        }
    });
    AsModelBoundView.call(view);
    AsValidatedView.call(view);
    AsEditModalView.call(view);
    return view;
}).call(this);