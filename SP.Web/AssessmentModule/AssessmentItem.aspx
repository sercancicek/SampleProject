<%@ Page Title="" Language="C#" MasterPageFile="~/General_Master.Master" AutoEventWireup="true" CodeBehind="AssessmentItem.aspx.cs" Inherits="BSW.Web.AssessmentModule.AssessmentItem" EnableViewState="true" %>

<%@ Register Src="~/UserControls/BSWDataSelection.ascx" TagName="BSWDataSelection" TagPrefix="uc1" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">
    <asp:UpdatePanel ID="pnlAI" runat="server">
        <ContentTemplate>
            <section class="card">
                <div class="card-header">
                    <span class="cat__core__title">
                        <strong>Assessment Item</strong>
                        <i class="fa fa-check-square-o"></i>
                        <sup class="text-muted">
                            <small>
                                <asp:Label ID="lblAssessmentName" runat="server"></asp:Label>
                            </small>
                        </sup>
                    </span>
                </div>
                <div class="card-block">
                    <section id="pnlQuestion" runat="server" class="card">
                        <div class="card-header">
                            <span class="cat__core__title">
                                <strong>Question Information</strong>
                            </span>
                        </div>
                        <div class="card-block">
                            <div class="col-lg-12">
                                <div class="row">
                                    <div class="col-lg-2">
                                        <div class="form-group">
                                            <label class="form-control-label">Order</label>
                                            <asp:TextBox ID="txtOrder" runat="server" TextMode="Number" CssClass="form-control" />
                                        </div>
                                    </div>
                                    <div class="col-lg-2">
                                        <div class="form-group">
                                            <label class="form-control-label">Version</label>
                                            <asp:TextBox ID="txtVersion" runat="server" TextMode="Number" CssClass="form-control" Enabled="false" />
                                        </div>
                                    </div>
                                    <div class="col-lg-4">
                                        <div class="form-group">
                                            <label class="form-control-label">Type</label>
                                            <asp:DropDownList ID="cmbType" runat="server" CssClass="form-control" AutoPostBack="true" EnableViewState="true" OnSelectedIndexChanged="cmbType_SelectedIndexChanged"></asp:DropDownList>
                                        </div>
                                    </div>
                                    <div class="col-lg-4 pull-right">
                                        <div class="form-check pull-right">
                                            <label class="form-check-label">
                                                <input id="chkIsRequired" runat="server" class="form-check-input" type="checkbox">
                                                Is Required
                                            </label>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-lg-12">
                                    <div class="form-group">
                                        <label class="form-control-label">Question</label>
                                        <asp:TextBox ID="txtText" runat="server" TextMode="MultiLine" Rows="2" CssClass="form-control" />
                                    </div>
                                </div>
                                <div class="col-lg-12">
                                    <div class="form-group">
                                        <label class="form-control-label">Tooltip</label>
                                        <asp:TextBox ID="txtHelpText" runat="server" TextMode="MultiLine" Rows="2" CssClass="form-control" />
                                    </div>
                                </div>

                                <div class="card-header">
                            <span class="cat__core__title">
                                <h5>Display Condition</h5>
                            </span>
                        </div>

                                <div class="row">
                                    <div class="col-lg-3">
                                        <div class="form-group">
                                            <label class="form-control-label">Assessment Items</label>
                                            <asp:DropDownList ID="cmbAssessmentItems" runat="server" CssClass="form-control" AutoPostBack="true" EnableViewState="true" OnSelectedIndexChanged="cmbAssessmentItems_SelectedIndexChanged"></asp:DropDownList>
                                        </div>
                                    </div>
                                    <div class="col-lg-2">
                                        <div class="form-group">
                                            <label class="form-control-label">Operator</label>
                                            <asp:DropDownList ID="cmbOperator" runat="server" CssClass="form-control" AutoPostBack="true" EnableViewState="true" OnSelectedIndexChanged="cmbOperator_SelectedIndexChanged"></asp:DropDownList>
                                        </div>
                                    </div>
                                    <div class="col-lg-3">
                                        <div id="divCombo" runat="server" class="form-group">
                                            <label class="form-control-label">Value</label>
                                            <asp:DropDownList ID="cmbControl" runat="server" CssClass="form-control" AutoPostBack="true" EnableViewState="true" OnSelectedIndexChanged="cmbControl_SelectedIndexChanged"></asp:DropDownList>
                                        </div>
                                        <div id="divText" runat="server" visible="false" class="form-group">
                                            <label class="form-control-label">Value</label>
                                            <asp:TextBox ID="txtControl" runat="server" CssClass="form-control" />
                                        </div>
                                    </div>
                                    <asp:Panel ID="divButtons" runat="server" class="col-lg-4 form-group" Style="padding-top: 32px;">

                                        <asp:Button ID="btnAddOpenParanthesis" runat="server" CssClass="btn btn-sm btn-primary col-lg-2 form-control" Text="(" Style="cursor: pointer" OnClientClick="Add2Box('(');" />
                                        <asp:Button ID="btnAddTheItem" runat="server" CssClass="btn btn-sm btn-success form-control col-lg-3" Text="Add Item" Style="cursor: pointer" OnClick="btnAddTheItem_Click" />
                                        <asp:Button ID="btnAddAndOp" runat="server" CssClass="btn btn-sm btn-warning form-control col-lg-2" Text="&&" Style="cursor: pointer" OnClientClick="Add2Box('&&');" />
                                        <asp:Button ID="btnAddOrOp" runat="server" CssClass="btn btn-sm btn-warning form-control col-lg-2" Text="||" Style="cursor: pointer" OnClientClick="Add2Box('||');" />
                                        <asp:Button ID="btnAddCloseParanthesis" runat="server" CssClass="btn btn-sm btn-primary form-control col-lg-2" Text=")" Style="cursor: pointer" OnClientClick="Add2Box(')');" />
                                    </asp:Panel>

                                </div>
                                
                                <div class="col-lg-12">
                                    <div class="form-group">
                                        <label class="form-control-label">Formula</label>
                                        <asp:TextBox ID="txtFormula" runat="server" TextMode="MultiLine" Rows="2" CssClass="form-control" />
                                    </div>
                                </div>
                            </div>
                            <div class="form-actions" style="width: 100%;">
                                <div class="form-group col-lg-12" style="text-align: right;">
                                    <a id="btnBack" runat="server" class="btn btn-sm btn-danger">
                                        <i class="fa fa-times"></i>Cancel
                                    </a>
                                    <asp:Button ID="btnSaveAssessmentItem" runat="server" CssClass="btn btn-sm btn-primary" OnClick="btnSaveAssessmentItem_Click" Text="Save" Style="cursor: pointer" />
                                </div>
                            </div>
                        </div>
                    </section>
                    <div id="divPageBreak" style="display: none;"></div>
                    <section id="pnlChoice" runat="server" class="card">
                        <div id="choice_header" class="card-header">
                            <span class="cat__core__title">
                                <strong>Choice Information</strong>
                            </span>
                        </div>
                        <div id="divGridSubItem" runat="server" visible="false" class="card-block row">
                            <div class="col-lg-6 mb-5">
                                <div class="col-lg-2">
                                    <div class="form-group">
                                        <label class="form-control-label">Order</label>
                                        <asp:TextBox ID="txtSubOrder" runat="server" TextMode="Number" CssClass="form-control" />
                                    </div>
                                </div>
                                <div class="col-lg-12">
                                    <div class="form-group">
                                        <label class="form-control-label">Question</label>
                                        <asp:TextBox ID="txtSubText" runat="server" TextMode="MultiLine" Rows="2" CssClass="form-control" />
                                    </div>
                                </div>
                                <div class="col-lg-12">
                                    <div class="form-group">
                                        <label class="form-control-label">Tooltip</label>
                                        <asp:TextBox ID="txtSubHelpText" runat="server" TextMode="MultiLine" Rows="2" CssClass="form-control" />
                                    </div>
                                </div>
                                <div class="form-actions" style="width: 100%;">
                                    <div class="form-group col-lg-12" style="text-align: right;">
                                        <asp:Button ID="btnSaveSubAssessmentItem" runat="server" CssClass="btn btn-sm btn-primary" OnClick="btnSaveSubAssessmentItem_Click" Text="Save" Style="cursor: pointer" />
                                    </div>
                                </div>
                            </div>
                            <div id="divGridSubItem_Grid" runat="server" visible="false" class="col-lg-6 mb-5">
                                <table class="table">
                                    <thead>
                                        <tr>
                                            <th>#</th>
                                            <th>Text</th>
                                            <th>Help Text</th>
                                            <th></th>
                                        </tr>
                                    </thead>
                                    <tbody id="grdSubAI" runat="server">
                                    </tbody>
                                </table>
                            </div>
                        </div>
                        <div id="divChoice" runat="server" class="card-block row">
                            <div id="divNewChoice" runat="server" class="col-lg-6" style="border-right: 1px solid #808080">
                                <div class="col-lg-12 row">
                                    <div class="col-lg-4">
                                        <div class="form-group">
                                            <label class="form-control-label">Order</label>
                                            <asp:TextBox ID="txtChoiceOrder" runat="server" TextMode="Number" CssClass="form-control" />
                                        </div>
                                    </div>
                                    <div class="col-lg-4 pull-right">
                                        <div class="form-check pull-right">
                                            <asp:CheckBox ID="chk_ChoiceIsDefault" runat="server" CssClass="form-check-input" Text="Is Default?" Width="100" />
                                        </div>
                                    </div>
                                </div>
                                <div class="col-lg-12 row">
                                    <div class="col-lg-6">
                                        <div class="form-group">
                                            <label class="form-control-label">Value</label>
                                            <asp:TextBox ID="txtChoicevalue" runat="server" CssClass="form-control" />
                                        </div>
                                    </div>
                                    <div class="col-lg-4">
                                        <div class="form-group">
                                            <label class="form-control-label">Score</label>
                                            <asp:TextBox ID="txtScoreValue" runat="server" TextMode="Number" CssClass="form-control" />
                                        </div>
                                    </div>
                                </div>
                                <div class="form-actions" style="width: 100%;">
                                    <div class="form-group col-lg-12" style="text-align: right;">
                                        <asp:Button ID="btnSaveChoice" runat="server" CssClass="btn btn-sm btn-primary" OnClick="btnSaveChoice_Click" Text="Save" Style="cursor: pointer" />
                                        <asp:Button ID="btnAddGridChoice" runat="server" Visible="false" CssClass="btn btn-sm btn-primary" OnClick="btnAddGridChoice_Click" Text="Add" Style="cursor: pointer" />
                                    </div>
                                </div>
                            </div>
                            <div id="grdChoices" runat="server" visible="false" class="col-lg-6 mb-5">
                                <table class="table">
                                    <thead>
                                        <tr>
                                            <th>#</th>
                                            <th>Value</th>
                                            <th>Score</th>
                                            <th>IsDefault?</th>
                                            <th></th>
                                        </tr>
                                    </thead>
                                    <tbody id="grdChoiceBody" runat="server">
                                    </tbody>
                                </table>
                            </div>

                            <div id="divSaveAll" runat="server" class="form-actions" style="width: 100%;">
                                <div class="form-group col-lg-12" style="text-align: right;">
                                    <asp:Button ID="btnSaveAll" runat="server" CssClass="btn btn-sm btn-primary" OnClick="btnSaveAll_Click" Text="Save" Style="cursor: pointer" />
                                </div>
                            </div>
                        </div>

                    </section>
                </div>
            </section>
            <asp:Button ID="btnDeleteChoice" runat="server" Width="0" Height="0" Style="display: none;" OnClick="btnDeleteChoice_Click" ClientIDMode="Static" />
            <input id="hidChoice2DeleteID" runat="server" type="hidden" />
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scriptsContent" runat="server">
    <script>
        function DeleteItem(pID) {
            $("[id*='hidChoice2DeleteID']").val(pID);
            $("#btnDeleteChoice").click();
        }
    </script>
</asp:Content>
