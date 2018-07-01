document.addEventListener("DOMContentLoaded", function(event) {
   
    var json_text = document.querySelector("#json").textContent;
    var json = JSON.parse(json_text);
    var annotations = json.responses[0].textAnnotations.slice(1);
    
    var overlay = document.createElement("div"); 
    overlay.id = "overlay";
    
    function appendOverlayText(x, y, w, h, text) {
        var container = document.createElement("div");
        container.className = "container";
        container.style.left = x;
        container.style.top = y;
        container.style.width = container.style.min_width = container.style.max_width = w;
        container.style.height = container.style.min_height = container.style.max_height = h;
        var node = document.createElement("span");
        node.textContent = text + " ";
        container.appendChild(node);
        overlay.appendChild(container);
        
        var image = document.querySelector("#image"); 
        var img = image.querySelector("img");
        overlay.style.width = img.width;
        overlay.style.height = img.height;
        image.parentNode.insertBefore(overlay, image);
    }
    
    annotations.forEach(function(elm) {
        var v = elm.boundingPoly.vertices;
        var x = Math.min(v[0].x, v[3].x);
        var y = Math.min(v[0].y, v[1].y);
        var w = Math.max(v[1].x, v[2].x) - x;
        var h = Math.max(v[2].y, v[3].y) - y;
        appendOverlayText(x, y, w, h, elm.description);
    });
});
