window.app = window.app || {};

function OrganizationInfo(data) {
    var self = this;
    data = data || {};
    self.ParentID = data.ParentID;
    self.ID = data.ID;
    self.Name = data.Name;
    self.Telephone = data.Telephone;
    self.Address = data.Address;
    self.Latitude = ko.observable(data.Latitude);
    self.Longitude = ko.observable(data.Longitude);
    self.IsSetted = ko.computed(function () {
        if (self.Latitude() && self.Longitude())
            return true;
        return false;
    });
}

window.app.mapViewModel = (function (ko) {
    var organizationList = ko.observableArray(),
		getOrganizations = function (map) {
		    this._ajaxRequest("GET", 'api/organization', null, function (data) {
		        this.organizationList.removeAll();
		        for (var i = 0; i < data.length; i++) {
		            organizationList.push(new OrganizationInfo(data[i]));
		            if (data[i].Latitude && data[i].Longitude) {
		                var marker = new BMap.Marker(new BMap.Point(data[i].Longitude, data[i].Latitude));
		                map.addOverlay(marker);
		                marker.setTitle(data[i].Name);
		            }
		        }
		    });
		};

    var viewmodel = {
        organizationList: organizationList,
        _ajaxRequest: ajaxRequest,
        getOrganizations: getOrganizations,
        currentOrganization: ko.observable(),
        inmarking: ko.observable(false),//是否正在标注当前机构
        saveCoordinate: function (marker) {
            var organization = this.currentOrganization();
            if (organization && organization.ID) {
                var resetPosition = false;
                var lat = organization.Latitude();
                var lng = organization.Longitude();
                if (organization.IsSetted()) {
                    if (!confirm("该机构已经标注过，确认重新标注吗？"))
                        return;
                    resetPosition = true;
                }
                var position = marker.getPosition();
                organization.Latitude(position.lat);
                organization.Longitude(position.lng);
                this._ajaxRequest("PUT", 'api/organization/SetPosition?organizationID=' + organization.ID + '&lng=' + position.lng + '&lat=' + position.lat,
                    null,
                    function (data) {
                        if (data.IsSucceed) {
                            if (resetPosition) {
                                var map = marker.getMap();
                                var overlays = map.getOverlays();
                                for (var i = 0; i < overlays.length; i++) {
                                    if (overlays[i] instanceof BMap.Marker) {
                                        var p = overlays[i].getPosition();
                                        if (p.lat == lat && p.lng == lng) {
                                            map.removeOverlay(overlays[i]);
                                            break;
                                        }
                                    }
                                }
                            }
                            marker.setTitle(organization.Name);
                            this.inmarking(false);
                        }
                        else {
                            alert('标注失败：' + data.Message);
                            organization.Latitude(lat);
                            organization.Longitude(lng);
                        }
                    });
            }
            else {
                alert('请选择机构');
            }
        },
        deleteCoordinate: function (marker) {
            var organization = this.currentOrganization();
            if (organization && organization.ID) {
                this._ajaxRequest("PUT", 'api/organization/SetPosition?organizationID=' + organization.ID + '&lng=&lat=',
                    null,
                    function (data) {
                        if (data.IsSucceed) {
                            organization.Latitude(null);
                            organization.Longitude(null);
                            marker.closeInfoWindow();
                            marker.getMap().removeOverlay(marker);
                        }
                        else
                            alert('删除失败：' + data.Message);
                    });
            }
            else {
                //infoWin.hide();
                marker.closeInfoWindow();
                marker.getMap().removeOverlay(marker);
            }
        },
        setOrganizationOptionColor: function (option, organization) {
            if (organization) {
                //不知为何，以下方式绑定写法不能在IsSetted变化时更新UI
                ko.applyBindingsToNode(option, { style: { color: organization.IsSetted() ? 'red' : 'black' } }, organization);
            }
        },
        setCurrentOrganization: function (marker) {
            var position = marker.getPosition();
            var olist = this.organizationList();
            for (var i = 0; i < olist.length; i++) {
                if (olist[i].Latitude() == position.lat && olist[i].Longitude() == position.lng) {
                    this.currentOrganization(olist[i]);
                    return;
                }
            }
            this.currentOrganization(null);
        }
    };
    return viewmodel;

    function ajaxRequest(type, url, data, success, error, complete, dataType) { // Ajax helper
        $.ajax({
            url: url,
            data: data,
            type: type,
            dataType: dataType || "json",
            //contentType: contentType || "application/json; charset=utf8",
            context: this,//指定this为当前对象viewmodel
            success: success,
            error: error,
            complete: complete
        });
    };
})(ko);

// Initiate the Knockout bindings
ko.applyBindings(window.app.mapViewModel);
