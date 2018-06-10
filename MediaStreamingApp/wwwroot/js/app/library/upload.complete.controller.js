angular.module('libraryApp')
    .controller('UploadCompleteController', ['$scope', '$location', '$route', 'uploadSyncInfo', 'uploadService',
        function($scope, $location, $route, uploadSyncInfo, uploadService) {
            $scope.model = uploadSyncInfo.data.Result;

            // Goto upload page
            if (!$scope.model || $scope.model.length == 0)
                $location.path("/upload");

            $scope.onCompleteUpload = function() {
                uploadService.completeUpload($scope.model[0])
                    .then(function(data) {
                            $location.path("/upload");
                        },
                        function(header) {});
            };
        }
    ]);