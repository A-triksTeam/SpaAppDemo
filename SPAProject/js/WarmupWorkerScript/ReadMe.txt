This is a warmup javascript worker. It is needed so that it calls all the pages (asynchronously on a separate thread). This is needed so that after the first page call,
all the markup from the other pages is cached on the client browser. This significantly speeds up the page delivery and rendering.

The worker is called from the layout master file:

~/ResourcePackages/Bootstrap/MVC/Views/Layouts/default.cshtml