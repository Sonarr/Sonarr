<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<NzbDrone.Core.Repository.Series>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Edit
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Edit</h2>

    <% using (Html.BeginForm()) {%>
        <%: Html.ValidationSummary(true) %>
        
        <fieldset>
            <legend>Fields</legend>
            
            <div class="editor-label">
                <%: Html.LabelFor(model => model.SeriesId) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(model => model.SeriesId) %>
                <%: Html.ValidationMessageFor(model => model.SeriesId) %>
            </div>
            
            <div class="editor-label">
                <%: Html.LabelFor(model => model.Title) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(model => model.Title) %>
                <%: Html.ValidationMessageFor(model => model.Title) %>
            </div>
            
            <div class="editor-label">
                <%: Html.LabelFor(model => model.CleanTitle) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(model => model.CleanTitle) %>
                <%: Html.ValidationMessageFor(model => model.CleanTitle) %>
            </div>
            
            <div class="editor-label">
                <%: Html.LabelFor(model => model.Status) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(model => model.Status) %>
                <%: Html.ValidationMessageFor(model => model.Status) %>
            </div>
            
            <div class="editor-label">
                <%: Html.LabelFor(model => model.Overview) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(model => model.Overview) %>
                <%: Html.ValidationMessageFor(model => model.Overview) %>
            </div>
            
            <div class="editor-label">
                <%: Html.LabelFor(model => model.AirTimes) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(model => model.AirTimes) %>
                <%: Html.ValidationMessageFor(model => model.AirTimes) %>
            </div>
            
            <div class="editor-label">
                <%: Html.LabelFor(model => model.Language) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(model => model.Language) %>
                <%: Html.ValidationMessageFor(model => model.Language) %>
            </div>
            
            <div class="editor-label">
                <%: Html.LabelFor(model => model.Path) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(model => model.Path) %>
                <%: Html.ValidationMessageFor(model => model.Path) %>
            </div>
            
            <div class="editor-label">
                <%: Html.LabelFor(model => model.Monitored) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(model => model.Monitored) %>
                <%: Html.ValidationMessageFor(model => model.Monitored) %>
            </div>
            
            <%--<div class="editor-label">
                <%: Html.LabelFor(model => model.ProfileId) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(model => model.ProfileId) %>
                <%: Html.ValidationMessageFor(model => model.ProfileId) %>
            </div>--%>
            
            <p>
                <input type="submit" value="Save" />
            </p>
        </fieldset>

    <% } %>

    <div>
        <%: Html.ActionLink("Back to List", "Index") %>
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="headerContent" runat="server">
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ActionMenu" runat="server">
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="Scripts" runat="server">
</asp:Content>

