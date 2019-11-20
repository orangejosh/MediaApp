
// Read the list of people from peopleList.txt
function readFile(){
	const fs = require('fs')
	fs.readFile('peopleList.txt', 'utf-8', (err, data) => {
		if (err) throw err;
		var people = data.split("\n");
		for (var i in people){
			loadFirst(people[i]);
		}
	}) 
}

// Download the seach page from IMDb for the given actor.
function loadFirst(person){
	var first = person.substring(0, person.indexOf(' '));
	var last = person.substring(person.indexOf(' ')+1);

	var XMLHTTPRequest = require("xmlhttprequest").XMLHttpRequest;
	var xhttp = new XMLHTTPRequest();
	xhttp.onreadystatechange = function () {
		if (this.readyState == 4 && this.status == 200){
			parseFirst(this.responseText, person);
		}
	};
	xhttp.open("GET", "https://www.imdb.com/find?ref_=nv_sr_fn&q=" + first + "+" + last + "&s=all", false);
	xhttp.send();
}

// Download the first url from the previously downloaded search page.
function loadSecond(url, person){
	var first = person.substring(0, person.indexOf(' '));
	var last = person.substring(person.indexOf(' ')+1);

	var XMLHTTPRequest = require("xmlhttprequest").XMLHttpRequest;
	var xhttp = new XMLHTTPRequest();
	xhttp.onreadystatechange = function () {
		if (this.readyState == 4 && this.status == 200){
			var url = parseSecond(this.responseText);
			console.log(first + " " + last + ";" + url);
		}
	};
	xhttp.open("GET", url, false);
	xhttp.send();
}

// Parse the search page for the download link to the actors page.
function parseFirst(data, person){
	try{
		var regEx = /result_text"> <a href=["\/a-z 0-9 ?_=]+/g;
		var match = regEx.exec(data);

		regEx = /=".+"/g;
		match = regEx.exec(match[0]);

		regEx = /".+"/g;
		match = regEx.exec(match[0]);

		var url = match[0].substring(1, match[0].length - 1);
		url = "https://www.imdb.com" + url;

		loadSecond(url, person);
	} catch(err){
		return "NULL"
	}
}

// Parse the actors page for the image link.
function parseSecond(data){
	try{
		var regEx = /name-poster[" \s a-z A-Z 0-9 = : \/ \. \- _ , @]+/g
		var match = regEx.exec(data);

		regEx = /https:[\/ a-z A-Z 0-9 \. \- _ , @]+/g
		match = regEx.exec(match[0]);
		return match[0];
	} catch(err){
		return "NULL"
	}
}

readFile();
