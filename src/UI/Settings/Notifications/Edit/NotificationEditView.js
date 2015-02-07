var vent = require('vent');
var Marionette = require('marionette');
var DeleteView = require('../Delete/NotificationDeleteView');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
var AsEditModalView = require('../../../Mixins/AsEditModalView');
require('../../../Form/FormBuilder');
require('../../../Mixins/TagInput');

module.exports = (function(){
    var view = Marionette.ItemView.extend({
        template           : 'Settings/Notifications/Edit/NotificationEditViewTemplate',
        ui                 : {
            onDownloadToggle : '.x-on-download',
            onUpgradeSection : '.x-on-upgrade',
            tags             : '.x-tags'
        },
        events             : {
            'click .x-back'         : '_back',
            'change .x-on-download' : '_onDownloadChanged'
        },
        _deleteView        : DeleteView,
        initialize         : function(options){
            this.targetCollection = options.targetCollection;
        },
        onRender           : function(){
            this._onDownloadChanged();
            this.ui.tags.tagInput({
                model    : this.model,
                property : 'tags'
            });
        },
        _onAfterSave       : function(){
            this.targetCollection.add(this.model, {merge : true});
            vent.trigger(vent.Commands.CloseModalCommand);
        },
        _onAfterSaveAndAdd : function(){
            this.targetCollection.add(this.model, {merge : true});
            require('../Add/NotificationSchemaModal').open(this.targetCollection);
        },
        _back              : function(){
            if(this.model.isNew()) {
                this.model.destroy();
            }
            require('../Add/NotificationSchemaModal').open(this.targetCollection);
        },
        _onDownloadChanged : function(){
            var checked = this.ui.onDownloadToggle.prop('checked');
            if(checked) {
                this.ui.onUpgradeSection.show();
            }
            else {
                this.ui.onUpgradeSection.hide();
            }
        }
    });
    AsModelBoundView.call(view);
    AsValidatedView.call(view);
    AsEditModalView.call(view);
    return view;
}).call(this);