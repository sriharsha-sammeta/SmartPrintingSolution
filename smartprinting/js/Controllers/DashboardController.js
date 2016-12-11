(function () {
    var App = angular.module("App.controllers", []);
    App.controller('DashboardController', DashboardController);
    
    
    
    function DashboardController($scope, $rootScope, $state, $filter, $uibModal, adalAuthenticationService, DashboardService) {
        var isAuthenticated = adalAuthenticationService.userInfo.isAuthenticated;
        
        if (isAuthenticated) {
            OnLoad();
        } else {
            $rootScope.$on('adal:loginSuccess', function () {
                isAuthenticated = true;
            });
        }

        $scope.filterPrintJobs = function(printJob, status) {
            if (!status) {
                return printJob;
            }
            var arr = [];

            angular.forEach(printJob, function (pj) {
                switch (status) {
                    case 'Queued': if (pj.Status == 0 && pj.DelegatedTo == null) {
                        arr.push(pj);
                    }
                        break;
                    case 'Pending': if (pj.Status == 1) {
                        arr.push(pj);
                    }
                        break;
                    case 'Delegated': if (pj.Status == 0  && pj.DelegatedTo != null) {
                        arr.push(pj);
                    }
                }
            })

            return arr;
        }
        
        $scope.trashBtnClicked = function (doc) {
            deleteJob(doc);
        }

        $scope.getDate = function (date) {
            return new Date(date).toDateString();
        }

        $scope.decrement = function (id) {
            var doc = $filter('filter')($scope.allDocuments, { Id: id });
            if (doc.length > 0) {
                doc = doc[0];
                console.log("Decrementing " + doc.FileName + "...");
                updateCount( doc.FileName, doc.NumberOfCopies-1,doc);
            }
        }

        $scope.increment = function (id) {
            var doc = $filter('filter')($scope.allDocuments, { Id: id });
            if (doc.length > 0) {
                doc = doc[0];
                console.log("Incrementing " + doc.FileName + "...");
                updateCount( doc.FileName, doc.NumberOfCopies+1,doc);
            }
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
            DashboardService.get_print_jobs($scope.user_name).then(function(response){
                $scope.allDocuments = response.data;
                console.log(response);
            },errorCallback );
        }
        $scope.changeState = function(file_name,status){
            DashboardService.change_state($scope.user_name,file_name,status).then(function(result){
                var doc = $filter('filter')($scope.allDocuments, { FileName: file_name });
                if (doc.length > 0) {
                    doc = doc[0];
                    doc.Status = status;
                }
            },errorCallback);
        }
        function deleteJob(doc){
            DashboardService.delete_job($scope.user_name, doc.FileName).then(function () {
                var index = $scope.allDocuments.indexOf(doc);
                $scope.allDocuments.splice(index, 1);
            }, errorCallback);
        }
        function updateCount(file_name,number_of_copies,doc) {
            DashboardService.update_count($scope.user_name, file_name, number_of_copies).then(function () {
                doc.NumberOfCopies = number_of_copies;
            }, errorCallback);
        }
        function delegateJob( file_name, delegated_user_alias) {
            DashboardService.delegate_job($scope.user_name, file_name, delegated_user_alias).then(function () {
                alert("Delegated to "+ delegated_user_alias)
            }, errorCallback);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


        
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
                    id: id
                }
            });

            modalInstance.result.then(function (delegatedToAlias) {
                //DashboardService.checkUser(delegatedToAlias).then(function (result) {
                //    if (result.d) {
                //        var doc = $filter('filter')($scope.alldocuments, { id: id });
                //        delegatejob(doc.filename, delegatedtoalias);
                //    }
                //}, errorCallback)
                var doc = $filter('filter')($scope.allDocuments, { Id: id });
                if (doc.length > 0) {
                    doc = doc[0];
                    delegateJob(doc.FileName, delegatedToAlias);
                } 
                

            }, function () {
                
            });
            
        };

       

    }

})();