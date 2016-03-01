//This script does not support JS objects like window, document etc.
//The _isRan property prevents the script execution on each page switch from the navigation. However ctrl+F5 (clearing the cache) triggers it again.

var _isRan = null;

self.addEventListener("message", function (e) {
    if (get_isRan() == null || get_isRan() == false) {
        var boolArray = [];
        
        for (var i = 0; i < e.data.length; i++) {
            var xhr = new XMLHttpRequest();

            xhr.onreadystatechange = function () {
                if (this.readyState === 4 && this.status === 200) {
                    boolArray.push(true);
                    //When postMessage is called the onmessage handler logic is executed to increase the progress bar value.
                    postMessage(boolArray); // Another callback here
                }
            };
           
            xhr.open('GET', [location.protocol, '//', location.host, e.data[i]].join(''), true);
            xhr.send('');
        }
        set_isRan(true);
    }

}, false);

function get_isRan() {
    return _isRan;
}

function set_isRan(value) {
    _isRan = value;
}