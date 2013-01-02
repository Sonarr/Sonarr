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

        this.model.set({ "Name": name });
        this.model.save(this.model, {
            success: function (model) {
                var id = model.get('Id');
                var name = model.get('Name');

                var exists = $('#DefaultQualityProfileId option[value="' + id + '"]');

                if (exists.length == 0) 
                    $('#DefaultQualityProfileId').append($('\<option\> \</option\>').val(id).html(name));

                else
                    $('#DefaultQualityProfileId option[value="' + id + '"]').html(name);
            }
        });
    },
    destroy: function (e) {
        e.preventDefault();
        
        $("#DefaultQualityProfileId option[value='" + this.model.get('Id') + "']").remove();

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
        
        //Todo: Need to get the default profile from the server, instead of creating it manually...
        var newProfile = new QualityProfile({
            Name: 'New Profile', Cutoff: 0, Qualities: [
                { "Id": 1, "Weight": 1, "Name": "SDTV", "Allowed": false },
                { "Id": 8, "Weight": 2, "Name": "WEBDL-480p", "Allowed": false },
                { "Id": 2, "Weight": 3, "Name": "DVD", "Allowed": false },
                { "Id": 4, "Weight": 4, "Name": "HDTV-720p", "Allowed": false },
                { "Id": 9, "Weight": 5, "Name": "HDTV-1080p", "Allowed": false },
                { "Id": 5, "Weight": 6, "Name": "WEBDL-720p", "Allowed": false },
                { "Id": 3, "Weight": 6, "Name": "WEBDL-1080p", "Allowed": false },
                { "Id": 6, "Weight": 8, "Name": "Bluray720p", "Allowed": false },
                { "Id": 7, "Weight": 9, "Name": "Bluray1080p", "Allowed": false }
            ]
        });
        //Todo: It would be nice to not have to save this on add (via create)
        this.collection.create(newProfile);
    }
});