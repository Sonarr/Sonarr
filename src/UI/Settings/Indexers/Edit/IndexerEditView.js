var vent = require('../../../vent');
var AppLayout = require('../../../AppLayout');
var Marionette = require('marionette');
var DeleteView = require('../Delete/IndexerDeleteView');
var CommandController = require('../../../Commands/CommandController');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
var AsEditModalView = require('../../../Mixins/AsEditModalView');
require('../../../Form/FormBuilder');
require('../../../Mixins/AutoComplete');
require('bootstrap');
require('../Add/IndexerSchemaModal');

module.exports = (function(){
    var view = Marionette.ItemView.extend({
        template           : 'Settings/Indexers/Edit/IndexerEditViewTemplate',
        events             : {"click .x-back" : '_back'},
        _deleteView        : DeleteView,
        initialize         : function(options){
            this.targetCollection = options.targetCollection;
        },
        _onAfterSave       : function(){
            this.targetCollection.add(this.model, {merge : true});
            vent.trigger(vent.Commands.CloseModalCommand);
        },
        _onAfterSaveAndAdd : function(){
            this.targetCollection.add(this.model, {merge : true});
            require('../Add/IndexerSchemaModal').open(this.targetCollection);
        },
        _back              : function(){
            if(this.model.isNew()) {
                this.model.destroy();
            }
            require('../Add/IndexerSchemaModal').open(this.targetCollection);
        }
    });
    AsModelBoundView.call(view);
    AsValidatedView.call(view);
    AsEditModalView.call(view);
    return view;
}).call(this);