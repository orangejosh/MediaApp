var movieList = [];
var page = process.argv.slice(2);

function loadDoc(page){
	var XMLHTTPRequest = require("xmlhttprequest").XMLHttpRequest;
	var xhttp = new XMLHTTPRequest();
	xhttp.onreadystatechange = function () {
		if (this.readyState == 4 && this.status == 200){
			parseDoc(this.responseText);
			printList();
		}
	};
	xhttp.open("GET", "https://www.imdb.com/search/title/?groups=top_1000&sort=user_rating,desc&start=" + page + "&ref_=adv_nxt", true);
	xhttp.send();
}

function parseDoc(data){
	var regEx = /loadlate=".+/g;
	while(match = regEx.exec(data)){
		var regEx2 = /".+"/g;
		match2 = regEx2.exec(match[0]);
		var src = match2[0].substring(1, match2[0].length - 1);

		movieList.push(src);
	}

	var i = 0;
	regEx = /<span class="lister-item-index unbold text-primary">\d+\.<\/span>\s+<a href="\/title.+\s+>.+<\/a>/g;
	while(match = regEx.exec(data)){
		var regEx2 = /<a.+\s*.+<\/a>/g
		match2 = regEx2.exec(match[0]);

		var regEx3 = />.*</g
		match3 = regEx3.exec(match2[0]);
		var title = match3[0].substring(1, match3[0].length - 1);

		movieList[i] += ";" + title;
		i++;
	}

	i = 0;
	regEx = /<p class="text-muted">\s+.+</g;
	while(match = regEx.exec(data)){
		var regEx2 = />\s+.+</g;
		match2 = regEx2.exec(match[0]);
		var synopsis = match2[0].substring(1, match2[0].length - 1);

		movieList[i] += ";" + synopsis.trim();
		i++;
	}

	i = 0;
	regEx = /<span class="lister-item-year text-muted unbold">.*</g;
	while(match = regEx.exec(data)){
		var regEx2 = /\(\d+\)/g;
		match = regEx2.exec(match[0]);
		var year = match[0].substring(1, match[0].length - 1);

		movieList[i] += ";" + year;
		i++;
	}

	i = 0;
	regEx = /Director:\s+.+\s+>.+</g;
	while(match = regEx.exec(data)){
		var regEx2 = />.*</g;
		match = regEx2.exec(match[0]);
		var director = match[0].substring(1, match[0].length - 1);

		movieList[i] += ";" + director;
		i++;
	}

	i = 0;
	regEx = /Stars:\s+.+\s+>.+\s+.+\s+.+\s+.+\s+.+\s+.+\s+.+/g;
	while(match = regEx.exec(data)){
		var regEx2 = />.*</g;
		var actors = [];
		while(match2 = regEx2.exec(match[0])){
			var actor = match2[0].substring(1, match2[0].length - 1);
			actors.push(actor);
		}
		var actorString = "";
		for (var j = 0; j < actors.length; j++){
			if (j == 0){
				actorString += actors[j];
			} else {
				actorString += ", " + actors[j];
			}
		}

		movieList[i] += ";" + actorString;
		i++;
	}

	i = 0;
	regEx = /genre".*\s+.+</g
	while(match = regEx.exec(data)){
		var regEx2 = /\s+.+\s/g
		match = regEx2.exec(match[0]);
		var genre = match[0].trim();

		movieList[i] += ";" + genre;
		i++;
	}	

	i = 0;
	regEx = /imdb-rating"><\/span>\s+<strong>.+</g;
	while(match = regEx.exec(data)){
		var regEx2 = /strong>.*</g;
		match = regEx2.exec(match[0]);
		var regEx3 = />.+</g;
		match = regEx3.exec(match[0]);
		var rating = match[0].substring(1, match[0].length - 1);

		movieList[i] += ";" + rating;
		i++;
	}
}

function printList(){
	for (var index in movieList){
		console.log(movieList[index] + "\n");
	}
}

loadDoc(page);
