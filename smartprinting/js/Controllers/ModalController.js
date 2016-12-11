var App = angular.module("App.controllers");
App.controller('ModalInstanceController', function ($scope,$uibModalInstance, id) {
        
    console.log(id);
    $scope.ok = function () {
        $uibModalInstance.close($scope.delegatedToAlias);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
});