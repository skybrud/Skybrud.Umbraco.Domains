angular.module('umbraco').controller('Skybrud.Umbraco.DomainsDashboard.Controller', function ($scope, $http) {

    var baseEp = "/umbraco/Api/SkyDomainsApi/";

    var getAllDomainsEp = baseEp + "GetAllDomains";
    var addDomainEp = baseEp + "AddDomain"; // "?inbound=&outbound="
    var getDomainByIdEp = baseEp + "GetDomnainById"; // "?id="
    var removeDomainEp = baseEp + "RemoveDomain"; // "?id="

    $scope.protocolTypes = ["http", "https"];

    var init = function() {
        
        $http.get(getAllDomainsEp).success(function (r) {
            $scope.allDomains = r.data;
        });

    }


    $scope.deleteDomain = function(id) {

        if (!prompt("Er du sikker?")) return;

        $http.get(removeDomainEp + "?id=" + id).success(function(r) {
            alert(r.data);
        });

        $scope.init();
    }

    $scope.addDomain = function(inboundProtocol, inboundDomain, outboundProtocol, outboundDomain) {

        $http.get(addDomainEp +
            "?inboundProtocol=" +
            inboundProtocol +
            "&inbound=" +
            inboundDomain +
            "&outboundProtocol=" +
            outboundProtocol +
            "&outbound=" +
            outboundDomain).success(function (r) {

            alert(r.data);
        });

    }



    init();
});