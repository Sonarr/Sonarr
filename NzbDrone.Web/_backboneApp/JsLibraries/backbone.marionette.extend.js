_.extend(Marionette.TemplateCache.prototype, {

    loadTemplate: function (templateId) {

        var template;

        jQuery.ajax({
            url: 'Backbone.NzbDrone//' + templateId + '.html',
            async: false

        }).done(function (data) {
            template = data;

        }).fail(function (data) {
            console.log("couldn't load template " + this.templateId + " Error: " + data.statusText);
            template = "<div class='alert alert-error'>Couldn't load template '" + templateId + "'. Status: " + data.statusText + "</div>";
        });

        return template;
    }
});