'use strict';
var apiURL = "https://www.vegvesen.no/nvdb/api/v2/";

// Main function that fetches the objects from the database
// Parameters:
//	object - The object you want from the database. Helper functions listed below fill this out
//	objectType - The type of object you want from the database given by a number. Default: no type
//	objectId - The id of the objectType you want from the database given by a number. Default: no id
// Returns the data in json format
function getObjects (latitude, longitude, object, objectType, objectId) {
	var url = apiURL + object;
	if (objectType != undefined ) {
		url += ("/" + objectType);
		if (objectId != undefined)
			url += ("/" + objectId);
	}
	var latLons = getAreaAroundPosition(latitude, longitude);
	url += "?inkluder=geometri&srid=4326&kartutsnitt=" + latLons[0] + "," + latLons[1] + "," + latLons[2] + "," + latLons[3];

	console.log("url: ", url);
	return fetch(url, {
		method: "GET",
		headers: {
			"Accept": "application/vnd.vegvesen.nvdb-v2+json"
		}
	})
	.then((response) => response.json());
}

function getAreaAroundPosition(latitude, longitude) {
	var lonMin, latMin, lonMax, latMax;
	lonMin = longitude + 0.0005;
	latMin = latitude - 0.0005;
	lonMax = longitude - 0.0005;
	latMax = latitude + 0.0005;
	return [lonMin, latMin, lonMax, latMax];
}

var api = {
	// Helper functions to fetch various objects from the database
	// Parameters:
	//	objectType - The type of object you want from the database given by a number. Default: no type
	//	objectId - The id of the objectType you want from the database given by a number. Default: no id
	getRoadObjects(latitude, longitude, objectType, objectId) {
		return getObjects(latitude, longitude, "vegobjekter", objectType, objectId);
	},
	getRoadObjectsTypes(latitude, longitude, objectType, objectId) {
		return getObjects(latitude, longitude, "vegobjekttyper", objectType, objectId);
	},
	getRoadNetwork(latitude, longitude, objectType, objectId) {
		return getObjects(latitude, longitude, "vegnett", objectType, objectId);
	},
	getArea(latitude, longitude, objectType, objectId) {
		return getObjects(latitude, longitude, "omrader", objectType, objectId);
	},
	// TODO
	// Most likely useless, but leaving it here in case we need it.
	// Also, probably not working as the API has some specific requirements for
	// getting this
	// getClosestPosition(latitude, longitude, objectType, objectId) {
	// 	return getObjects(latitude, longitude, "posisjon", objectType, objectId);
	// },
	getRoad(latitude, longitude, objectType, objectId) {
		return getObjects(latitude, longitude, "veg", objectType, objectId);
	},
	getStatus(latitude, longitude, objectType, objectId) {
		return getObjects(latitude, longitude, "status", objectType, objectId);
	}
};

module.exports = api;
