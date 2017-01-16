var Marionette = require('marionette');
var QualityDefinitionItemView = require('./QualityDefinitionItemView');

module.exports = Marionette.CompositeView.extend({
    template : 'Settings/Quality/Definition/QualityDefinitionCollectionTemplate',

    itemViewContainer : '.x-rows',

    itemView : QualityDefinitionItemView
});