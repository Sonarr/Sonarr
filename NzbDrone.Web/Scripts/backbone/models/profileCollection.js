window.ProfileCollection = Backbone.Collection.extend({

    model: Profile,

    url: '/api/qualityprofiles',

    search: function (searchTerm, options) {

        var self = this;
        this.fetch({
            success: function () {
                if (options.success) {
                    options.success();
                }
            }
        });
    }
});