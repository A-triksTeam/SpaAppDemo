//Standart angular application initalization
//ngRoute is needed and used for the url routing that SPA needs.
//ngSanitize is used in the MainController to be able to work with the parsed page HTML.
//ngAnimate is just for looks. It makes the fade in/fade out transitions between the pages, but the SPA does not require it to function.

var myApp = angular.module('myApp', ['ngRoute', 'ngSanitize', 'ngAnimate']);

