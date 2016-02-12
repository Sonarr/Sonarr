var Wreqr = require('./JsLibraries/backbone.wreqr');

var reqres = new Wreqr.RequestResponse();

reqres.Requests = {
    GetEpisodeFileById                  : 'GetEpisodeFileById',
    GetAlternateNameBySeasonNumber      : 'GetAlternateNameBySeasonNumber',
    GetAlternateNameBySceneSeasonNumber : 'GetAlternateNameBySceneSeasonNumber'
};

module.exports = reqres;