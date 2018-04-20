<%@ Page Title="" Language="C#" MasterPageFile="~/General_Master.Master" AutoEventWireup="true" CodeBehind="AssessmentViewer.aspx.cs" Inherits="BSW.Web.AssessmentModule.AssessmentViewer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .cat__wizard .actions li a {
            background-color: #2184be;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">
    <asp:UpdatePanel ID="pnl" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <!-- START: documentation/index -->
            <section class="card">
                <div class="card-header">
                    <div class="pull-right">
                        <asp:Label ID="lblAssessmentType" runat="server"></asp:Label>
                    </div>
                    <span class="cat__core__title">
                        <strong>
                            <asp:Label ID="lblAssessmentName" runat="server"></asp:Label></strong>
                        <sup class="text-muted">
                            <small>
                                <asp:Label ID="lblAssessmentVersion" runat="server"></asp:Label>
                            </small>
                        </sup>
                    </span>
                    <div class="col-sm-12">
                        <asp:Label ID="lblIntroduction" runat="server" Text=""></asp:Label>
                    </div>
                </div>
                <div class="card-block">
                    <div class="nav-tabs-horizontal">
                        <div id="divViewer" runat="server" class="row">
                            <div id="divWizard" runat="server" class="col-lg-12" visible="false">
                                <div id="divWizardContent" runat="server" class="cat__wizard cat__wizard__numbers">
                                </div>
                            </div>
                            <div id="divTOC" runat="server" class="col-lg-3" visible="false">
                                <div class="table-of-contents">
                                    <h6 class="mb-3 text-black" style="height: 2.125rem; padding-top: 0.15rem;">
                                        <strong>Table of Contents</strong>
                                    </h6>
                                    <hr />
                                    <div class="px-3 mb-5">
                                        <ol id="divTOC_Contents" runat="server" class="list-unstyled scroll-to-chapter">
                                            <li class="mb-2"><a href="#chapter8" class="cat__core__link--blue cat__core__link--underlined">8. Modules</a></li>
                                            <li class="mb-2">
                                                <ol class="list-unstyled ml-3">
                                                    <li class="mb-2"><a href="#chapter8-1" class="cat__core__link--blue cat__core__link--underlined">8.1. Core Module</a></li>
                                                </ol>
                                            </li>
                                        </ol>
                                    </div>
                                </div>
                            </div>
                            <div id="divAssessmentItems" runat="server" class="col-lg-12">
                            </div>
                            <div id="divBtnSinglePage" runat="server" class="col-lg-12">
                                <a href='javascript:StepsFinished();' class="btn btn-success mr-2 mb-2 pull-right">
                                    <i class='fa fa-save' aria-hidden='true'></i>
                                    Save
                                </a>
                                <a href='AssessmentList.aspx' class="btn btn-warning mr-2 mb-2 pull-right">
                                    <i class='fa fa-arrow-left' aria-hidden='true'></i>
                                    Back To List
                                </a>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
            <asp:Button ID="btnCloseAssessment" runat="server" Style="display: none;" OnClick="btnCloseAssessment_Click" />
            <div class="row">
                <div class="col-xl-12">
                    <div class="form-group col-lg-12" style="text-align: right;">
                        <asp:Button ID="btnDeleteAssessmentResponse" runat="server" CssClass="btn btn-danger btn-xs " OnClick="btnDeleteAssessmentResponse_Click" Text="Delete Assessment Response (For Test purposes)" Style="cursor: pointer" Visible="false" />
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanel runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:HiddenField ID="hdnRespID" runat="server" ClientIDMode="Static" />
            <asp:Button ID="btnUpdateRespID" runat="server" style="display:none;" OnClick="btnUpdateRespID_Click" ClientIDMode="Static" />
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scriptsContent" runat="server">
    <style>
        .Item-Centered {
            text-align: center;
        }
    </style>
    <script>
        var _img;
        function SaveValue(ctrl) {
            var _key = "<%= gAssessmentID %>";
            var _key7 = "<%= gAssessmentRespID %>";

            $.blockUI({
                css: {
                    border: 'none',
                    padding: '15px',
                    backgroundColor: '#000',
                    '-webkit-border-radius': '10px',
                    '-moz-border-radius': '10px',
                    opacity: .5,
                    color: '#fff'
                }
            });

            if (_key7 == undefined || _key7 == '' || _key7 == '-1')
            {
                if ($("#hdnRespID").val() != '')
                    _key7 = $("#hdnRespID").val();               
            }

            var sc = $(ctrl).attr('data-attr1');
            var _val = $(ctrl).val();
            _img = $(ctrl).attr('data-img');
            var _key2 = $(ctrl).attr('data-attr2');
            var _key3 = $(ctrl).attr('data-attr3');
            var _key4 = $("#img" + _img).attr("data-required");
            var _key5 = $(ctrl).attr('data-attr4');
            var _key6 = $("#thAI_" + _img).attr("data-rel-con");
            var _key8 = $("#thAI_" + _img).attr("data-rel-con-fb");

            if (_key6 == undefined)
            {
                _key6 = $(ctrl).attr("data-rel-con");
                if (_key6 == undefined)
                    _key6 = '';
            }

            if (_key8 == undefined) {
                _key8 = $(ctrl).attr("data-rel-con-fb");
                if (_key8 == undefined)
                    _key8 = '';
            }

            if (_key5 == undefined)
                _key5 = '';
            if (_key3 == '5' || _key3 == '7') {
                if ($(ctrl).is(":checked"))
                    _val = "1";
                else
                    _val = "0";
            }

            $("#img" + _img).attr('class', 'fa fa-spinner fa-pulse fa-3x fa-fw');
            BSW.Web.AssessmentModule.Assessment_WS.SaveValueX(_key2, _key, sc, _val, _key3, _key4, _key5, _key6, _key7, _key8, OnComplete, OnError,
                OnTimeOut);
        }

        function OnComplete(result) {
            if (result != null && result != undefined) {
                var _resp_id;
                var _resp_id_x = "<%= gAssessmentRespID %>";

                if (result.indexOf("#") > 0)
                {
                    var _arr = result.split("#");
                    result = _arr[0];
                    _resp_id = _arr[1];
                }
                if (result == "False")
                    $("#img" + _img).attr('class', 'fa fa-times text-danger');
                else if (result == "True")
                    $("#img" + _img).attr('class', 'fa fa-check text-success');
                else {
                    $.each(jQuery.parseJSON(result), function () {
                        var _res = JSON.stringify(this['Result']).toLowerCase();
                        if (_res == 'true')
                            $("[data-key='" + this['AID'] + "']").show();
                        else
                            $("[data-key='" + this['AID'] + "']").hide();

                        _resp_id = this['RespID'];
                    });
                    $("#img" + _img).attr('class', 'fa fa-check text-success');                    
                }
                if (_resp_id > 0 && _resp_id != _resp_id_x) {
                    $("#hdnRespID").val(_resp_id);
                    if (_resp_id_x == "-1")
                        $('#btnUpdateRespID').click();
                    _resp_id_x = _resp_id;
                }
                $.unblockUI();
            }
        }
        function OnTimeOut(arg) {
            $("#img" + _img).attr('class', 'fa fa-times text-danger');
            $("#img" + _img).attr('data-placement', 'left');
            $("#img" + _img).attr('data-content', 'Timeout!');
            $("#img" + _img).attr('data-container', 'body');
            $("#img" + _img).attr('data-toggle', 'popover-hover');
            $("#img" + _img).attr('title', 'Error');
            $("#divStatus_" + _img).prepend("<span class='badge badge-danger mr-2 mb-2'>Timeout!</span>");
            $("[data-toggle=popover-hover]").popover({
                trigger: 'hover'
            });
            $.unblockUI();
        }
        function OnError(arg) {
            $("#img" + _img).attr('class', 'fa fa-times text-danger');
            $("#img" + _img).attr('data-placement', 'left');
            $("#img" + _img).attr('data-content', arg._message);
            $("#img" + _img).attr('data-container', 'body');
            $("#img" + _img).attr('data-toggle', 'popover-hover');
            $("#img" + _img).attr('title', 'Error');
            $("#divStatus_" + _img).prepend("<span class='badge badge-danger mr-2 mb-2'>" + arg._message + "</span>");
            $("[data-toggle=popover-hover]").popover({
                trigger: 'hover'
            });
            $.unblockUI();
        }

        $(function () {
            $("[id*='datetimepicker']").datetimepicker({
                icons: {
                    time: 'fa fa-time',
                    date: 'fa fa-calendar',
                    up: 'fa fa-chevron-up',
                    down: 'fa fa-chevron-down',
                    previous: 'fa fa-chevron-left',
                    previous: 'fa fa-backward',
                    next: 'fa fa-chevron-right',
                    today: 'fa fa-screenshot',
                    clear: 'fa fa-trash',
                    close: 'fa fa-remove'
                },
                format: 'MM/DD/YYYY'
            });
            $("[data-toggle=tooltip]").tooltip({
                trigger: 'hover'
            });
        });

        function StepsFinishing(event, currentIndex) {
            $('[data-required="True"]').each(function () {
                var data_img_id = $(this).attr("id").replace("img", "")
                var ctrl = $("[data-img='" + data_img_id + "']");
                if ($(ctrl).val() == null || $(ctrl).val() == '') {
                    alert('can not be empty');
                    return false;
                }
                else
                    return true;
            });
            return true;
        }

        function StepsFinished(event, currentIndex) {
            ShowYesNoWarning("This assessment will be closed. You can not change your answers anymore but you can see them any time. Do you want to continue?", function (res) {

                if (res == 1) {
                    $("[id*='btnCloseAssessment']").click();
                    $.blockUI({
                        css: {
                            border: 'none',
                            padding: '15px',
                            backgroundColor: '#000',
                            '-webkit-border-radius': '10px',
                            '-moz-border-radius': '10px',
                            opacity: .5,
                            color: '#fff'
                        }
                    });
                    return true;
                }
                else {
                    return false;
                }
            });
        }
    </script>
</asp:Content>
