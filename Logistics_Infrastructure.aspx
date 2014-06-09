<%@ Page Title="" Language="C#" Culture="he-IL" UICulture="he-IL" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Logistics_Infrastructure.aspx.cs" Inherits="WebApplication._LogisticsInfrastructure" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js" type="text/javascript"></script>
    <script src="http://maps.google.com/maps/api/js?sensor=false&libraries=drawing&language=he" type="text/javascript"></script>
    <style type="text/css">
        /* fix for the google map istorted zoomControl (bootstrap img max-width: 100% issue) */
        #main-map label {
            width: auto;
            display: inline;
        }

        #main-map img {
            max-width: none !important;
        }

        #main-map-big label {
            width: auto;
            display: inline;
        }

        #main-map-big img {
            max-width: none !important;
        }
    </style>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <script type="text/javascript">
        function ShowModal(id) {
            $(id).modal();
        }

        function ChangeClass(id, oldClass, newClass) {
            $(id).removeClass(oldClass).addClass(newClass);
        }
    </script>

    <asp:Panel ID="mapMapViewPanel" runat="server" Visible="false">
        <script type="text/javascript">
            var map;
            var marker;
            var geocoder = new google.maps.Geocoder();
            var latLng = new google.maps.LatLng(32.066158, 34.777819);

            function geocodePosition(pos) {
                geocoder.geocode({
                    latLng: pos
                }, function (responses) {
                    if (responses && responses.length > 0) {
                        updateMarkerAddress(responses[0].formatted_address);
                    } else {
                        updateMarkerAddress('Cannot determine address at this location.');
                    }
                });
            }

            function initialize() {
                map = new google.maps.Map(document.getElementById('main-map'), {
                    zoom: 15,
                    center: latLng,
                    mapTypeId: google.maps.MapTypeId.ROADMAP,
                    disableDefaultUI: true,
                    zoomControl: true
                });

                //callMarker();
            }

            //onload handler to fire off the app
            //google.maps.event.addDomListener(window, 'load', initialize);
        </script>
        <script type="text/javascript">
            var currentlyOpenedInfoWindow = null;

            function init_map(map_canvas_id, lat, lng, zoom, markers, infoWindowContents) {
                var myLatLng = new google.maps.LatLng(lat, lng);

                var options = {
                    zoom: zoom,
                    center: myLatLng,
                    mapTypeId: google.maps.MapTypeId.ROADMAP
                };

                var map_canvas = document.getElementById(map_canvas_id);
                var map = new google.maps.Map(map_canvas, options);

                if (markers && markers.length > 0) {
                    var bounds = new google.maps.LatLngBounds();

                    for (var i = 0; i < markers.length; i++) {
                        var marker = new google.maps.Marker(markers[i]);
                        marker.setMap(map);

                        bounds.extend(marker.getPosition());

                        if (infoWindowContents && infoWindowContents.length > i)
                            createInfoWindow(map, marker, infoWindowContents[i]);
                    }

                    map.fitBounds(bounds);
                    map.setCenter(bounds.getCenter());
                }
            }

            function createInfoWindow(map, marker, infoWindowProperties) {
                var info = new google.maps.InfoWindow(infoWindowProperties);

                google.maps.event.addListener(marker, 'click', function () {
                    if (currentlyOpenedInfoWindow != null)
                        currentlyOpenedInfoWindow.close();

                    info.open(map, marker);
                    currentlyOpenedInfoWindow = info;
                });
            }
        </script>
    </asp:Panel>
    
    <script type="text/javascript">
        var map;
        var marker;
        var markersArray = [];
        var geocoder = new google.maps.Geocoder();
        var latLng = new google.maps.LatLng(32.066158, 34.777819);

        function geocodePosition(pos) {
            geocoder.geocode({
                latLng: pos
            }, function (responses) {
                if (responses && responses.length > 0) {
                    updateMarkerAddress(responses[0].address_components); //updateMarkerAddress(responses[0].formatted_address);

                } else {
                    updateMarkerAddress('Cannot determine address at this location.');
                }
            });
        }

        function updateMarkerStatus(str) {
            document.getElementById('markerStatus').innerHTML = str;
        }

        function updateMarkerPosition(latLng) {
            document.getElementById('markerPosition').innerHTML = [
              latLng.lat(),
              latLng.lng()
            ].join(', ');

            $('#MainContent_markerDetails').val([
              latLng.lat(),
              latLng.lng()
            ].join(', '));
        }

        function updateMarkerAddress(str) {
            var country = '';
            var address = '';
            var postal_code = '';
            var state = '';
            var city = '';
            var sublocality = '';
            var street_number = '';

            for (i = 0; i < str.length; ++i) {
                var component = str[i];

                if (component.types.indexOf("sublocality") > -1)
                    sublocality = component.long_name;
                else if (component.types.indexOf("locality") > -1) //city
                    city = component.long_name;
                else if (component.types.indexOf("postal_code") > -1) //zip
                    postal_code = component.long_name;
                else if (component.types.indexOf("country") > -1)
                    country = component.long_name;
                else if (component.types.indexOf("administrative_area_level_1") > -1) //state
                    state = component.long_name;
                else if (component.types.indexOf("street_address") > -1) //address 1
                    address = address + component.types.streetNumber;
                else if (component.types.indexOf("establishment") > -1)
                    address = address + component.long_name;
                else if (component.types.indexOf("route") > -1)  //address 2
                    address = address + component.long_name;
                else if (component.types.indexOf("street_number") > -1)  //street number
                    street_number = component.long_name;
            }

            $('#MainContent_street').val(address);
            $('#MainContent_streetNumber').val(street_number);
            $('#MainContent_apartmentNumber').val("");

            //document.getElementById('markerAddress').innerHTML = str;
            //document.getElementById('markerAddress').innerHTML = country + " - " + address + " - " + street_number + " - " + postal_code + " - " + state + " - " + city + " - " + sublocality;
            var getAddress = "";

            if (address != "") {
                getAddress += address;

                if (street_number != "")
                    getAddress += " " + street_number;

                getAddress += ", ";

            }
            if (city != "")
                getAddress += city;
            if (country != "")
                getAddress += ", " + country;

            document.getElementById('markerAddress').innerHTML = "כתובת: " + getAddress;

            $('#MainContent_ddlStreetId').val("0");
            $("#MainContent_ddlStreetId option").each(function () {
                if ($(this).text() == address) {
                    $(this).attr('selected', 'selected');
                }
            });

            var selectedCity = $("#MainContent_ddlCityId option:selected").text();
            //alert(selectedCity + "-" + city);

            if (selectedCity != "--- בחר ---") {
                if (selectedCity != city) {
                    alert('אינך מורשה להציב נקודת תשתית במיקום זה, בדוק הגדרות עיר/יישוב ונסה שנית');
                    $('#MainContent_ddlStreetId').val("0");
                    document.getElementById('markerAddress').innerHTML = "";
                    document.getElementById('markerPosition').innerHTML = "";
                    $('#MainContent_streetNumber').val("");
                    $('#MainContent_apartmentNumber').val("");
                    clearOverlays();
                }
            }
        }

        function initialize() {
            map = new google.maps.Map(document.getElementById('main-map'), {
                zoom: 15,
                zoomControl: true,
                center: latLng,
                mapTypeId: google.maps.MapTypeId.ROADMAP,
                disableDefaultUI: true
            });

            var polyOptions = {
                strokeWeight: 0,
                fillOpacity: 0.50,
                editable: true
            };

            //creates a drawing manager attached to the map that allows the user to draw markers, lines, and shapes
            drawingManager = new google.maps.drawing.DrawingManager({
                //drawingMode: google.maps.drawing.OverlayType.MARKER, //default drawing mode
                drawingControlOptions: {
                    drawingModes: [
                      //google.maps.drawing.OverlayType.MARKER
                      //google.maps.drawing.OverlayType.POLYGON
                    ]
                },
                markerOptions: {
                    draggable: true,
                    editable: true
                },
                polylineOptions: {
                    editable: true
                },
                rectangleOptions: polyOptions,
                circleOptions: polyOptions,
                polygonOptions: polyOptions,
                map: map
            });
            //end

            google.maps.event.addListener(drawingManager, 'dragstart', function (event) {
                if (event.type == google.maps.drawing.OverlayType.MARKER) {
                    //alert("00");
                    //updateMarkerAddress('Dragging...');
                }
            });

            google.maps.event.addListener(drawingManager, 'overlaycomplete', function (event) {
                if (event.type == google.maps.drawing.OverlayType.MARKER) {
                    clearOverlays();

                    var projection = event.overlay.getPosition();
                    geocodePosition(projection);
                    //alert(projection);
                    //$('#MainContent_markerDetails').val(projection);
                }

                if (event.type == google.maps.drawing.OverlayType.POLYGON) {
                    overlayClickListener(event.overlay);
                    //alert(event.overlay.getPath().getArray());
                    //$('#MainContent_polygonDetails').val(event.overlay.getPath().getArray());
                }
            });

            callMarker();
        }

        function overlayClickListener(overlay) {
            google.maps.event.addListener(overlay, 'mouseup', function (event) {
                //alert(overlay.getPath().getArray());
                //$('#MainContent_polygonDetails').val(overlay.getPath().getArray());
            });
        }

        function clearOverlays() {
            if (markersArray) {
                for (i in markersArray) {
                    markersArray[i].setMap(null);
                    //alert(i);
                }
            }
        }

        function codeAddress() {
            var address = "";
            var c = document.getElementById('MainContent_ddlCityId');
            var cityId = c.options[c.selectedIndex].value;
            var city = c.options[c.selectedIndex].text;
            var s = document.getElementById('MainContent_ddlStreetId');
            var streetId = s.options[s.selectedIndex].value;
            var street = s.options[s.selectedIndex].text;
            var streetNumber = document.getElementById('MainContent_streetNumber').value;

            //var city = "תל אביב";
            //var city = document.getElementById('MainContent_city').value;
            //var street = "רוטשילד";

            //alert(streetId + "-" + street + "-" + streetNumber + "-" + cityId + "-" + city);

            if (streetId != 0) {
                address += street;

                if (streetNumber != "") {
                    address += ' ' + streetNumber;
                }

                address += ', ';
            }
            if (cityId != 0 && city != "") {
                address += city;
            }

            //address = street + ' ' + streetNumber + ', ' + city;
            //alert(address);

            clearOverlays();

            google.maps.Map.prototype.clearMarkers = function () {
                for (var i = 0; i < this.markers.length; i++) {
                    this.markers[i].setMap(null);
                }
                this.markers = new Array();
            };

            geocoder.geocode({ 'address': address }, function (results, status) {
                if (cityId != 0 && status == google.maps.GeocoderStatus.OK) {
                    map.setCenter(results[0].geometry.location);
                    //$('#MainContent_markerDetails').val(results[0].geometry.location);

                    marker = new google.maps.Marker({
                        zoom: 20,
                        map: map,
                        draggable: true,
                        animation: google.maps.Animation.DROP,
                        position: results[0].geometry.location
                    });
                    markersArray.push(marker);

                    updateMarkerPosition(marker.getPosition());
                    geocodePosition(marker.getPosition());

                    //add dragging event listeners
                    google.maps.event.addListener(marker, 'dragstart', function () {
                        updateMarkerAddress('Dragging...');
                    });

                    google.maps.event.addListener(marker, 'drag', function () {
                        updateMarkerStatus('Dragging...');
                        updateMarkerPosition(marker.getPosition());
                    });

                    google.maps.event.addListener(marker, 'dragend', function () {
                        updateMarkerStatus('Drag ended');
                        geocodePosition(marker.getPosition());
                    });
                } else {
                    alert('אירעה שגיאה במהלך הוספת הנקודה על המפה, בדוק הגדרות עיר/יישוב ונסה שנית');
                    document.getElementById('markerAddress').innerHTML = "";
                    document.getElementById('markerPosition').innerHTML = "";
                    $('#MainContent_streetNumber').val("");
                    $('#MainContent_apartmentNumber').val("");
                    clearOverlays();
                    //alert('Geocode was not successful for the following reason: ' + status);
                    //$('#MainContent_markerDetails').val("");
                }
            });

            //alert(address);
        }

        //initialize();
        //alert("1");
        google.maps.event.addDomListener(window, 'load', initialize); //onload handler to fire off the app
    </script>

    <asp:Literal ID="js" runat="server" />

    <!-- BEGIN PAGE -->
    <div id="main-content">
        <!-- BEGIN PAGE CONTAINER-->
        <div class="container-fluid">
            <!-- BEGIN PAGE HEADER-->
            <div class="row-fluid">
                <div class="span12">
                    <ul class="breadcrumb">
                        <li>
                            <asp:LinkButton ID="homeLinkButton" runat="server" PostBackUrl="~/Account/Information/Control"><i class="icon-home"></i></asp:LinkButton><span class="divider">&nbsp;</span>
                        </li>
                        <li>
                            <asp:Literal ID="topPageTitle" runat="server" Text="לוגיסטיקה" />
                            <span class="divider">&nbsp;</span>
                        </li>
                        <li><a href="#">
                            <asp:Literal ID="pageTitle" runat="server" Text="תשתיות" /></a><span class="divider-last">&nbsp;</span>
                        </li>
                    </ul>
                    <!-- END PAGE TITLE & BREADCRUMB-->
                </div>
            </div>
            <!-- END PAGE HEADER-->

            <!-- BEGIN PAGE CONTENT-->
            <asp:Panel ID="alertPanel" runat="server" CssClass="alert" Visible="false">
                <button class="close" data-dismiss="alert">×</button>
                <asp:Label ID="messageLabel" runat="server" />
            </asp:Panel>

            <div class="row-fluid">
                <div class="span12">
                    <!-- START LIST TABLE -->
                    <div class="widget">
                        <div class="widget-title">
                            <h4><i class="icon-user"></i>&nbsp;<asp:Literal ID="tableTitle" runat="server" Text="תשתיות" /></h4>
                            <span class="tools">
                                <a href="javascript:;" class="icon-chevron-down"></a>
                                <a href="javascript:;" class="icon-remove"></a>
                            </span>
                        </div>
                        <div class="widget-body">
                            <div class="clearfix">
                                <div class="btn-group">
                                    <asp:LinkButton ID="getNewItemLinkButton" runat="server" CssClass="btn" OnCommand="GetLogisticsInfrastructure_Click" CommandArgument="0" CommandName="InsertItem"><i class="icon-plus"></i>&nbsp;הוסף תשתית חדשה</asp:LinkButton>
                                </div>
                                <div class="btn-group pull-left">
                                    <button class="btn dropdown-toggle" data-toggle="dropdown">
                                        אפשרויות <i class="icon-angle-down"></i>
                                    </button>
                                    <ul class="dropdown-menu pull-left">
                                        <li>
                                            <asp:LinkButton ID="changeViewLinkButton" runat="server" OnCommand="ChangeViewButton_Click" CommandName="List">תצוגת רשימה</asp:LinkButton></li>
                                        <li><a href="#myModalFilter" data-toggle="modal">פילוח</a></li>
                                        <li>
                                            <asp:LinkButton ID="editButton" runat="server" OnCommand="clearFilterButton_Click">נקה פילוח</asp:LinkButton></li>
                                    </ul>
                                </div>
                            </div>
                            <div id="myModalFilter" class="modal hide fade" tabindex="-1" role="dialog" aria-labelledby="myModalLabel1" aria-hidden="true">
                                <div class="modal-header">
                                    <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
                                    <h3 id="myModalLabel1">מסנן פילוח</h3>
                                </div>
                                <div class="modal-body">
                                    <div class="form-vertical">
                                        <asp:UpdatePanel ID="filterTypeUpdatePanel" runat="server" UpdateMode="Conditional">
                                            <ContentTemplate>
                                                <div class="control-group" style="float: right; margin-left: 20px;">
                                                    <label class="control-label">
                                                        סוג תשתית
                                                    </label>
                                                    <div>
                                                        <asp:DropDownList ID="filterTypeId" runat="server"
                                                            ItemType="WebApplication.Models.LogisticsInfrastructureType"
                                                            SelectMethod="GetLogisticsInfrastructureType" DataTextField="name" DataValueField="ID"
                                                            OnSelectedIndexChanged="filterTypeId_SelectedIndexChanged" AutoPostBack="true"
                                                            OnDataBound="filterTypeId_DataBound"
                                                            CssClass="input-medium m-wrap" />
                                                    </div>
                                                </div>
                                                <asp:Panel ID="filterSubTypeIdPanel" runat="server" CssClass="control-group" Style="float: right; margin-left: 20px;" Visible="false">
                                                    <label class="control-label">
                                                        סוג מבנה
                                                    </label>
                                                    <div>
                                                        <asp:DropDownList ID="filterSubTypeId" runat="server" CssClass="input-medium m-wrap" />
                                                    </div>
                                                </asp:Panel>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                        <div class="control-group" style="float: right; margin-left: 20px;">
                                            <label class="control-label">
                                                שם תשתית
                                            </label>
                                            <div>
                                                <asp:TextBox ID="nameFilter" runat="server" CssClass="input-small" />
                                            </div>
                                        </div>
                                        <div style="clear: both;"></div>
                                        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                                            <ContentTemplate>
                                                <div class="control-group" style="float: right; margin-left: 20px;">
                                                    <label class="control-label">
                                                        עיר/יישוב 
                                                    </label>
                                                    <div>
                                                        <asp:DropDownList ID="cityIdFilter" runat="server"
                                                            ItemType="WebApplication.Models.City"
                                                            SelectMethod="GetCities" DataTextField="name" DataValueField="SignId"
                                                            OnDataBound="cityIdFilter_DataBound"
                                                            OnSelectedIndexChanged="cityIdFilter_SelectedIndexChanged" AutoPostBack="true"
                                                            CssClass="input-medium m-wrap" />
                                                    </div>
                                                </div>
                                                <div class="control-group" style="float: right; margin-left: 20px;">
                                                    <label class="control-label">
                                                        רחוב
                                                    </label>
                                                    <div>
                                                        <asp:DropDownList ID="streetIdFilter" runat="server"
                                                            DataTextField="name" DataValueField="ID"
                                                            OnDataBound="streetIdFilter_DataBound"
                                                            CssClass="input-medium m-wrap" />
                                                        <asp:TextBox ID="streetFilter" runat="server" CssClass="input-small" Visible="false" />
                                                    </div>
                                                </div>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                        <div class="control-group" style="float: right; margin-left: 20px;">
                                            <label class="control-label">
                                                מספר בית
                                            </label>
                                            <div>
                                                <asp:TextBox ID="streetNumberFilter" runat="server" CssClass="input-micro" />
                                            </div>
                                        </div>
                                        <div class="control-group" style="float: right; margin-left: 20px;">
                                            <label class="control-label">
                                                דירה
                                            </label>
                                            <div>
                                                <asp:TextBox ID="apartmentNumberFilter" runat="server" CssClass="input-micro" />
                                            </div>
                                        </div>
                                    </div>
                                    <div style="clear: both;"></div>
                                </div>
                                <div class="modal-footer">
                                    <button class="btn" data-dismiss="modal" aria-hidden="true">סגור</button>
                                    <asp:Button ID="submitFilterButton" runat="server" Text="הפעל מסנן" CssClass="btn btn-primary" OnClick="submitFilterButton_Click" />
                                    <asp:Button ID="clearFilterButton" runat="server" Text="נקה" CssClass="btn btn-primary" OnCommand="clearFilterButton_Click" Visible="false" />
                                </div>
                            </div>
                            <div class="space15"></div>
                            <asp:Panel ID="logisticsInfrastructureListPanel" runat="server" Visible="false">
                                <asp:GridView ID="gvLogisticsInfrastructure" runat="server" AllowPaging="true" AllowSorting="false" PageSize="10"
                                    ItemType="WebApplication.Models.LogisticsInfrastructure"
                                    DataKeyNames="ID" AutoGenerateColumns="false"
                                    GridLines="None" ShowHeader="true" Width="100%"
                                    CssClass="table table-striped table-bordered"
                                    OnPageIndexChanging="gvLogisticsInfrastructure_PageIndexChanging" OnRowDataBound="gvLogisticsInfrastructure_RowDataBound">
                                    <Columns>
                                        <asp:TemplateField HeaderText="#" Visible="false">
                                            <ItemTemplate>
                                                <asp:Label ID="idLabel" runat="server" Text='<%# Eval("ID") %>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="שם תשתית">
                                            <ItemTemplate>
                                                <asp:Label ID="logisticsInfrastructureNameLabel" runat="server" Text='<%# Eval("Name") %>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="סוג תשתית">
                                            <ItemTemplate>
                                                <asp:Label ID="logisticsInfrastructureType" runat="server" Text='<%# Eval("type") %>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="כתובת">
                                            <ItemTemplate>
                                                <asp:Label ID="logisticsInfrastructureAddress" runat="server" Text='<%# Eval("address") %>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField ItemStyle-Width="70">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="getEditItemLinkButton" runat="server" CssClass="btn btn-primary" OnCommand="GetLogisticsInfrastructure_Click" CommandArgument='<%# Eval("ID") %>' CommandName="EditItem"><i class='icon-pencil icon-white'></i>&nbsp;ערוך</asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                    <PagerSettings Mode="NumericFirstLast" NextPageText="Next&amp;nbsp;&amp;gt;" PreviousPageText="&amp;lt;&amp;nbsp;Previous" PageButtonCount="5" />
                                    <EmptyDataTemplate>לא נמצאו פריטים</EmptyDataTemplate>
                                </asp:GridView>
                            </asp:Panel>
                            <asp:Panel ID="logisticsInfrastructureMapPanel" runat="server">
                                <div id="main-map-big" style="margin-top: 0px; height: 550px; width: 100%;"></div>
                            </asp:Panel>
                        </div>
                    </div>
                    <!-- END LIST TABLE -->

                    <div class="space15"></div>

                    <!-- START DIALOG BOX -->
                    <div class="widget">
                        <div id="myModalEditItem" class="modal hide fade" tabindex="-1" role="dialog" aria-labelledby="myModalLabel2" aria-hidden="true">
                            <asp:Label ID="infrastructureId" runat="server" Text="0" Visible="false" />
                            <div class="modal-header">
                                <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
                                <h3 id="myModalLabel2">
                                    <asp:Literal ID="updateItemTitleModal" runat="server" Text="הוספת תשתית" /></h3>
                            </div>
                            <div class="modal-body">
                                <!--BEGIN TABS-->
                                <div class="tabbable tabbable-custom">
                                    <ul class="nav nav-tabs">
                                        <li class="active"><a href="#tab_1_1" data-toggle="tab">פרטים</a></li>
                                        <li><a href="#tab_1_2" data-toggle="tab">ציוד</a></li>
                                        <li><a href="#tab_1_3" data-toggle="tab">אוכלוסיה</a></li>
                                        <li><a href="#tab_1_4" data-toggle="tab">תמונות</a></li>
                                    </ul>
                                    <div class="tab-content">
                                        <div class="tab-pane active" id="tab_1_1">
                                            <div class="scroller" data-height="290px">
                                                <script type="text/javascript">
                                                    Sys.Application.add_init(appl_init);

                                                    function appl_init() {
                                                        var pgRegMgr = Sys.WebForms.PageRequestManager.getInstance();
                                                        pgRegMgr.add_endRequest(EndHandler);
                                                    }

                                                    function EndHandler() {
                                                        //this will be called whenever your updatepanel update 
                                                        //it will be trigger after the update updatepanel
                                                        if (!jQuery().chosen) {
                                                            return;
                                                        }
                                                        $(".chosen").chosen();
                                                        $(".chosen-with-diselect").chosen({
                                                            allow_single_deselect: true
                                                        });
                                                    }
                                                </script>
                                                <asp:ValidationSummary ID="ValidationSummaryVg1" runat="server"
                                                    HeaderText="אירעו השגיאות הבאות:"
                                                    DisplayMode="BulletList"
                                                    EnableClientScript="true" ValidationGroup="vg1" />
                                                <asp:Label ID="itemId" runat="server" Visible="false" Text="0" />
                                                <div style="float: right;">
                                                    <asp:UpdatePanel ID="typeUpdatePanel" runat="server" UpdateMode="Conditional">
                                                        <ContentTemplate>
                                                            <div class="control-group" style="float: right; margin-left: 20px;">
                                                                <label class="control-label">
                                                                    סוג תשתית
                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator10" runat="server"
                                                                    Display="Dynamic" Text="*" ErrorMessage="הוסף סוג תשתית" CssClass="text-error"
                                                                    ControlToValidate="typeId" InitialValue="0" ValidationGroup="vg1" />
                                                                </label>
                                                                <div>
                                                                    <asp:DropDownList ID="typeId" runat="server"
                                                                        ItemType="WebApplication.Models.LogisticsInfrastructureType"
                                                                        SelectMethod="GetLogisticsInfrastructureType" DataTextField="name" DataValueField="ID"
                                                                        OnSelectedIndexChanged="typeId_SelectedIndexChanged" AutoPostBack="true"
                                                                        OnDataBound="typeId_DataBound"
                                                                        CssClass="input-medium m-wrap" />
                                                                </div>
                                                            </div>
                                                            <asp:Panel ID="subTypeIdPanel" runat="server" CssClass="control-group" Style="float: right; margin-left: 20px;" Visible="false">
                                                                <label class="control-label">
                                                                    סוג מבנה
                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator11" runat="server"
                                                                    Display="Dynamic" Text="*" ErrorMessage="הוסף סוג מבנה" CssClass="text-error"
                                                                    ControlToValidate="subTypeId" InitialValue="0" ValidationGroup="vg1" />
                                                                </label>
                                                                <div>
                                                                    <asp:DropDownList ID="subTypeId" runat="server"
                                                                        OnSelectedIndexChanged="subTypeId_SelectedIndexChanged" AutoPostBack="true"
                                                                        CssClass="input-medium m-wrap" />
                                                                </div>
                                                            </asp:Panel>
                                                        </ContentTemplate>
                                                    </asp:UpdatePanel>
                                                    <div style="clear: both;"></div>
                                                    <div class="control-group" style="float: right; margin-left: 20px;">
                                                        <label class="control-label">
                                                            שם התשתית
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator12" ControlToValidate="name"
                                                            ValidationGroup="vg1" runat="server" Display="Dynamic" Text="*" ErrorMessage="הוסף שם תשתית" CssClass="text-error" />
                                                        </label>
                                                        <div>
                                                            <asp:TextBox ID="name" runat="server" />
                                                        </div>
                                                    </div>
                                                    <div style="clear: both;"></div>

                                                    <asp:UpdatePanel ID="cityUpdatePanel" runat="server" UpdateMode="Conditional">
                                                        <ContentTemplate>
                                                            <asp:Literal ID="jsInner" runat="server" Text="" />
                                                            <asp:Panel ID="cityPanel" runat="server" Style="float: right; margin-left: 20px;">
                                                                <div class="control-group">
                                                                    <label class="control-label">
                                                                        עיר/יישוב 
                                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server"
                                                                            Display="Dynamic" Text="*" ErrorMessage="הוסף עיר/יישוב" CssClass="text-error"
                                                                            ControlToValidate="ddlCityId" InitialValue="0" ValidationGroup="vg1" />
                                                                    </label>
                                                                    <div>
                                                                        <asp:DropDownList ID="ddlCityId" runat="server"
                                                                            ItemType="WebApplication.Models.City"
                                                                            SelectMethod="GetCities" DataTextField="name" DataValueField="SignId"
                                                                            OnDataBound="ddlCityId_DataBound"
                                                                            OnSelectedIndexChanged="ddlCityId_SelectedIndexChanged" AutoPostBack="true"
                                                                            CssClass="input-medium m-wrap" />
                                                                        <asp:TextBox ID="city" runat="server" CssClass="autoSuggestCity" Visible="false" />
                                                                    </div>
                                                                </div>
                                                            </asp:Panel>
                                                            <div class="control-group" style="float: right; margin-left: 20px;">
                                                                <label class="control-label">
                                                                    רחוב
                                                                </label>
                                                                <div>
                                                                    <asp:DropDownList ID="ddlStreetId" runat="server"
                                                                        DataTextField="name" DataValueField="ID"
                                                                        OnDataBound="ddlStreetId_DataBound"
                                                                        OnSelectedIndexChanged="ddlStreetId_SelectedIndexChanged" AutoPostBack="true"
                                                                        CssClass="input-medium m-wrap" />
                                                                    <asp:TextBox ID="street" runat="server" CssClass="input-small" style="display:none;" />
                                                                </div>
                                                            </div>

                                                            <div class="control-group" style="float: right; margin-left: 20px;">
                                                                <label class="control-label">
                                                                    מספר בית
                                                                </label>
                                                                <div>
                                                                    <asp:TextBox ID="streetNumber" runat="server" CssClass="input-micro" />
                                                                </div>
                                                            </div>
                                                            <div class="control-group" style="float: right; margin-left: 20px;">
                                                                <label class="control-label">
                                                                    דירה
                                                                </label>
                                                                <div>
                                                                    <asp:TextBox ID="apartmentNumber" runat="server" CssClass="input-micro" />
                                                                </div>
                                                            </div>
                                                            <div style="clear: both;"></div>
                                                            <div class="control-group" style="float: right; margin-left: 20px;">
                                                                <div>
                                                                </div>
                                                            </div>
                                                            <div style="clear: both;"></div>
                                                            <div class="control-group" style="float: right; margin-left: 20px;">
                                                                <div style="float: right;">
                                                                    <asp:LinkButton ID="findMapButton" runat="server" CssClass="btn btn-warning" OnClientClick="codeAddress(); return false;" ValidationGroup="vg4"><i class="icon-plus icon-white"></i>&nbsp;עדכן נקודה על המפה</asp:LinkButton>
                                                                </div>
                                                                <div style="float: right; margin-top: 5px; margin-right: 10px;">
                                                                    <div id="markerAddress" style="display: block;"></div>
                                                                </div>
                                                                <div style="clear: both;"></div>
                                                                <asp:LinkButton ID="clearMapButton" runat="server" Text="נקה מפה" CssClass="btn" OnCommand="ClearMap_Click" Visible="false" />
                                                            </div>
                                                        </ContentTemplate>
                                                    </asp:UpdatePanel>
                                                </div>
                                                <div style="float: left;">
                                                    <div id="color-palette" style="display: none;"></div>
                                                    <div id="main-map" style="border: 1px solid #808080; margin-top: 5px; height: 400px; width: 600px;"></div>
                                                    <div id="markerStatus" style="display: none;"></div>
                                                    <div id="markerPosition" style="display: block;"></div>
                                                    <div style="display: none;">
                                                        <br />
                                                        <asp:TextBox ID="polygonDetails" runat="server" TextMode="MultiLine" Width="350" />
                                                        <br />
                                                        <asp:TextBox ID="markerDetails" runat="server" TextMode="MultiLine" Width="350" />
                                                    </div>
                                                </div>
                                                <div style="clear: both;"></div>
                                            </div>
                                        </div>
                                        <div class="tab-pane" id="tab_1_2">
                                            <div class="scroller" data-height="290px">
                                                <asp:UpdatePanel ID="equipmentUpdatePanel" runat="server" UpdateMode="Conditional">
                                                    <ContentTemplate>
                                                        <asp:ValidationSummary ID="ValidationSummary1" runat="server"
                                                            HeaderText="אירעו השגיאות הבאות:"
                                                            DisplayMode="BulletList"
                                                            EnableClientScript="true" ValidationGroup="vg3" />
                                                        <asp:Panel ID="addInfrastructureAlertPanel" runat="server" CssClass="alert" Visible="false">
                                                            <button class="close" data-dismiss="alert">×</button>
                                                            <asp:Label ID="addInfrastructureAlertLabel" runat="server" />
                                                        </asp:Panel>
                                                        <div class="row-fluid">
                                                            <div class="span12">
                                                                <div class="accordion-group">
                                                                    <div class="accordion-heading">
                                                                        <a class="accordion-toggle collapsed" data-toggle="collapse" data-parent="#accordion1" href="#addEquipment">
                                                                            <i class=" icon-plus"></i>
                                                                            הוסף ציוד
                                                                        </a>
                                                                    </div>
                                                                    <div id="addEquipment" class="accordion-body collapse">
                                                                        <asp:Label ID="itemIdEquipmentType" runat="server" Text="0" Visible="false" />
                                                                        <div class="accordion-inner">
                                                                            <div>
                                                                                <div class="control-group" style="float: right; margin-left: 20px;">
                                                                                    <label class="control-label">
                                                                                        שם הציוד
                                                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator4" ControlToValidate="nameEquipment"
                                                                                ValidationGroup="vg3" runat="server" Text="*" ErrorMessage="הוסף שם ציוד" CssClass="text-error" />
                                                                                    </label>
                                                                                    <div>
                                                                                        <asp:TextBox ID="nameEquipment" runat="server" CssClass="input-medium" />
                                                                                    </div>
                                                                                </div>
                                                                                <div class="control-group" style="float: right; margin-left: 20px;">
                                                                                    <label class="control-label">
                                                                                        כמות מומלצת
                                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="amountRecommendedEquipment"
                                                                ValidationGroup="vg3" runat="server" Text="*" ErrorMessage="הוסף כמות מומלצת" CssClass="text-error" />
                                                                                    </label>
                                                                                    <div>
                                                                                        <asp:TextBox ID="amountRecommendedEquipment" runat="server" CssClass="input-micro" />
                                                                                    </div>
                                                                                </div>
                                                                                <div class="control-group" style="float: right; margin-left: 20px;">
                                                                                    <label class="control-label">
                                                                                        כמות בפועל
                                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" ControlToValidate="amountFoundEquipment"
                                                                ValidationGroup="vg3" runat="server" Text="*" ErrorMessage="הוסף כמות בפועל" CssClass="text-error" />
                                                                                    </label>
                                                                                    <div>
                                                                                        <asp:TextBox ID="amountFoundEquipment" runat="server" CssClass="input-micro" />
                                                                                    </div>
                                                                                </div>
                                                                                <div style="clear: both;"></div>
                                                                                <div>
                                                                                    <div style="float: right;">
                                                                                        <asp:Button ID="saveEquipmentButton" runat="server" Text="הוסף" CssClass="btn btn-primary" CommandName="Insert" CommandArgument="0" OnCommand="AddEquipment_Click" ValidationGroup="vg3" />
                                                                                    </div>
                                                                                    <div style="float: left;">
                                                                                        <asp:Button ID="deleteEquipmentButton" runat="server" Text="מחק" CssClass="btn btn-warning" CommandName="Delete" CommandArgument="0" OnCommand="AddEquipment_Click" Visible="false" />
                                                                                    </div>
                                                                                </div>
                                                                                <div style="clear: both;"></div>
                                                                            </div>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </div>
                                                        <div class="space15"></div>
                                                        <asp:Panel ID="equipmentPanel" runat="server" CssClass="formItemFloatClear">
                                                            <asp:Panel ID="addEquipmentPanel" runat="server" Visible="false">
                                                            </asp:Panel>
                                                            <div class="widget">
                                                                <div class="widget-title">
                                                                    <h4><i class="icon-user"></i>&nbsp;רשימת ציוד</h4>
                                                                    <div style="float: left;">
                                                                        <span class="tools">
                                                                            <a href="javascript:;" class="icon-chevron-down"></a>
                                                                            <!--<a href="javascript:;" class="icon-remove"></a>-->
                                                                        </span>
                                                                    </div>
                                                                    <div style="float: left; margin-left: 10px; margin-top: 3px;">
                                                                        <asp:Button ID="saveAllEquipmentButton" runat="server" Text="שמור שינויים ברשימת ציוד" CssClass="btn btn-primary" CommandName="EditAll" OnCommand="EditAllEquipment_Click" />
                                                                    </div>
                                                                </div>
                                                                <div class="widget-body">
                                                                    <asp:Panel ID="listEquipmentPanel" runat="server">
                                                                        <asp:GridView ID="gvEquipment" runat="server" AllowPaging="true" AllowSorting="false" PageSize="10"
                                                                            DataKeyNames="ID" AutoGenerateColumns="false"
                                                                            GridLines="None" ShowHeader="true" Width="100%"
                                                                            CssClass="table table-striped table-bordered"
                                                                            OnRowDataBound="gvEquipment_RowDataBound">
                                                                            <Columns>
                                                                                <asp:TemplateField HeaderText="#" Visible="false">
                                                                                    <ItemTemplate>
                                                                                        <asp:Label ID="equipmentId" runat="server" Text='<%# Eval("ID") %>' />
                                                                                    </ItemTemplate>
                                                                                </asp:TemplateField>
                                                                                <asp:TemplateField HeaderText="שם ציוד">
                                                                                    <ItemTemplate>
                                                                                        <asp:Label ID="equipmentName" runat="server" Text='<%# Eval("comment") %>' />
                                                                                    </ItemTemplate>
                                                                                </asp:TemplateField>
                                                                                <asp:TemplateField HeaderText="כמות מומלצת">
                                                                                    <ItemTemplate>
                                                                                        <asp:Label ID="equipmentAmountRecommended" runat="server" Text='<%# Eval("amountRecommended") %>' />
                                                                                    </ItemTemplate>
                                                                                </asp:TemplateField>
                                                                                <asp:TemplateField HeaderText="כמות בפועל">
                                                                                    <ItemTemplate>
                                                                                        <asp:TextBox ID="equipmentAmountFound" runat="server" Text='<%# Eval("amountFound") %>' CssClass="input-micro" Style="margin-bottom: -10px" />
                                                                                    </ItemTemplate>
                                                                                </asp:TemplateField>
                                                                                <asp:TemplateField HeaderText="שם מבצע">
                                                                                    <ItemTemplate>
                                                                                        <asp:TextBox ID="equipmentOperetor" runat="server" Text='<%# Eval("operetor") %>' CssClass="input-small" Style="margin-bottom: -10px" />
                                                                                    </ItemTemplate>
                                                                                </asp:TemplateField>
                                                                                <asp:TemplateField HeaderText="אחראי">
                                                                                    <ItemTemplate>
                                                                                        <asp:Label ID="equipmentManager" runat="server" Text='<%# Eval("equipmentManager") %>' />
                                                                                    </ItemTemplate>
                                                                                </asp:TemplateField>
                                                                                <asp:TemplateField ItemStyle-Width="70">
                                                                                    <ItemTemplate>
                                                                                        <asp:LinkButton ID="getEditItemLinkButton" runat="server" CssClass="btn btn-primary" OnCommand="GetEquipmentItem_Click" CommandArgument='<%# Eval("ID") %>' CommandName="EditItem"><i class='icon-pencil icon-white'></i>&nbsp;ערוך</asp:LinkButton>
                                                                                    </ItemTemplate>
                                                                                </asp:TemplateField>
                                                                            </Columns>
                                                                            <PagerSettings Mode="NumericFirstLast" NextPageText="Next&amp;nbsp;&amp;gt;" PreviousPageText="&amp;lt;&amp;nbsp;Previous" PageButtonCount="5" />
                                                                            <EmptyDataTemplate>לא נמצאו פריטים</EmptyDataTemplate>
                                                                        </asp:GridView>
                                                                    </asp:Panel>
                                                                </div>
                                                            </div>
                                                        </asp:Panel>
                                                    </ContentTemplate>
                                                </asp:UpdatePanel>
                                            </div>
                                        </div>
                                        <div class="tab-pane" id="tab_1_3">
                                            <div class="scroller" data-height="290px">
                                                <div class="control-group" style="float: right; margin-left: 20px;">
                                                    <label class="control-label">הוסף אוכלוסיה לתשתית</label>
                                                    <div class="controls">
                                                        <asp:ListBox ID="lbPopulation" runat="server"
                                                            SelectionMode="Multiple"
                                                            ItemType="WebApplication.Models.Population"
                                                            SelectMethod="GetLogisticsInfrastructurePopulation" DataTextField="firstName" DataValueField="ID"
                                                            CssClass="chosen" multiple="multiple" TabIndex="6" Style="width: 400px; height: 100px;" />
                                                        <asp:DropDownList Visible="false" ID="ddlPopulation" runat="server"
                                                            ItemType="WebApplication.Models.Population"
                                                            SelectMethod="GetLogisticsInfrastructurePopulation" DataTextField="firstName" DataValueField="ID"
                                                            CssClass="chosen" multiple="multiple" TabIndex="6" Style="width: 400px;" />
                                                    </div>
                                                </div>
                                                <div style="clear: both;"></div>
                                            </div>
                                        </div>
                                        <div class="tab-pane" id="tab_1_4">
                                            <div class="scroller" data-height="290px">
                                                <asp:UpdatePanel ID="imagesUpdatePanel" runat="server" UpdateMode="Conditional">
                                                    <ContentTemplate>
                                                        <script type="text/javascript">
                                                            function showUploadStartUpdateProgress(sender, args) {
                                                                //var UpdateProgress = document.getElementById('ctl00_ContentPlaceHolder1_UpdateProgress');
                                                                //UpdateProgress.style.display = 'block';
                                                            }
                                                            function showUploadEndUpdateProgress(sender, args) {
                                                                //alert("00");
                                                                //var results = document.getElementById('uploadResult');
                                                                //var refreshImagesButton = document.getElementById('MainContent_refreshImagesButton');
                                                                //results.innerHTML = "קובץ נוסף בהצלחה, לחץ על רענן רשימת תמונות";
                                                                //refreshImagesButton.style.display = 'block';
                                                            }
                                                        </script>
                                                        <div class="control-group" style="float: right; margin-left: 20px;">
                                                            <label class="control-label">
                                                                העלאת תמונות לתשתית
                                                            </label>
                                                            <div>
                                                                <div style="float: right;">
                                                                    <ajaxToolkit:AsyncFileUpload ID="AsyncFileUpload_contract" runat="server" 
                                                                        OnClientUploadStarted="showUploadStartUpdateProgress"
                                                                        OnUploadedComplete="AsyncFileUploadUploadedComplete" 
                                                                        OnClientUploadComplete="showUploadEndUpdateProgress"
                                                                        OnClientUploadError="showUploadEndUpdateProgress" Width="200" />
                                                                </div>
                                                                <div style="float: right; margin-top: 5px; margin-right: 10px;">
                                                                    <div id="uploadResult"></div>

                                                                </div>
                                                                <div style="float: right; margin-right: 10px;">
                                                                    <asp:Button ID="refreshImagesButton" runat="server" Text="רענן רשימה" CssClass="btn btn-primary" OnCommand="refreshImages_Click" Style="display: none;" />
                                                                </div>
                                                            </div>
                                                        </div>
                                                        <div style="clear: both;"></div>

                                                        <div class="control-group" style="float: right; margin-left: 20px;">
                                                            <label class="control-label">
                                                                רשימת תמונות
                                                            </label>
                                                            <div>
                                                                <div style="padding-top: 10px;">
                                                                    <asp:ListView ID="lvImages" runat="server" DataKeyNames="ID" GroupItemCount="4">
                                                                        <LayoutTemplate>
                                                                            <div>
                                                                                <table id="groupPlaceholderContainer" runat="server" width="100%" border="0" cellpadding="0" cellspacing="0">
                                                                                    <tr id="groupPlaceholder" runat="server" />
                                                                                </table>
                                                                            </div>
                                                                        </LayoutTemplate>
                                                                        <EmptyItemTemplate>
                                                                            <td id="Td1" runat="server" />
                                                                        </EmptyItemTemplate>
                                                                        <GroupTemplate>
                                                                            <tr id="itemPlaceholderContainer" runat="server">
                                                                                <td id="itemPlaceholder" runat="server" />
                                                                            </tr>
                                                                        </GroupTemplate>
                                                                        <EmptyDataTemplate></EmptyDataTemplate>
                                                                        <ItemTemplate>
                                                                            <td valign="top">
                                                                                <div style="padding-left: 20px; padding-bottom: 20px;">
                                                                                    <asp:Label ID="itemId" runat="server" Text='<%# Eval("ID") %>' Visible="false" />
                                                                                    <asp:Image ID="Image" runat="server" ImageUrl='<%# String.Format("/Uploads/{0}/{1}", Eval("clientId"), Eval("fileName")) %>' Height="150" />
                                                                                    <div style="padding-top: 2px">
                                                                                        <asp:LinkButton ID="deleteImageLinkButton" Visible="false" runat="server" CssClass="btn btn-primary" OnCommand="addEditImage_Click" CommandArgument='<%# Eval("ID") %>' CommandName="DeleteItem"><i class='icon-pencil icon-white'></i>&nbsp;מחק</asp:LinkButton>
                                                                                        <asp:Button ID="deleteImageButton" runat="server" Text="מחק" CssClass="btn btn-primary" CommandName="DeleteItem" CommandArgument='<%# Eval("ID") %>' OnCommand="addEditImage_Click" />
                                                                                    </div>
                                                                                </div>
                                                                            </td>
                                                                        </ItemTemplate>
                                                                    </asp:ListView>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </ContentTemplate>
                                                </asp:UpdatePanel>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <!--END TABS-->
                            </div>
                            <div class="modal-footer">
                                <div style="float: right;">
                                    <button class="btn" data-dismiss="modal" aria-hidden="true">סגור</button>
                                    <asp:Button ID="saveButton" runat="server" Text="שמור" CssClass="btn btn-primary" OnCommand="addEditInfrastructure_Click" CommandName="Insert" ValidationGroup="vg1" />
                                </div>
                                <div style="float: left;">
                                    <asp:Button ID="deleteButton" runat="server" Text="מחק תשתית" CssClass="btn btn-warning" OnCommand="addEditInfrastructure_Click" CommandName="Delete" Visible="false" />
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- END DIALOG BOX -->
                </div>
            </div>
            <!-- END PAGE CONTENT-->
        </div>
        <!-- END PAGE CONTAINER-->
    </div>
    <!-- END PAGE -->
</asp:Content>
