"use strict";

var ngatorControllers = angular.module("ngatorControllers",[]);

ngatorControllers.controller("RssSourcesController", [
    "$scope", "$http", "$controller", function ($scope, $http, $controller) {
        $scope.sources = [];
        $scope.newsList = [];
        $scope.modalScope = $scope.$new();
        $scope.modal = $controller("ModalController", {$scope:$scope.modalScope});

        var response = $http.get("/api/sources");
        response.success(function (data, status, headers, config) {

            data.sources.forEach(function(src) {
                $scope.sources.push(src.siteName);
            });
            
        }).error(function (data, status, headers, config) {
            console.log("Error occuted during sources loading:" + status);
        });

        $scope.loadButtonEnabled = true;
        $scope.selectedSources = {};
        $scope.selectedSources.selected = ["Lenta.ru"];

        $scope.maxSize = 10;
        $scope.totalItems = 375;
        $scope.currentPage = 1;

        $scope.sourcesRequest = {
            "page": $scope.currentPage,
            "sources": [],
            "refresh": true
        };

        $scope.setPage = function (pageNo) {
            $scope.currentPage = pageNo;
        };
        
        $scope.pageChanged = function () {
            $scope.sourcesRequest.page = $scope.currentPage;
            $scope.sourcesRequest.refresh = false;
            $scope.obtainArticles($scope.sourcesRequest);
        };

        $scope.accodionExpander = function (item) {
            var isOpened = false;
            Object.defineProperty(item, "IsOpened",
            {
                get: function () { return isOpened; },
                set: function (newValue) {
                    isOpened = newValue;
                    if (isOpened) {
                        console.log(item); // do something...
                        $http.get("/api/sources/article/?id=" + item.guid)
                            .success(function (data, status, headers, config) {
                                item.body.body = data.body;
                                item.body.hasPicture = data.hasPicture;
                                if (item.hasEnclosure || data.hasPicture) {
                                    var imgContainer = $("#" + item.guid + "-pic");
                                    imgContainer.empty();
                                    imgContainer.append("<img src='api/sources/picture/?Id=" + item.guid + "' class='article-picture' />");
                                }
                            }).error(function (data, status, headers, config) {

                            });
                    }
                }
            });
        }
        
        $scope.signalRNewsProvider = function (sourcesRequest) {
            var modalInstance = $scope.modal.open(sourcesRequest);
            modalInstance.result.then(function (result) {
                $scope.resultData = result;                

                if ($scope.resultData) {

                    $scope.loadButtonEnabled = true;
                    $scope.newsList = [];
                    $scope.totalItems = $scope.resultData.TotalArticlesCount;
                    $scope.resultData.Headers.forEach(function (header) {
                        var stringDate = new Date(Date.parse(header.PublishDate)).toLocaleString("ru-RU", { hour12: false });
                        $scope.newsList.push({
                            date: stringDate,
                            guid: header.Guid,
                            header: header.Title,
                            quote: header.Description,
                            link: header.Link,
                            source: header.Source,
                            hasLogo: header.HasLogo,
                            body: {
                                body: "",
                                hasPicture: false
                            },
                            hasEnclosure: header.HasEnclosure
                        });
                    });
                    $scope.newsList.forEach($scope.accodionExpander);
                }

            },
            function () { // Modal dismissed by Cancel                
                $scope.loadButtonEnabled = true;
            });
        };

        $scope.ajaxNewsProvider = function(sourcesRequest) {
            $http.post("/api/sources", sourcesRequest)
                .success(function (data, status, headers, config) {
                    
                    $scope.loadButtonEnabled = true;
                    $scope.newsList = [];
                    $scope.totalItems = data.totalArticlesCount;
                    data.headers.forEach(function (header) {
                        var stringDate = new Date(Date.parse(header.publishDate)).toLocaleString("ru-RU", { hour12: false });
                        $scope.newsList.push({
                            date: stringDate,
                            guid: header.guid,
                            header: header.title,
                            quote: header.description,
                            link: header.link,
                            source: header.source,
                            hasLogo: header.hasLogo,
                            body: {
                                body: "",
                                hasPicture: false
                            },
                            hasEnclosure: header.hasEnclosure
                        });
                    });
                    $scope.newsList.forEach($scope.accodionExpander);
                })
                .error(function (data, status, headers, config) {

                });
        };


        $scope.obtainArticles = $scope.signalRNewsProvider;
                        

        $scope.getNews = function() {
            console.log($scope.selectedSources.selected.length);
            $scope.sourcesRequest.sources = [];
            $scope.selectedSources.selected.forEach(function(selectedSite) {
                $scope.sourcesRequest.sources.push({ sitename: selectedSite });
            });
            
            $scope.loadButtonEnabled = false;

            $scope.sourcesRequest.page = $scope.currentPage;
            $scope.sourcesRequest.refresh = true;
            $scope.obtainArticles($scope.sourcesRequest);
        };        
    }
]);


angular.module("ngatorControllers").controller("ModalController", function ($scope, $modal, $log, $document) {    
    $scope.resultData = {};            
    $scope.open = function(request) {
        $scope.request = request;
        $scope.modalInstance = $modal.open({
            animation: true,
            templateUrl: 'modalProgress.html',
            controller: 'ModalInstanceController',            
            resolve: {
                request: function() {
                    return $scope.request;
                }
            }
        });
        return $scope.modalInstance;
    }    
    this.open = function(request) {
        var instance = $scope.open(request);
        return instance;
    };
    this.ok = function() {
        
    };
});


angular.module('ngatorControllers').controller('ModalInstanceController', function ($scope, $modalInstance, $document, request) {
    
    $scope.args = request;
    $scope.result = {};
    $scope.currentProgress = 0;
    
    $scope.fooBar = true;

    $scope.ok = function () {
        $modalInstance.close($scope.result);
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };    

    $scope.setProgress = function(progress) {
        $scope.currentProgress = progress;
        return $scope;
    };

    var hub = $.connection.newsHub;
    $.connection.hub.start().done(function () {
        hub.server.loadNews($scope.args)
            .progress(function (percent) {                
                $scope.setProgress(percent).$apply();                
            })
            .done(function (result) {
                $scope.result = result;                
                $document[0].body.classList.remove('modal-open');
                angular.element($document[0].getElementsByClassName('modal-backdrop')).remove();
                angular.element($document[0].getElementsByClassName('modal')).remove();
                $modalInstance.close($scope.result);
            })
            .fail(function (error) {
                console.log("Error occured: " + error);
            });
    });    
});