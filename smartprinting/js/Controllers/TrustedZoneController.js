(function () {
    var App = angular.module("App.controllers");
    App.controller('TrustedZoneController', TrustedZoneController);
    function TrustedZoneController($scope,$uibModal, adalAuthenticationService, TrustedZoneService, ngToast) {
        var isAuthenticated = adalAuthenticationService.userInfo.isAuthenticated;

        if (isAuthenticated) {
            OnLoad();
        } else {
            $rootScope.$on('adal:loginSuccess', function () {
                isAuthenticated = true;
                ExecuteController();
            });
        }
        
        $scope.statusArray = [
            { status: 'Pending', 'class': 'label label-default' },
            { status: 'Accepted', 'class': 'label label-success' },
            { status: 'Rejected', 'class': 'label label-danger' }]

        function errorCallback(response) {

            console.log(response);
            ngToast.create({
                className: 'warning',
                content: response.data.Message
            });
        }

        var seperateDelegates = function (trustedDelegates) {
            $scope.delegateInvites = [];
            $scope.delegates = [];
            angular.forEach(trustedDelegates, function (trustedDelegate) {
                if(trustedDelegate.UserName != $scope.user_name && trustedDelegate.Status == 0 ) {
                    $scope.delegateInvites.push(trustedDelegate);
                }
                else {
                    $scope.delegates.push(trustedDelegate);
                }
            });
        }

        $scope.getTrustedUserName = function (trustedDelegate, user_name) {
            if (trustedDelegate.UserName == user_name) {
                return trustedDelegate.TrustedUser;
            } else {
                trustedDelegate.UserName;
            }
        }

        $scope.changeDelegationState = function (trustedDelegate, status) {
            TrustedZoneService.change_status($scope.user_name, trustedDelegate, status).then(function (result) {
                alert(trustedDelegate + " is a member of your Trust zone now");
            }, errorCallback);
        }

        function OnLoad() {
            var user_name = adalAuthenticationService.userInfo.userName
            $scope.user_name = user_name.slice(0, user_name.search('@'));
            TrustedZoneService.get_delegates($scope.user_name).then(function (response) {
                console.log(response.data);
                seperateDelegates(response.data);
            }, errorCallback)
                
        }



        $scope.open = function (id, parentSelector) {
            var parentElem = parentSelector ?
              angular.element($document[0].querySelector('.modal-demo ' + parentSelector)) : undefined;
            var modalInstance = $uibModal.open({
                animation: true,
                ariaLabelledBy: 'modal-title',
                ariaDescribedBy: 'modal-body',
                templateUrl: 'myModalContent.html',
                controller: 'ModalInstanceController',
                appendTo: parentElem,
                resolve: {
                    id: -1
                }
            });

            modalInstance.result.then(function (delegatedToAlias) {
                TrustedZoneService.add_delegate($scope.user_name, delegatedToAlias).then(function(response) {
                }, errorCallback)
                   
            }, function () {

            });

        };
    }

    
})();