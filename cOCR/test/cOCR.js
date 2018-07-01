
function appendDescriptionText(json_node, text) {
    var description = document.createElement("div");
    description.id = "description";
    var node = document.createElement("span");
    node.innerHTML = text.replace("\r\n", "\n").replace("\n", "<br>");
    description.appendChild(node);
    
    json_node.parentNode.insertBefore(description, json_node);
}

function appendOverlayLayer(image_node) {
    var overlay = document.createElement("div"); 
    overlay.id = "overlay";
    var img = image_node.querySelector("img");
    image_node.parentNode.insertBefore(overlay, image_node);
    overlay.appendChild(img.cloneNode());
}

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
}

document.addEventListener("DOMContentLoaded", function(event) {
	var image_node = document.querySelector("#image");
	var json_node = document.querySelector("#json");
	
    var json_text = json_node.textContent;
    var json = JSON.parse(json_text);
    
    var description_text = json.responses[0].textAnnotations[0].description;
    appendDescriptionText(json_node, description_text);
    
    appendOverlayLayer(image_node);
    
    var image = image_node.querySelector("img");
    var iw = image.naturalWidth;
    var ih = image.naturalHeight;
    
    var annotations = json.responses[0].textAnnotations.slice(1);
    annotations.forEach(function(elm) {
        var v = elm.boundingPoly.vertices;
        var x = Math.min(v[0].x, v[3].x);
        var y = Math.min(v[0].y, v[1].y);
        var w = Math.max(v[1].x, v[2].x) - x;
        var h = Math.max(v[2].y, v[3].y) - y;
        var rx = (x / iw * 100) + "%";
        var ry = (y / ih * 100) + "%";
        var rw = (w / iw * 100) + "%";
        var rh = (h / ih * 100) + "%";
        appendOverlayText(rx, ry, rw, rh, elm.description);
    });
});
