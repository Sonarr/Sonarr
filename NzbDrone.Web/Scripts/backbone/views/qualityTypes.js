QualityTypeView = Backbone.Marionette.ItemView.extend({
    tagName: "div",
    className: "quality-type",
    template: QualityTypeApp.Constants.Templates.QualityType,
    events: {
        'change .slider-value': 'changeSize'
    },
    changeSize: function (e) {
        var target = $(e.target);
        var maxSize = parseInt($(target).val());
        
        this.model.set({ "MaxSize": maxSize });
        this.model.save();
    }
});

QualityTypeCollectionView = Backbone.Marionette.CompositeView.extend({
    tagName: "div",
    id: "quality-type-collection",
    itemView: QualityTypeView,
    template: QualityTypeApp.Constants.Templates.QualityTypeCollection,
    
    initialize: function () {
        _.bindAll(this, 'render');
        this.collection = new QualityTypeCollection();
        this.collection.fetch();
        this.collection.bind('reset', this.render);
    }
});