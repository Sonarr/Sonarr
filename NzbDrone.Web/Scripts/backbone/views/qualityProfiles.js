QualityProfileView = Backbone.Marionette.ItemView.extend({
    tagName: "div",
    className: "quality-profile",
    template: "#QualityProfileTemplate",
    initialize: function() {
        this.model.on('destroy', this.remove, this)
    },
    events: {
        'click .quality-selectee': 'toggleAllowed',
        'change .cutoff': 'changeCutoff',
        'change .name': 'changeName',
        'click .remove-profile': 'destroy'
    },
    toggleAllowed: function (e) {
        //Add to cutoff
        //Update model
        var target = $(e.target);
        var el = $(this.el);

        var checked = $(target).attr('checked') != undefined;
        var id = this.model.get("Id");
        
        var qualities = _.clone(this.model.get("Qualities"));
        _.each(qualities, function (qualityType) {
            var qualityId = parseInt($(target).attr('data-quality-id'));

            if (qualityType.Id == qualityId) {
                qualityType.Allowed = checked;
               
                //Find cutoff dropdown
                var cutoff = $(el).find('.cutoff');

                if (checked) {
                    $('<option>' + qualityType.Name + '</option>').val(qualityId).appendTo(cutoff);
                }
                
                else {
                    $(cutoff).find('option[value="' + qualityId + '"]').remove();
                }

                //Todo: auto-sort by weight (which is not the value)
                //$(cutoff).html($($(cutoff).children("option")).sort(function(a, b) {
                //    return a.value == b.value ? 0 : a.value < b.value ? -1 : 1;
                //}));
            }
        });

        this.model.set({ "Qualities": qualities });
        this.model.save();
    },
    changeCutoff: function(e) {
        var cutoff = $(e.target).val();
        
        this.model.set({ "Cutoff": cutoff });
        this.model.save();
    },
    changeName: function(e) {
        var name = $(e.target).val();

        $('#DefaultQualityProfileId option[value="' + this.model.get("Id") + '"]').html(name);

        this.model.set({ "Name": name });
        this.model.save();
    },
    destroy: function (e) {
        //if (e === undefined)
        //    return;

        e.preventDefault();
        this.model.destroy();
        e.stopPropagation();
    },
    remove: function (e) {
        $(this.el).remove();
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
    },
    events: {
        'click #add-profile': 'addProfile'
    },
    addProfile: function (e) {
        e.preventDefault();
        //Add new profile to collection
        //Todo: How will we get the list of qualities (they would all be NOT allowed) - it all comes from the server side...
        var newProfile = new QualityProfile({
            Name: '', Cutoff: 0, Qualities: [
                { "Id": 0, "Weight": 0, "Name": "Unknown", "Allowed": false },
                { "Id": 1, "Weight": 1, "Name": "SDTV", "Allowed": false },
                { "Id": 2, "Weight": 2, "Name": "DVD", "Allowed": false },
                { "Id": 4, "Weight": 4, "Name": "HDTV", "Allowed": false },
                { "Id": 5, "Weight": 5, "Name": "WEBDL-720p", "Allowed": false },
                { "Id": 3, "Weight": 7, "Name": "WEBDL-1080p", "Allowed": false },
                { "Id": 6, "Weight": 6, "Name": "Bluray720p", "Allowed": false },
                { "Id": 7, "Weight": 8, "Name": "Bluray1080p", "Allowed": false }
            ]
        });

        this.collection.add(newProfile);      
    }
});