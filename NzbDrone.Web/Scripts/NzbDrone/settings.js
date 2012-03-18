$('#SeriesName').live('change', function () { createExamples(); });
$('#EpisodeName').live('change', function () { createExamples(); });
$('#ReplaceSpaces').live('change', function () { createExamples(); });
$('#AppendQuality').live('change', function () { createExamples(); });
$('#SeparatorStyle').live('change', function () { createExamples(); });
$('#NumberStyle').live('change', function () { createExamples(); });
$('#MultiEpisodeStyle').live('change', function () { createExamples(); });

var testProwlUrl = '../Command/TestProwl';
var testSabUrl = '../Command/TestSabnzbd';


function createExamples() {
    createSingleEpisodeExample();
    createMultiEpisodeExample();
}

function createSingleEpisodeExample() {
    var result = '';

    var separator = ' - ';

    if ($("#SeparatorStyle option:selected").val() == 1)
        separator = ' ';

    if ($('#SeriesName').attr('checked')) {
        result += 'Series Name';
        result += separator;
    }

    result += $("#NumberStyle option:selected").text();

    if ($('#EpisodeName').attr('checked')) {
        result += separator;
        result += 'Episode Name';
    }

    if ($('#AppendQuality').attr('checked'))
        result += ' [TV]';

    if ($('#ReplaceSpaces').attr('checked'))
        result = result.replace(/\s/g, '.');

    $('#singleEpisodeExample').children('.result').text(result);
}

function createMultiEpisodeExample() {
    var result = '';

    var separator = ' - ';

    if ($("#SeparatorStyle option:selected").val() == 1)
        separator = ' ';

    if ($('#SeriesName').attr('checked')) {
        result += 'Series Name';
        result += separator;
    }

    var numberStyle = $("#NumberStyle option:selected").text();
    var numberId = $("#NumberStyle option:selected").val();
    var style = $("#MultiEpisodeStyle option:selected").val();

    result += numberStyle;

    if (style == 0)
        result += '-06';

    if (style == 1) {
        result += separator;
        result += numberStyle.replace('5', '6');
    }

    if (style == 2) {
        if (numberId <= 1)
            result += 'x06';

        if (numberId == 2)
            result += 'E06';

        if (numberId == 3)
            result += 'e06';
    }

    if (style == 3) {
        if (numberId <= 1)
            result += '-x06';

        if (numberId == 2)
            result += '-E06';

        if (numberId == 3)
            result += '-e06';
    }

    if ($('#EpisodeName').attr('checked')) {
        result += separator;
        result += 'Episode Name';
    }

    if ($('#AppendQuality').attr('checked'))
        result += ' [TV]';

    if ($('#ReplaceSpaces').attr('checked'))
        result = result.replace(/\s/g, '.');

    $('#multiEpisodeExample').children('.result').text(result);
}

function testProwl(event) {
    var apiKeys = $('#ProwlApiKeys').val();

    $.ajax({
        type: "GET",
        url: testProwlUrl,
        data: jQuery.param({ apiKeys: apiKeys })
    });

    event.preventDefault();
}

function testSabnzbd(event) {
    var host = $('#SabHost').val();
    var port = $('#SabPort').val();
    var apiKey = $('#SabApiKey').val();
    var username = $('#SabUsername').val();
    var password = $('#SabPassword').val();

    $.ajax({
        type: "GET",
        url: testSabUrl,
        data: jQuery.param({ host: host, port: port, apiKey: apiKey, username: username, password: password })
    });

    event.preventDefault();
}