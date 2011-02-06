<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<NzbDrone.Core.Repository.Quality.QualityProfile>" %>
<%@ Import Namespace="NzbDrone.Web.Helpers" %>
<%@ Import Namespace="NzbDrone.Core.Repository.Quality" %>

<% using (Html.BeginCollectionItem("UserProfiles"))
   { %>

        <style type="text/css">
	        #sortable1, #sortable2 { list-style-type: none; margin: 0; padding: 0; float: left; margin-right: 10px; background: #eee; padding: 5px; width: 143px;}
	        #sortable1 li, #sortable2 li { margin: 5px; padding: 5px; font-size: 1.2em; width: 120px; }
	    </style>

        <script type="text/javascript">
            $(function () {
                $("#sortable1, #sortable2").sortable({
                    connectWith: ".connectedSortable",
                    placeholder: "ui-state-highlight",
                    dropOnEmpty: true,
                    update: function (event, ui) {

                        var order = $('#sortable1').sortable("toArray");
                        $("#allowedString").val(order);
                    }
                }).disableSelection();
            });

	    </script>

    <div class="userProfileSectionEditor"> 

        <fieldset>

        <%= Html.TextBoxFor(m => m.AllowedString, new { @id = "allowedString", @style = "display:none" })%>
        <%--<%= Html.TextBoxFor(m => m.AllowedString, new { @id = "allowedString" })%>--%>

        <label><%= Model.Name %></label>
            <div class="removeDiv"><a href="#" class="deleteRow">Remove</a></div>

            <div class="editor-label">
                <%= Html.LabelFor(x => x.Name)%>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(x => x.Name) %> 
                <%= Html.ValidationMessageFor(x => x.Name)%>
            </div>

            <div class="hiddenProfileDetails">
                <%= Html.TextBoxFor(x => x.ProfileId, new { @style = "display:none" })%>
                <%= Html.CheckBoxFor(x => x.UserProfile, new { @style = "display:none" })%>
            </div>

            <br />

            <div class="allowedQualities">
                <ul id="sortable1" class="connectedSortable" title="Allowed">
                    <%for (int i = 0; i < Model.Allowed.Count(); i++){%>
                        <li class="ui-state-default" id="<%= Model.Allowed[i].ToString() %>">
                        <%= Html.DisplayTextFor(c => c.Allowed[i]) %>
                        </li>
                    <%}%>
                </ul>
            </div>

            <div class="otherQualities">
                <ul id="sortable2" class="connectedSortable">
                    <% var qualitiesList = (List<QualityTypes>) ViewData["Qualities"]; %>

                    <%for (int i = 0; i < qualitiesList.Count(); i++){%>                      
                        <%
                            //Skip Unknown and any item that is in the allowed list
                            if (qualitiesList[i].ToString() == "Unknown")
                                continue;
                            if (Model.Allowed.Contains(qualitiesList[i]))
                                continue;
                        %>

                        <li class="ui-state-default" id="<%= qualitiesList[i].ToString() %>">
                        <%= Html.Label(qualitiesList[i].ToString()) %>
                        <%--<%= Html.RenderPartial("ProfileAllowedQualities", Model.Allowed[i]) %>--%>
                        </li>

                        <% } %>
                </ul>
            </div>            
        </fieldset>
    </div> 
<% } %>