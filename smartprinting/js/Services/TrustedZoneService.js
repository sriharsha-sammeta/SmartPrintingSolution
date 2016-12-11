(function () {
    var App = angular.module("App.services");
    App.factory('TrustedZoneService',function ($http, ApiURLs) {
        return {
            'get_delegates': function (user_name) {
                return $http({
                    method: 'GET',
                    url: ApiURLs.get_delegates,
                    params: {
                        userName: user_name
                    }

                })
            },
            'add_delegate': function (user_name,trusted_user_name) {
                return $http({
                    method: 'GET',
                    url: ApiURLs.add_delegate,
                    params: {
                        userName: user_name,
                        trustedUserName: trusted_user_name
                    }
                })
            },
            'change_status': function (user_name, trusted_user_name,state) {
                return $http({
                    method: 'GET',
                    url: ApiURLs.change_delegate_status,
                    params: {
                        userName: user_name,
                        trustedUserName: trusted_user_name,
                        state: state
                    }
                })
            }
        }
    });
})();