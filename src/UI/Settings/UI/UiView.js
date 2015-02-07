var vent = require('vent');
var Marionette = require('marionette');
var UiSettingsModel = require('../../Shared/UiSettingsModel');
var AsModelBoundView = require('../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../Mixins/AsValidatedView');

module.exports = (function(){
    var view = Marionette.ItemView.extend({
        template          : 'Settings/UI/UiViewTemplate',
        initialize        : function(){
            this.listenTo(this.model, 'sync', this._reloadUiSettings);
        },
        _reloadUiSettings : function(){
            UiSettingsModel.fetch();
        }
    });
    AsModelBoundView.call(view);
    AsValidatedView.call(view);
    return view;
}).call(this);