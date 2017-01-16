var Marionette = require('marionette');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');

var view = Marionette.ItemView.extend({
    template : 'Settings/MediaManagement/Sorting/SortingViewTemplate'
});

AsModelBoundView.call(view);
AsValidatedView.call(view);

module.exports = view;