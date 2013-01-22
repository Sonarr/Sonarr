_.extend(Marionette.TemplateCache.prototype, {

    loadTemplate: function (templateId) {

        var template = "<div class='alert alert-error'>TEMPLATE [" + templateId + "] NOT FOUND</div>";

        jQuery.ajax({
            url: 'Backbone.NzbDrone//' + templateId + '.html',
            async: false

        }).done(function (data) {
            template = data;

        }).fail(function (data) {
            console.log("couldn't load template " + this.templateId + " Error: " + data.statusText);
        });

        return template;
    }
});