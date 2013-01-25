/// <reference path="../../app.js" />
NzbDrone.AddSeries.RootDirModel = Backbone.Model.extend({

    mutators: {
        freeSpaceString: function () {
        	return this.get('freeSpace').bytes(2) + " Free";
        }
    },

    defaults: {
        freeSpace: 0,
    }
});
