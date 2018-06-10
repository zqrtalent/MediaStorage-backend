angular.module('libraryApp')
.factory('uploadService', ['$http', '$q', function ($http, $q) {

    return {
        completeUpload: completeUpload,
        getUploadSyncInfo: getUploadSyncInfo,
        uploadMediaFile: uploadMediaFile
    };

    function completeUpload(uploadSyncInfo) {
        var deferred = $q.defer();
        $http({
            method: 'POST',
            url: '/library/media/upload/complete',
            data: uploadSyncInfo,
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function (data) {
            deferred.resolve(data);
        }, function (header) {
            deferred.reject(header);
        });

        return deferred.promise;
    };

    function getUploadSyncInfo() {
        var deferred = $q.defer();
        $http({
            method: 'POST',
            url: '/library/media/upload/sync',
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function (data) {
            deferred.resolve(data);
        }, function (header) {
            deferred.reject(header);
        });

        return deferred.promise;
    };

    function uploadMediaFile(mediaFile, defer) {
        var deferred = $q.defer();
        var formData = new FormData();
        formData.append('media', mediaFile);

        $http({
            method: 'POST',
            url: '/library/media/upload',
            data: formData,
            headers: {
                'Content-Type': undefined
            },
            uploadEventHandlers: {
                progress: function (e) {
                    deferred.notify(e.loaded * 100 / e.total);
                }
            }
        }).then(function (data) {
            deferred.resolve(data);
        }, function (header) {
            deferred.reject(header);
        });

        return deferred.promise;
    };
}]);