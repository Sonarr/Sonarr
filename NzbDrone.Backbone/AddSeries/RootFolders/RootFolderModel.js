define(['app'], function () {
    NzbDrone.AddSeries.RootFolders.RootFolderModel = Backbone.Model.extend({
        mutators: {
            freeSpaceString: function () {
                return this.get('freeSpace').bytes(2) + " Free";
            }
        },

        defaults: {
            freeSpace: 0,
        }
    });
});