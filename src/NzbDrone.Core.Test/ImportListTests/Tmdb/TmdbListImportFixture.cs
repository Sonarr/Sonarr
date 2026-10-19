using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.Tmdb.List;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ImportListTests.Tmdb;

[TestFixture]
public class TmdbListImportFixture : CoreTest<TmdbListImport>
{
    [SetUp]
    public void Setup()
    {
        Mocker.GetMock<ISonarrCloudRequestBuilder>()
            .SetupGet(s => s.Services)
            .Returns(new HttpRequestBuilder("https://services.sonarr.tv/v1/").CreateFactory());

        Subject.Definition = new ImportListDefinition
        {
            Settings = new TmdbListSettings { AuthToken = "token", AccountId = "acc", ListId = 1 }
        };
    }

    [Test]
    public void should_return_approval_url_and_request_token_for_start_oauth()
    {
        GivenResponse("""{ "success": true, "statusCode": 1, "requestToken": "abc" }""");

        var result = Subject.RequestAction("startOAuth", Query("callbackUrl", "http://localhost/cb"));

        var type = result.GetType();
        type.GetProperty("RequestToken")!.GetValue(result).Should().Be("abc");
        ((string)type.GetProperty("OauthUrl")!.GetValue(result)!).Should().Contain("request_token=abc");
    }

    [Test]
    public void should_throw_when_not_successful_for_start_oauth()
    {
        GivenResponse("""{ "success": false, "statusCode": 7, "statusMessage": "Invalid API key" }""");

        var action = () => Subject.RequestAction("startOAuth", Query("callbackUrl", "http://localhost/cb"));

        action.Should().Throw<NzbDroneClientException>().Which.Message.Should().Contain("Invalid API key");
    }

    [Test]
    public void should_return_access_token_for_get_oauth_token()
    {
        GivenResponse("""{ "success": true, "accountId": "acc", "accessToken": "tok" }""");

        var result = Subject.RequestAction("getOAuthToken", Query("requestToken", "abc"));

        result.GetType().GetProperty("accountId")!.GetValue(result).Should().Be("acc");
        result.GetType().GetProperty("authToken")!.GetValue(result).Should().Be("tok");
    }

    [Test]
    public void should_throw_when_not_successful_for_get_oauth_token()
    {
        GivenResponse("""{ "success": false, "statusMessage": "Request token not approved" }""");

        var action = () => Subject.RequestAction("getOAuthToken", Query("requestToken", "abc"));

        action.Should().Throw<NzbDroneClientException>().Which.Message.Should().Contain("not approved");
    }

    [Test]
    public void should_return_empty_object_for_unknown_action()
    {
        Subject.RequestAction("nope", new Dictionary<string, string>()).Should().NotBeNull();
    }

    [Test]
    public void should_return_none_plus_lists_with_integer_values_for_get_account_lists()
    {
        GivenResponse("""{ "results": [ { "id": 11, "name": "A", "number_of_items": 3 }, { "id": 12, "name": "B", "number_of_items": 4 } ] }""");

        var result = Subject.RequestAction("getAccountLists", new Dictionary<string, string>());

        var options = result.GetType().GetProperty("options")!.GetValue(result)
            .Should().BeAssignableTo<List<FieldSelectOption<int>>>().Subject;

        options.Select(o => o.Value).Should().Equal(0, 11, 12);
        options.Select(o => o.Name).Should().Equal("None", "A", "B");
    }

    [Test]
    public void should_return_only_none_without_authentication_for_get_account_lists()
    {
        Subject.Definition = new ImportListDefinition { Settings = new TmdbListSettings() };

        var result = Subject.RequestAction("getAccountLists", new Dictionary<string, string>());

        var options = (List<FieldSelectOption<int>>)result.GetType().GetProperty("options")!.GetValue(result)!;
        options.Should().ContainSingle().Which.Name.Should().Be("None");
    }

    [Test]
    public void should_throw_bad_request_when_callback_url_is_missing_for_start_oauth()
    {
        var action = () => Subject.RequestAction("startOAuth", new Dictionary<string, string>());

        action.Should().Throw<NzbDroneClientException>().Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public void should_throw_bad_request_when_request_token_is_missing_for_get_oauth_token()
    {
        var action = () => Subject.RequestAction("getOAuthToken", new Dictionary<string, string>());

        action.Should().Throw<NzbDroneClientException>().Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public void should_return_only_none_when_request_fails_for_get_account_lists()
    {
        Mocker.GetMock<IHttpClient>()
            .Setup(c => c.Execute(It.IsAny<HttpRequest>()))
            .Throws(new HttpException(new HttpResponse(new HttpRequest("http://localhost"), new HttpHeader(), string.Empty, HttpStatusCode.Unauthorized)));

        var result = Subject.RequestAction("getAccountLists", new Dictionary<string, string>());

        var options = (List<FieldSelectOption<int>>)result.GetType().GetProperty("options")!.GetValue(result)!;
        options.Should().ContainSingle().Which.Value.Should().Be(0);

        ExceptionVerification.ExpectedWarns(1);
    }

    [Test]
    public void should_return_only_none_when_results_missing_for_get_account_lists()
    {
        GivenResponse("{}");

        var result = Subject.RequestAction("getAccountLists", new Dictionary<string, string>());

        var options = (List<FieldSelectOption<int>>)result.GetType().GetProperty("options")!.GetValue(result)!;
        options.Should().ContainSingle().Which.Value.Should().Be(0);
    }

    [Test]
    public void should_fetch_next_page_only_when_page_is_full_for_get_account_lists()
    {
        var fullPage = "{ \"results\": [" + string.Join(",", Enumerable.Range(1, 20).Select(i => $"{{ \"id\": {i}, \"name\": \"L{i}\", \"number_of_items\": 1 }}")) + "] }";
        const string shortPage = """{ "results": [ { "id": 99, "name": "Last", "number_of_items": 1 } ] }""";
        var responses = new Queue<string>([fullPage, shortPage, fullPage]);

        Mocker.GetMock<IHttpClient>()
            .Setup(c => c.Execute(It.IsAny<HttpRequest>()))
            .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), responses.Dequeue()));

        var result = Subject.RequestAction("getAccountLists", new Dictionary<string, string>());

        var options = (List<FieldSelectOption<int>>)result.GetType().GetProperty("options")!.GetValue(result)!;
        options.Should().HaveCount(1 + 20 + 1);
        options.Last().Value.Should().Be(99);
        Mocker.GetMock<IHttpClient>().Verify(c => c.Execute(It.IsAny<HttpRequest>()), Times.Exactly(2));
    }

    private static Dictionary<string, string> Query(string key, string value) => new() { { key, value } };

    private void GivenResponse(string content, HttpStatusCode status = HttpStatusCode.OK)
    {
        Mocker.GetMock<IHttpClient>()
            .Setup(c => c.Execute(It.IsAny<HttpRequest>()))
            .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), content, status));
    }
}
