QualityProfileView = Backbone.Marionette.ItemView.extend({
    tagName: "div",
    className: "quality-profile",
    template: "#QualityProfileTemplate",
    events: {
        'click .quality-selectee': 'toggleAllowed',
        'change .cutoff': 'changeCutoff',
        'change .name': 'changeName'
    },
    toggleAllowed: function (e) {
        //Add to cutoff
        //Update model
        var target = $(e.target);

        var checked = $(target).attr('checked') != undefined;
        var id = this.model.get("Id");
        
        var qualities = _.clone(this.model.get("Qualities"));
        _.each(qualities, function (qualityType) {
            var qualityId = parseInt($(target).attr('data-quality-id'));

            if (qualityType.Id == qualityId) {
                qualityType.Allowed = checked;
                
                //Todo: Add/Remove from cutoff
                //Find cutoff dropdown
                var cutoff = ('select#' + id);

                if (checked) {
                    $('<option>' + qualityType.Name + '</option>').val(qualityId).appendTo(cutoff);
                }
                
                else {
                    $(cutoff).find('option[value="' + qualityId + '"]').remove();
                }
            }
        });

        this.model.set({ "Qualities": qualities });
        this.model.save();
    },
    changeCutoff: function(e) {
        //Todo: save change
        var cutoff = $(e.target).val();
        
        this.model.set({ "Cutoff": cutoff });
        this.model.save();
    },
    changeName: function(e) {
        var name = $(e.target).val();

        //Todo: update default quality dropdown
        $('#DefaultQualityProfileId option[value="' + this.model.get("Id") + '"]').html(name);

        this.model.set({ "Name": name });
        this.model.save();
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