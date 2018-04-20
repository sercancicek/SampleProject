<%@ Page Title="" Language="C#" MasterPageFile="~/General_Master.Master" AutoEventWireup="true" CodeBehind="Assessment.aspx.cs" Inherits="BSW.Web.AssessmentModule.Assessment" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card-header">
                <div class="pull-right">
                    <a href='AssessmentList.aspx' class="btn btn-warning btn-sm">
                        <i class='fa fa-arrow-left' aria-hidden='true'></i>
                        Back To List
                    </a>
                </div>
                <span class="cat__core__title">
                    <strong>Assessment</strong>
                    <i class="fa fa-check-square-o"></i>
                </span>
            </div>
            <section id="divControls" class="card">
                <div class="card-block">
                    <div class="row">
                        <div class="col-lg-12">
                            <div class="form-group">
                                <label class="form-control-label">Name</label>
                                <asp:TextBox ID="txtName" runat="server" CssClass="form-control" />
                            </div>
                        </div>
                        <div class="col-lg-12">
                            <div class="form-group">
                                <label class="form-control-label">Introduction</label>
                                <asp:TextBox ID="txtIntroduction" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-control" />
                            </div>
                        </div>
                        <div class="col-lg-12 row">
                            <div class="col-lg-2">
                                <div class="form-group">
                                    <label class="form-control-label">Major</label>
                                    <asp:TextBox ID="txtMajorVersion" runat="server" TextMode="Number" CssClass="form-control" />
                                </div>

                            </div>
                            <div class="col-lg-2">
                                <div class="form-group">
                                    <label class="form-control-label">Minor</label>
                                    <asp:TextBox ID="txtMinorVersion" runat="server" TextMode="Number" CssClass="form-control" />
                                </div>
                            </div>
                            <div class="col-lg-2">
                                <div class="form-group">
                                    <label class="form-control-label">Build No</label>
                                    <asp:TextBox ID="txtBuildNumber" runat="server" TextMode="Number" CssClass="form-control" />
                                </div>
                            </div>
                        </div>
                        <div class="col-sm-12">
                            <div class="form-group">
                                <label class="form-control-label">Category</label>
                                <select id="cmbType" runat="server" class="form-control selectpicker">
                                </select>
                            </div>
                        </div>
                        <div class="col-sm-12">
                            <div class="form-group">
                                <label class="form-control-label">Program</label>
                                <select id="cmbProgram" runat="server" class="form-control selectpicker">
                                </select>
                            </div>
                        </div>
                        <div class="col-lg-12">
                            <div class="form-group">
                                <label class="form-control-label">Author</label>
                                <asp:TextBox ID="txtAuthor" runat="server" CssClass="form-control" />
                            </div>
                        </div>
                        <div class="form-actions" style="width: 100%;">
                            <div class="form-group col-lg-12" style="text-align: right;">
                                <asp:Button ID="btnSaveAssessment" runat="server" CssClass="btn width-200 btn-primary" OnClick="btnSaveAssessment_Click" Text="Save" Style="cursor: pointer" />
                            </div>
                        </div>
                    </div>
                </div>

            </section>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scriptsContent" runat="server">
</asp:Content>
