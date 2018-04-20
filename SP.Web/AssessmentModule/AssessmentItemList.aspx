<%@ Page Title="" Language="C#" MasterPageFile="~/General_Master.Master" AutoEventWireup="true" CodeBehind="AssessmentItemList.aspx.cs" Inherits="BSW.Web.AssessmentModule.AssessmentItemList" %>

<%@ Register Src="~/UserControls/BSWGrid.ascx" TagName="BSWGrid" TagPrefix="uc1" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">
    <section class="card">
        <div class="card-header">
            <div class="pull-right">
                <a href='AssessmentList.aspx' class="btn btn-warning btn-sm">
                    <i class='fa fa-arrow-left' aria-hidden='true'></i>
                    Back To List
                </a>
                <asp:Button ID="btnAddNewAssessmentItem" runat="server" CssClass="btn btn-primary btn-sm" aria-expanded="false" Text="Add New Assessent Item" OnClick="btnAddNewAssessmentItem_Click" />
            </div>
            <span class="cat__core__title">
                <strong>Assessment Item List</strong>
                <i class="fa fa-check-square-o"></i>
                <sup class="text-muted">
                    <small>
                        <asp:Label ID="lblAssessmentName" runat="server"></asp:Label>
                    </small>
                </sup></span>
        </div>
        <div class="card-block">
            <uc1:BSWGrid ID="grd" runat="server" UseFooterHeader="false" ShowExport2Excel="true" />
        </div>
    </section>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scriptsContent" runat="server">
</asp:Content>
