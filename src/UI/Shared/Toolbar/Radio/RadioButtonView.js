var Marionette = require('marionette');
var Config = require('../../../Config');

module.exports = Marionette.ItemView.extend({
    template       : 'Shared/Toolbar/RadioButtonTemplate',
    className      : 'btn btn-default',
    ui             : {icon : 'i'},
    events         : {"click" : 'onClick'},
    initialize     : function(){
        this.storageKey = this.model.get('menuKey') + ':' + this.model.get('key');
    },
    onRender       : function(){
        if(this.model.get('active')) {
            this.$el.addClass('active');
            this.invokeCallback();
        }
        if(!this.model.get('title')) {
            this.$el.addClass('btn-icon-only');
        }
        if(this.model.get('tooltip')) {
            this.$el.attr('title', this.model.get('tooltip'));
        }
    },
    onClick        : function(){
        Config.setValue(this.model.get('menuKey'), this.model.get('key'));
        this.invokeCallback();
    },
    invokeCallback : function(){
        if(!this.model.ownerContext) {
            throw 'ownerContext must be set.';
        }
        var callback = this.model.get('callback');
        if(callback) {
            callback.call(this.model.ownerContext, this);
        }
    }
});