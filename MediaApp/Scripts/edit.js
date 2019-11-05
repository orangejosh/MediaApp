
function addcast() {
    var castButton = document.getElementById("addCast");
    var index = document.getElementsByClassName("cast").length;

    var lineBreak = document.createElement("br");
    castButton.parentNode.insertBefore(lineBreak, castButton);

    var newInput = document.createElement("input");
    newInput.type = "text";
    newInput.className = "cast";
    newInput.id = "Cast_" + index + "__Name";
    newInput.name = "Cast[" + index + "].Name";
    castButton.parentNode.insertBefore(newInput, castButton);
}

function addgenre() {
    var genreButton = document.getElementById("addGenre");
    var index = document.getElementsByClassName("genre").length;

    var lineBreak = document.createElement("br");
    genreButton.parentNode.insertBefore(lineBreak, genreButton);

    var newInput = document.createElement("input");
    newInput.type = "text";
    newInput.className = "genre";
    newInput.id = "Genre_" + index + "_";
    newInput.name = "Genre[" + index + "]";
    genreButton.parentNode.insertBefore(newInput, genreButton);
}
