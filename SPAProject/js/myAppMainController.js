//This is a standart Angular service. It is responsible for building up the requested page url and calling the page via http request asynchronosly. 
//The result (the page's html) is processed in the MainController.
//The service also handles 404 and 500 error pages.
myApp.service('asyncCall', function ($http, $location, $routeParams) {

    //Location of error pages need to be specified.
    var pageURL = "";
    var page404 = "/errorpages/404";
    var page500 = "/errorpages/500";

    //Builds the appropriate page url irrelevant of the environment (live, qa, dev or even cassini dev server).
    function buildPageUrl() {
        var currLocation = "";
        currLocation += $location.protocol();
        currLocation += '://';
        currLocation += $location.host();
        var port = $location.port();
        if (port !== '80') {
            currLocation += ':';
            currLocation += port;
        }
        currLocation += $location.path();
        return currLocation;
    }

    return {
        //Sends you to 404 error page.
        go404: function () {
            $location.path(page404);
        },

        //Sends you to 500 error page.
        go500: function () {
            $location.path(page500);
        },

        //Gets page via http request.
        getHtml: function () {
            return $http.get(buildPageUrl());
        }
    };
});

//Default Angular controller initialization.
myApp.controller('MainController', ['$scope', 'asyncCall', '$sce', '$routeParams', '$window', '$timeout', 'commentsCountScriptExec', 'commentsListScriptExec', 'overlayGalleryScriptExec',
    function ($scope, asyncCall, $sce, $routeParams, $window, $timeout, commentsCountScriptExec, commentsListScriptExec, overlayGalleryScriptExec) {

        //Calling the getHtml function of the asyncCall service and processing the response.
        asyncCall.getHtml().then(function (response) {

                //If response is OK we parse the html depending on the route parameters (defined in /js/myAppConfig.js).
                if (angular.isDefined($routeParams.group) &&
                  angular.isDefined($routeParams.page)) {
                    $scope.rawHtml = parseResponseHtml(response);
                } else {
                    if (angular.isUndefined($routeParams.page)) {
                        $scope.rawHtml = parseResponseHtml(response);
                    }
                }
                
                //Specific for this project. Running specific Sitefinity control's javascript depending on the page it is placed to.
                //The javascripts are called via timeouts so that the logic is executed after the DOM is being ready.
                //The scripts can be found in ~/js/OOTBScripts folder.
                var pageGroup = '/' + $routeParams.group;
                var pageName = '/' + $routeParams.page;
                
                //Script for the Picture gallery widget
                if (pageGroup === "/pictures" || pageName === "/pictures" || pageGroup === "/newsitefinitypagetest") {
                    overlayGalleryScriptExec.runTimedOut();
                }
                
                //Scripts for the comments widget (comments widgeet is added to news and blog posts widgets automatically).
                if (pageGroup === "/blog" || pageGroup === "/news" || pageGroup === "/sidenavsecond") {
                    commentsCountScriptExec.runTimedOut();
                    commentsListScriptExec.runTimedOut();
                }
            }
            , function (response) { //This is where the response is handled in case problems with the service call.
                if (response.status === 404) {
                    asyncCall.go404();
                    return;
                }
    
                if (response.status === 500) {
                    asyncCall.go500();
                    return;
                }
            });
        //Gets the service call's response (the page's html) and processes it.
        function parseResponseHtml(response) {
            var html = "";
            var parser = new DOMParser();
            var doc = parser.parseFromString(response.data, 'text/html');
    
            //looks better when targeting element by Id, but Sitefinity strips Ids from Layout controls for some reason.
            var internalMarkup = doc.getElementsByClassName('myholder');
    
            if (internalMarkup[0] !== null) {
                //set new page title here
                $window.document.title = doc.title;
                html = $sce.trustAsHtml(internalMarkup[0].innerHTML);
                return html;
            }
        };
    }
]);
