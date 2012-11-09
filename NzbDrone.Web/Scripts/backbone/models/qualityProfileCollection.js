window.QualityProfileCollection = Backbone.Collection.extend({
    model: QualityProfile,
    url: '/api/qualityprofiles'
});