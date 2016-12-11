(function () {
    var App = angular.module("App.services");
    App.factory('DashboardService',function ($http, ApiURLs) {
        return{
            'get_print_jobs': function (user_name) {
                return $http({
                    method: 'GET',
                    url: ApiURLs.get_print_jobs,
                    params: {
                        userName: user_name
                    }
                });
            },
            'update_count': function (user_name,file_name,number_of_copies) {
                return $http({
                    method: 'GET',
                    url: ApiURLs.update_count,
                    params: {
                        userName: user_name,
                        fileName: file_name,
                        numberOfCopies: number_of_copies
                    }
                });
            },
            'delete_job': function (user_name,file_name) {
                return $http({
                    method: 'GET',
                    url: ApiURLs.delete_job,
                    params: {
                        userName: user_name,
                        fileName: file_name
                    }
                });
            },
            'change_state': function (user_name,file_name,status) {
                return $http({
                    method: 'GET',
                    url: ApiURLs.change_state,
                    params: {
                        userName: user_name,
                        fileName: file_name,
                        status: status
                    }

                });
            },

            'delegate_job': function(user_name,file_name,delegated_user_alias){
                return $http({
                    method: 'GET',
                    url: ApiURLs.delegate_job,
                    params: {
                        userName: user_name,
                        fileName: file_name,
                        proxyUserName: delegated_user_alias
                    }

                });
            },

            'checkUser': function (alias) {
                return $http({
                    method: 'GET',
                    url: ApiURLs.drWhom,
                    params: {
                        alias: alias
                    }
                });
            }
    }
    });

})();