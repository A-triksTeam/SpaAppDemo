The scripts of any OOTB widget that relies on Javascript need to be overwritten as Angular services. The Javascript code is the same, but it is wrapped in a setTimeout function.
This causes the script to be executed after the DOM is being loaded. This is a MUST due to the dynamic async loading of the page contents and markup. 

Each of the services are passed to the MainController constructor and then used on the appropriate places.

Possible optimizations:
Currently the 3 files are loaded from the layout master page. This means that they exist on pages where they are not needed. 
The files can be loaded (client side) only on the pages they need to be present. Check Lazy loading with Angular.