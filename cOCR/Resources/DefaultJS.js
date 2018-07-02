
function appendDescriptionText(jsonNode, text) {
    var description = document.createElement("div");
    description.id = "description";
    var node = document.createElement("span");
    // Bacause `responses[0].textAnnotations[0].description` contains break lines as `\n`,
    // it should be replaced by `<br>` here for using as a html string.
    node.innerHTML = text.replace(/\n/g, "<br>");
    description.appendChild(node);
    jsonNode.parentNode.insertBefore(description, jsonNode);
}

function prepareOverlayLayer(imageNode) {
    var overlay = document.createElement("div"); 
    overlay.id = "overlay";
    var img = imageNode.querySelector("img");
    imageNode.parentNode.insertBefore(overlay, imageNode);
    overlay.appendChild(img.cloneNode());
    return overlay;
}

function appendLayer(parentNode, className, rect) {
    var layer = document.createElement("div"); 
    layer.className = className;
    layer.style.left = rect.x;
    layer.style.top = rect.y;
    layer.style.width = layer.style.minWidth = layer.style.maxWidth = rect.w;
    layer.style.height = layer.style.minHeight = layer.style.maxHeight = rect.h;
    parentNode.appendChild(layer);
    return layer;
}

function appendOverlayText(overlayNode, rect, text) {
	var container = appendLayer(overlayNode, "container", rect);
    var node = document.createElement("span");
    node.textContent = text;
    container.appendChild(node);
}

function verticesToRect(v) {
	var r = {};
	r.x = Math.min(v[0].x, v[3].x);
    r.y = Math.min(v[0].y, v[1].y);
    r.w = Math.max(v[1].x, v[2].x) - r.x;
    r.h = Math.max(v[2].y, v[3].y) - r.y;
    return r;
}

// Calculate relative position and size from which of absolute.
function relativize(iw, ih, r) {
	var rr = {};
    rr.x = (r.x / iw * 100) + "%";
    rr.y = (r.y / ih * 100) + "%";
    rr.w = (r.w / iw * 100) + "%";
    rr.h = (r.h / ih * 100) + "%";
    return rr;
}

document.addEventListener("DOMContentLoaded", function(event) {
	var imageNode = document.querySelector("#image");
	var jsonNode = document.querySelector("#json");
	
    var jsonText = jsonNode.textContent;
    var json = JSON.parse(jsonText);
    
    var descriptionText = json.responses[0].textAnnotations[0].description;
    appendDescriptionText(jsonNode, descriptionText);
    
    // Prepare overlay layer for OCR result overlay feature.
    var overlayNode = prepareOverlayLayer(imageNode);
    
    // The real size of original image which applied for OCR.
    var image = imageNode.querySelector("img");
    var iw = image.naturalWidth;
    var ih = image.naturalHeight;
    
    var annotation = json.responses[0].fullTextAnnotation;
	annotation.pages.forEach(function(page) {
		page.blocks.forEach(function(block) {
			var r = verticesToRect(block.boundingBox.vertices);
	        var rr = relativize(iw, ih, r);
	        var blockNode = appendLayer(overlayNode, "block", rr);
			block.paragraphs.forEach(function(paragraph) {
				var r = verticesToRect(paragraph.boundingBox.vertices);
		        var rr = relativize(iw, ih, r);
		        var paragraphNode = appendLayer(overlayNode, "paragraph", rr);
				paragraph.words.forEach(function(word) {
					var r = verticesToRect(word.boundingBox.vertices);
			        var rr = relativize(iw, ih, r);
			        var wordNode = appendLayer(overlayNode, "word", rr);
					word.symbols.forEach(function(symbol) {
				        var r = verticesToRect(symbol.boundingBox.vertices);
				        var rr = relativize(iw, ih, r);
				        var text = symbol.text;
				        if ("property" in symbol && "detectedBreak" in symbol.property) {
				        	switch (symbol.property.detectedBreak.type) {
				        		case "LINE_BREAK":
				        			text = text + "\r\n";
				        			break;
				        		/*
				        		case "SPACE":
				       			case "SURE_SPACE":
				        		case "EOL_SURE_SPACE":
				        		case "HYPHEN":
				        		case "UNKNOWN":
				        		 */
				        		default:
				        			text = text + " ";
				        			break;
				        	}
				        	console.log(symbol.property.detectedBreak);
				    	}
				        appendOverlayText(overlayNode, rr, text);
					});
				});
			});
		});
	});
    
	/*
	// Old format.
    var annotations = json.responses[0].textAnnotations.slice(1);
    annotations.forEach(function(annotation) {
        var r = verticesToRect(annotation.boundingPoly.vertices);
        var rr = relativize(iw, ih, r);
        appendOverlayText(rr.x, rr.y, rr.w, rr.h, annotation.description);
    });
    */
});
