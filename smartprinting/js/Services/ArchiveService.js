(function () {
    var App = angular.module("App.services");
    App.factory('ArchiveService', function ($http, ApiURLs) {
        return {
            'get_print_jobs_log': function (user_name) {
                return $http({
                    method: 'GET',
                    url: ApiURLs.get_print_jobs_log,
                    params: {
                        userName: user_name
                    }

                })
            }
        }
    });
})();