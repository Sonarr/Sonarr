window.QualityTypeCollection = Backbone.Collection.extend({
    model: QualityType,
    url: '/api/qualitytypes'
});