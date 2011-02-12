<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<NzbDrone.Core.Repository.Quality.QualityProfile>" %>
<%@ Import Namespace="NzbDrone.Web.Helpers" %>
<%@ Import Namespace="NzbDrone.Core.Repository.Quality" %>

<% using (Html.BeginCollectionItem("UserProfiles"))
   { %>   

       <%
           var idClean = ViewData.TemplateInfo.HtmlFieldPrefix.Replace('[', '_').Replace(']', '_');

           string sortable1 = String.Format("{0}_sortable1", idClean);
           string sortable2 = String.Format("{0}_sortable2", idClean);
           string allowedStringName = String.Format("{0}_AllowedString", idClean);
           string connectedSortable = String.Format("connected{0}", idClean);
       %>

        <style type="text/css">
            .sortable1, .sortable2 { list-style-type: none; margin-right: 10px; background: #eee; padding-left: 5px; padding-right: 5px; padding-top: 0px; padding-bottom: 6px; width: 107px; margin-bottom: 3px; }
            .allowedQualities, .otherQualities { list-style-type: none; float: left; margin-right: 10px; background: #eee; padding-left: 5px; padding-right: 5px; padding-top:6px; width: 112px; -khtml-border-radius:8px;border-radius:8px;-moz-border-radius:8px;-webkit-border-radius:8px; }
            .sortable1 li, .sortable2 li { margin: 5px; margin-left: 0px; padding: 0px; font-size: 1.2em; width: 100px; -khtml-border-radius:8px;border-radius:8px;-moz-border-radius:8px;-webkit-border-radius:8px; }
            .sortable1 li { background: #ddd; }
            .sortable2 li { background: #DAA2A2; }
            .sortableHeader { margin:2px; margin-left:12px }
            .sortable1 li.ui-state-highlight, .sortable2 li.ui-state-highlight { background: #fbf5d0; border-color: #065EFE; }
            .removeDiv { float: left; display:block; }
            .ui-state-highlight { height: 1.5em; line-height: 1.2em; }

        </style>

        <script type="text/javascript">
            $(function () {
                $("#<%= sortable1 %>, #<%= sortable2 %>").sortable({
                    connectWith: ".<%= connectedSortable %>",
                    placeholder: "ui-state-highlight",
                    dropOnEmpty: true,

                    create: function (event, ui) {
                        var order = $('#<%= sortable1 %>').sortable("toArray");
                        $("#<%= allowedStringName %>").val(order);
                    },

                    update: function (event, ui) {
                        var order = $('#<%= sortable1 %>').sortable("toArray");
                        $("#<%= allowedStringName %>").val(order);
                    }

                }).disableSelection();
            });
        </script>

    <div class="userProfileSectionEditor"> 

        <fieldset style="width:275px; margin:5px; margin-top: 0px; border-color:#CCCCCD">

            <div id="qualityHeader" style="padding-bottom: 5px; margin: 0px;">
                <h2 style="display:inline; padding-right: 4px; margin-left: 4px;"><%= Html.DisplayTextFor(m => m.Name) %></h2>
                <a href="#" class="deleteRow"><img src="../../Content/Images/X.png" alt="Delete" /></a>
            </div>

            <div class="config-group" style="width: 250px; margin-bottom: 5px; margin-left: 5px;">
                <div class="config-title"><%= Html.LabelFor(x => x.Name)%></div>
                <div class="config-value"><%= Html.TextBoxFor(x => x.Name)%></div>
                <div class="config-validation"><%= Html.ValidationMessageFor(x => x.Name)%></div>
            </div>

            <div id="sortablesDiv" style="margin: 0px;">
                <div class="allowedQualities">
                    <h4 class="sortableHeader">Allowed</h4>
                    <ul id="<%= sortable1 %>" class="<%= connectedSortable %> sortable1">
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
                    <h4 class="sortableHeader">Not-Allowed</h4>
                    <ul id="<%= sortable2 %>" class="<%= connectedSortable %> sortable2">
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
                            </li>

                        <% } %>
                    </ul>
                </div>
            </div>

            <%= Html.ValidationMessageFor(x => x.Cutoff) %>

            <div class="hiddenProfileDetails">
                <%= Html.TextBoxFor(x => x.ProfileId, new { @style = "display:none" })%>
                <%= Html.CheckBoxFor(x => x.UserProfile, new { @style = "display:none" })%>
                <%= Html.TextBoxFor(m => m.AllowedString, new { @style = "display:none" })%>
            </div>
        </fieldset>
    </div> 
<% } %>