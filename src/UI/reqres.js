var Wreqr = require('./JsLibraries/backbone.wreqr');

var reqres = new Wreqr.RequestResponse();

reqres.Requests = {
    GetEpisodeFileById             : 'GetEpisodeFileById',
    GetAlternateNameBySeasonNumber : 'GetAlternateNameBySeasonNumber'
};

module.exports = reqres;