QualityProfileView = Backbone.Marionette.ItemView.extend({
    tagName: "div",
    className: "quality-profile",
    template: "#QualityProfileTemplate"
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