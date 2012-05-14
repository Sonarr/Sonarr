$('#SeriesName').live('change', function () { createExamples(); });
$('#EpisodeName').live('change', function () { createExamples(); });
$('#ReplaceSpaces').live('change', function () { createExamples(); });
$('#AppendQuality').live('change', function () { createExamples(); });
$('#SeparatorStyle').live('change', function () { createExamples(); });
$('#NumberStyle').live('change', function () { createExamples(); });
$('#MultiEpisodeStyle').live('change', function () { createExamples(); });

var testProwlUrl = '../Command/TestProwl';
var testSabUrl = '../Command/TestSabnzbd';
var testEmailUrl = '../Command/TestEmail';


function createExamples() {
    createSingleEpisodeExample();
    createMultiEpisodeExample();
}

function createSingleEpisodeExample() {
    var result = '';

    var separator = ' - ';

    if ($("#SeparatorStyle option:selected").val() == 1)
        separator = ' ';
    
    if ($("#SeparatorStyle option:selected").val() == 2)
        separator = '.';

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
    
    if ($("#SeparatorStyle option:selected").val() == 2)
        separator = '.';

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

//Twitter
getAuthorizationUrl = '../Command/GetTwitterAuthorization';
verifyAuthorizationUrl = '../Command/VerifyTwitterAuthorization';

function requestTwitterAuthorization() {
    $.ajax({
        type: "GET",
        url: getAuthorizationUrl,
        error: function (req, status, error) {
            alert("Sorry! We could get Twitter Authorization at this time. " + error);
        },
        success: function (data, textStatus, jqXHR) {
            if (data.IsMessage)
                return false;

            $('#authorizationRequestToken').val(data.Token);
            window.open(data.Url);
        }
    });
}

function verifyTwitterAuthorization() {
    var token = $('#authorizationRequestToken').val();
    var verifier = $('#twitterVerification').val();

    $.ajax({
        type: "GET",
        url: verifyAuthorizationUrl,
        data: jQuery.param({ token: token, verifier: verifier }),
        error: function (req, status, error) {
            alert("Sorry! We could verify Twitter Authorization at this time. " + error);
        }
    });
}

//SMTP
function testSmtpSettings() {
    //Get the variables
    var server = $('#SmtpServer').val();
    var port = $('#SmtpPort').val();
    var ssl = $('#SmtpUseSsl').prop('checked');
    var username = $('#SmtpUsername').val();
    var password = $('#SmtpPassword').val();
    var fromAddress = $('#SmtpFromAddress').val();
    var toAddresses = $('#SmtpToAddresses').val();

    //Send the data!
    $.ajax({
        type: "POST",
        url: testEmailUrl,
        data: jQuery.param({
            server: server,
            port: port,
            ssl: ssl,
            username: username,
            password: password,
            fromAddress: fromAddress,
            toAddresses: toAddresses
        }),
        error: function (req, status, error) {
            alert("Sorry! We could send a test email at this time. " + error);
        }
    });

    return false;
}

//Growl
function registerGrowl() {
    //Get the variables
    var host = $('#GrowlHost').val();
    var password = $('#GrowlPassword').val();

    //Send the data!
    $.ajax({
        type: "POST",
        url: '../Command/RegisterGrowl',
        data: jQuery.param({
            host: host,
            password: password
        }),
        error: function (req, status, error) {
            alert("Sorry! We could send a test email at this time. " + error);
        }
    });

    return false;
}