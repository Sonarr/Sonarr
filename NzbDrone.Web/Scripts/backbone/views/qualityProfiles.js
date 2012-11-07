window.QualityProfilesView = Backbone.View.extend({

    initialize: function () {
        this.render();
    },

    render: function () {
        var profiles = this.model.models;
        var len = profiles.length;
        var startPos = (this.options.page - 1) * 8;
        var endPos = Math.min(startPos + 8, len);

        $(this.el).html('<ul class="thumbnails"></ul>');

        for (var i = startPos; i < endPos; i++) {
            $('.thumbnails', this.el).append(new QualityProfileView({ model: profiles[i] }).render().el);
        }

        return this;
    }
});

window.QualityProfileView = Backbone.View.extend({

    tagName: "li",

    initialize: function () {
        this.model.bind("change", this.render, this);
        this.model.bind("destroy", this.close, this);
    },

    render: function () {
        $(this.el).html(this.template(this.model.toJSON()));
        return this;
    }

});