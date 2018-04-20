<%@ Page Title="" Language="C#" MasterPageFile="~/General_Master.Master" AutoEventWireup="true" CodeBehind="AssessmentList.aspx.cs" Inherits="BSW.Web.AssessmentModule.AssessmentList" %>

<%@ Register Src="~/UserControls/BSWGrid.ascx" TagName="BSWGrid" TagPrefix="uc1" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .cat__ecommerce__catalog__item
        {
            padding: 0 !important;
        }
        .cat__ecommerce__catalog__item__img {
            height: 7.85rem;
        }

        .cat__ecommerce__catalog__item__like {
            right: 0;
            top: 0;
        }

        .cat__ecommerce__catalog__item__price {
            text-align: center;
            top: 2.71rem;
            left: 0;
        }

        .cat__ecommerce__catalog__item__title {
            padding-right: 0;
            margin-left: 0;
            margin-right: 0;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">
    <section class="card">
        <div class="card-header">
            <div class="pull-right">
                <asp:Button ID="btnAddNewAssessmentItem" runat="server" CssClass="btn btn-primary btn-sm" aria-expanded="false" Text="Add New Assessent" OnClick="btnAddNewAssessmentItem_Click" />                
            </div>
            <div class="pull-right" style="margin-right: 15px;">
                <asp:Button ID="btnOptDisplay" runat="server" CssClass="btn btn-danger btn-sm" aria-expanded="false" Text="" OnClick="btnOptDisplay_Click" />
            </div>
            <span class="cat__core__title">
                <strong>Assessment List</strong>
                <i class="fa fa-check-square-o"></i>
            </span>
        </div>
        <div class="card-block">
            <uc1:BSWGrid ID="grd" runat="server" UseFooterHeader="false" ShowExport2Excel="true" />
            <div id="divCardViewer" runat="server" class="cat__ecommerce__catalog" visible="false">
            </div>
        </div>
    </section>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scriptsContent" runat="server">
    <script>
        function selectRow(ctrl) {
            var $ctrl = $(ctrl);
            window.parent.selectData($ctrl.data("id"), $ctrl.data("text"), "<%= this.gPopupDataType %>");
        }
    </script>
</asp:Content>
