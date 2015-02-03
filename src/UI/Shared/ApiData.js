var $ = require('jquery');
require('../Mixins/jquery.ajax');

module.exports = {
    get : function(resource){
        var url = window.NzbDrone.ApiRoot + '/' + resource;
        var _data;
        $.ajax({
            url   : url,
            async : false
        }).done(function(data){
            _data = data;
        }).error(function(xhr, status, error){
            throw error;
        });
        return _data;
    }
};