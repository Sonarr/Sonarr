<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<NzbDrone.Core.Repository.Quality.QualityProfile>" %>
<%@ Import Namespace="NzbDrone.Web.Helpers" %>
<%@ Import Namespace="NzbDrone.Core.Repository.Quality" %>

<% using (Html.BeginCollectionItem("UserProfiles"))
   { %>
        <style type="text/css">
	        #sortable1, #sortable2 { list-style-type: none; margin: 0; padding: 0; float: left; margin-right: 10px; background: #eee; padding: 5px; width: 143px;}
	        .allowedQualities, .otherQualities { list-style-type: none; margin: 0; padding: 0; float: left; margin-right: 10px; background: #eee; padding: 5px; width: 148px;}
	        #sortable1 li, #sortable2 li { margin: 5px; padding: 5px; font-size: 1.2em; width: 120px; }
	    </style>

        <script type="text/javascript">
            $(function () {
                $("#sortable1, #sortable2").sortable({
                    connectWith: ".connectedSortable",
                    placeholder: "ui-state-highlight",
                    dropOnEmpty: true,

                    create: function (event, ui) {
                        var order = $('#sortable1').sortable("toArray");
                        $("#allowedString").val(order);
                    },

                    update: function (event, ui) {
                        var order = $('#sortable1').sortable("toArray");
                        $("#allowedString").val(order);
                    }

                }).disableSelection();
            });

	    </script>

    <div class="userProfileSectionEditor"> 

        <fieldset>
            <%--<label><%= Model.Name %></label>--%>

            <%= Html.TextBoxFor(m => m.AllowedString, new { @id = "allowedString", @style = "display:none" })%>
            <%--<%= Html.TextBoxFor(m => m.AllowedString, new { @id = "allowedString" })%>--%>
            <%--<%= Html.TextBox("Name", "Empty", new { @id = "allowedString2" })%>--%>

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
                <h4 style="padding:0">Allowed</h4>
                <ul id="sortable1" class="connectedSortable" title>
                    <% if (Model.Allowed != null) { %>
                        <%for (int i = 0; i < Model.Allowed.Count(); i++){%>
                            <li class="ui-state-default" id="<%= Model.Allowed[i].ToString() %>">
                            <%=Html.RadioButtonFor(x => x.Cutoff, Model.Allowed[i])%>
                            <%= Html.DisplayTextFor(c => c.Allowed[i]) %>
                            </li>
                        <%}%>
                    <%}%>
                </ul>
            </div>

            <div class="otherQualities">
                <h4 padding-left="10px">Not-Allowed</h4>
                <ul id="sortable2" class="connectedSortable">
                    <% var qualitiesList = (List<QualityTypes>) ViewData["Qualities"]; %>

                    <%for (int i = 0; i < qualitiesList.Count(); i++){%>                      
                        <%
                            //Skip Unknown and any item that is in the allowed list
                            if (qualitiesList[i].ToString() == "Unknown")
                                continue;
                          
                            if (Model.Allowed != null)
                            {
                                if (Model.Allowed.Contains(qualitiesList[i]))
                                    continue;
                            }
                        %>

                        <li class="ui-state-default" id="<%= qualitiesList[i].ToString() %>">
                        <%=Html.RadioButtonFor(x => x.Cutoff, qualitiesList[i])%>
                        <%= Html.Label(qualitiesList[i].ToString()) %>
                        <%--<%= Html.RenderPartial("ProfileAllowedQualities", Model.Allowed[i]) %>--%>
                        </li>

                    <% } %>
                </ul>
            </div>
            <div class="removeDiv"><a href="#" class="deleteRow">Remove</a></div>         
        </fieldset>
    </div> 
<% } %>