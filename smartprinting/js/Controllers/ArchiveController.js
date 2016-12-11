(function () {
    var App = angular.module("App.controllers");
    App.controller('ArchiveController', ArchiveController);
    function ArchiveController($scope, ArchiveService, adalAuthenticationService) {
        var isAuthenticated = adalAuthenticationService.userInfo.isAuthenticated;

        if (isAuthenticated) {
            OnLoad();
        } else {
            $rootScope.$on('adal:loginSuccess', function () {
                isAuthenticated = true;
                ExecuteController();
            });
        }

        $scope.getDate = function (date) {
            return new Date(date).toDateString();
        }

        function errorCallback(response) {

            console.log(response);
            ngToast.create({
                className: 'warning',
                content: response.data.Message
            });
        }

        function OnLoad() {
            var user_name = adalAuthenticationService.userInfo.userName
            $scope.user_name = user_name.slice(0, user_name.search('@'));
            ArchiveService.get_print_jobs_log($scope.user_name).then(function (response) {
                console.log(response.data);
                $scope.printJobs = response.data;
            }, errorCallback)

        }
    }
})();