angular.module('libraryApp')
    .controller('UploadController', ['$scope', '$q', '$location', 'SharedDataService', 'uploadService', 'uploadSyncInfo', function($scope, $q, $location, SharedDataService, uploadService, uploadSyncInfo) {

        // Goto upload complete page
        var uploadSyncInfos = uploadSyncInfo.data.Result;
        //alert(uploadSyncInfos);
        if (uploadSyncInfos && uploadSyncInfos.length > 0)
            $location.path("/upload/complete");

        $scope.Warning = {
            IsActive: false,
            Text: 'Please do not refresh the page !'
        };

        $scope.UploadInputs = [
            { Name: 'upload-media-file1', File: '', Size: '', LastModified: '' }
        ];

        $scope.Status = {
            IsReadyToUpload: false,
            IsBusy: false,
            UploadInfo: {
                uploadIdx: 0,
                percentage: 0,
                arrFiles: null
            }
        };

        $scope.clearInputMediaFiles = function() {
            $scope.Status.IsReadyToUpload = false;
            $scope.UploadInputs = [
                { Name: 'upload-media-file1', File: '', Size: '', LastModified: '' }
            ];
        };

        $scope.ValidateSelectedMediaFiles = function() {
            if ($scope.UploadInputs.length > 1)
                $scope.Status.IsReadyToUpload = true;
        };

        $scope.onMediaFileChange = function(name) {
            var uploadInputIndex = 0;
            for (var i = 0; i < $scope.UploadInputs.length; i++) {
                if ($scope.UploadInputs[i].Name === name) {
                    uploadInputIndex = i;
                    break;
                }
            }

            var files = $("input[name='" + name + "']")[0].files;
            if (files && files.length > 0) {
                var file = files[0];
                $scope.UploadInputs[uploadInputIndex].File = file.name;
                $scope.UploadInputs[uploadInputIndex].Size = file.size;
                $scope.UploadInputs[uploadInputIndex].LastModified = file.lastModified;
            } else {
                if ($scope.UploadInputs.length > 1) {
                    $scope.UploadInputs.splice(uploadInputIndex, 1);
                    $scope.$apply();
                }
                return;
            }

            $scope.UploadInputs.push({
                Name: 'upload-media-file' + ($scope.UploadInputs.length + 1).toString(),
                File: '',
                Size: '0',
                LastModified: ''
            });

            $scope.ValidateSelectedMediaFiles();
            $scope.$apply();
        };

        $scope.onUploadAction = function() {
            var arrUploadFiles = [];
            for (var i = 0; i < $scope.UploadInputs.length; i++) {
                var files = $("input[name='" + $scope.UploadInputs[i].Name + "']")[0].files;
                if (!files || files.length == 0)
                    continue;
                arrUploadFiles.push(files[0]);
            }

            if (arrUploadFiles.length == 0)
                return;

            $scope.Status.IsBusy = true;
            $scope.Status.UploadInfo.uploadIdx = 0;
            $scope.Status.UploadInfo.percentage = 0;
            $scope.Status.UploadInfo.arrFiles = arrUploadFiles;

            $scope.Warning.IsActive = true;
            $scope.Warning.Text = "Upload is in progress, don't reload page until upload is completed!";

            var deferredOverall = $q.defer();

            var fnSuccess = function(data) {
                var uploadInfo = $scope.Status.UploadInfo;
                if (uploadInfo.uploadIdx < uploadInfo.arrFiles.length - 1) {
                    uploadInfo.uploadIdx++;
                    uploadInfo.percentage = 0;

                    // Start uploading next media file.
                    uploadService.uploadMediaFile(arrUploadFiles[$scope.Status.UploadInfo.uploadIdx])
                        .then(fnSuccess, fnError, fnNotify);
                } else {
                    // Media upload is completed
                    deferredOverall.resolve();
                }
            };

            var fnError = function(header) {
                // stop all upload.
                deferredOverall.reject(header);
            };
            var fnNotify = function(percentage) {
                // Update progress bar percentage.
                $scope.Status.UploadInfo.percentage = percentage.toFixed(2);
            };

            uploadService.uploadMediaFile(arrUploadFiles[$scope.Status.UploadInfo.uploadIdx])
                .then(fnSuccess, fnError, fnNotify);

            deferredOverall.promise.then(function() {
                // Upload completed successfully.
                $scope.Status.IsBusy = false;
                $scope.Status.UploadInfo = {};
                $scope.Warning.IsActive = false;
                $scope.clearInputMediaFiles();

                // Goto upload complete page.
                $location.path("/upload/complete");

            }, function(header) {
                // Upload process has failed.
                $scope.Status.IsBusy = false;
                $scope.Status.UploadInfo = {};
                $scope.Warning.IsActive = false;
                $scope.clearInputMediaFiles();
            });
        };
    }]);