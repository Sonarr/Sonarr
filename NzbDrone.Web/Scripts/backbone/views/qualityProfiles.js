QualityProfileView = Backbone.Marionette.ItemView.extend({
    tagName: "div",
    className: "quality-profile",
    template: "#QualityProfileTemplate",
    events: {
        'click .quality-selectee': 'toggleAllowed'
    },
    toggleAllowed: function (e) {
        //Add to cutoff
        //Update model

        var checked = $(e.target).attr('checked') != undefined;
        this.model.set({  });
    }
});

QualityProfileCollectionView = Backbone.Marionette.CompositeView.extend({
    tagName: "div",
    id: "quality-profile-collection",
    itemView: QualityProfileView,
    template: QualityProfileApp.Constants.Templates.QualityProfileCollection,
    
    //appendHtml: function (collectionView, itemView) {
    //    collectionView.$('#collection').append(itemView.el);
    //},
    
    initialize: function () {
        _.bindAll(this, 'render');
        this.collection = new QualityProfileCollection();
        this.collection.fetch();
        this.collection.bind('reset', this.render);
    }
});