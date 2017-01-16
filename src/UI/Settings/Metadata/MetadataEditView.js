var vent = require('vent');
var Marionette = require('marionette');
var AsModelBoundView = require('../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../Mixins/AsValidatedView');
var AsEditModalView = require('../../Mixins/AsEditModalView');

var view = Marionette.ItemView.extend({
    template : 'Settings/Metadata/MetadataEditViewTemplate',

    _onAfterSave : function() {
        vent.trigger(vent.Commands.CloseModalCommand);
    }
});

AsModelBoundView.call(view);
AsValidatedView.call(view);
AsEditModalView.call(view);

module.exports = view;