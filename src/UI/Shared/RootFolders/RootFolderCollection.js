var Backbone = require('backbone');
var RootFolderModel = require('./RootFolderModel');
require('../../Mixins/backbone.signalr.mixin');

var RootFolderCollection = Backbone.Collection.extend({
    url   : window.NzbDrone.ApiRoot + '/rootfolder',
    model : RootFolderModel
});

module.exports = new RootFolderCollection();