//Configures Angular's SPA url routing.
//In order to be able to inject dynamic html a template with ng-bind-html directive is used. 
//All the html that is parsed from a page in Sitefinity is injected into the element wiht ng-bind-html directive through the MainController.
//This templated on the other hand is injected into the ng-view of Angular. (the ng-view is in a Feather layout widget ~/ResourcePackages/Bootstrap/GridSystem/Templates/angularHolder.html)

myApp.config(['$routeProvider', '$locationProvider', function ($routeProvider, $locationProvider) {
    var template = '<div ng-if="rawHtml" ng-bind-html="rawHtml" class="toggle"></div>';
    var mainController = 'MainController';
    $routeProvider
    .when('/', {
        template: template,
        controller: mainController
    })
    .when('/:group', {
        template: template,
        controller: mainController
    })
    .when('/:group/:page', {
        template: template,
        controller: mainController
    }).when('/:group/:name*', {
        template: template,
        controller: mainController
    })
    .when('/:group/:page/:name*', {
        template: template,
        controller: mainController
    });

    //Needed so that angular does not put hash (#) in urls but keep them clean.
    $locationProvider.html5Mode({
        enabled: true,
        requireBase: false
    });
}]);
