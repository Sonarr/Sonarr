/// <reference path="../../app.js" />
NzbDrone.AddSeries.RootDirModel = Backbone.Model.extend({
    idAttribute: 'Id',

    mutators: {
        FreeSpaceString: function () {
        	return this.get('FreeSpace').bytes(2) + " Free";
        }
    },

    defaults: {
        FreeSpace: 0,
    }
});
