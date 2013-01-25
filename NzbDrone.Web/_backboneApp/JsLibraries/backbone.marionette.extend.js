_.extend(Marionette.TemplateCache.prototype, {

    loadTemplate: function (templateId) {


        var template;

        console.log("Loading template '" + templateId + "'");

        jQuery.ajax({
            url: '_backboneApp//' + templateId + '.html',
            cache:false,
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


_.extend(Marionette.View.prototype, {
  
    

});