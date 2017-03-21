using System;
using UnityEngine;

public class HelperFunctions {

    // The earth's mean radius in meters
    //const double EARTH_MEAN_RADIUS = 6371001;
	private const double EarthMeanRadius = 6361181;

    // The haversine formula calculates the distance between two gps locations by air (ignoring altitude).
    // Parameters:
    //      GPSLocation startLocation   -> The location where we are
    //      GPSLocation endLocation     -> The location where the object is
    // Returns the distance between the startLocation and endLocation in meters (1 Unit = 1 meter for simplicity)
    public static double Haversine(GpsManager.GpsLocation startLocation, GpsManager.GpsLocation endLocation) {
        double dLat = (endLocation.Latitude - startLocation.Latitude) * Math.PI / 180;
        double dLon = (endLocation.Longitude - startLocation.Longitude) * Math.PI / 180;
        startLocation.Latitude *= Math.PI / 180;
        endLocation.Latitude *= Math.PI / 180;
        // a = Sin(dLat/2)^2 + Sin(dLon/2)^2 * Cos(sLat) * Cos(eLat)
        double a = Math.Pow(Math.Sin(dLat / 2), 2)
            + Math.Pow(Math.Sin(dLon / 2), 2)
            * Math.Cos(startLocation.Latitude)
            * Math.Cos(endLocation.Latitude);
        double c = 2 * Math.Asin(Math.Sqrt(a));
        double d = EarthMeanRadius * 2 * c;
        return d;
    }

    // The formula that calculates the bearing when travelling from startLocation to endLocation
    // Parameters:
    //      GPSLocation startLocation -> The location where we are
    //      GPSLocation endLocation     -> The location where the object is
    // Returns the bearing from startLocation to endLocation in radians
    public static double CalculateBearing(GpsManager.GpsLocation startLocation, GpsManager.GpsLocation endLocation) {
        double x = Math.Cos(startLocation.Latitude * Math.PI / 180)
            * Math.Sin(endLocation.Latitude * Math.PI / 180)
            - Math.Sin(startLocation.Latitude * Math.PI / 180)
            * Math.Cos(endLocation.Latitude * Math.PI / 180)
            * Math.Cos((endLocation.Longitude - startLocation.Longitude) * Math.PI / 180);
        double y = Math.Sin((endLocation.Longitude - startLocation.Longitude) * Math.PI / 180)
            * Math.Cos(endLocation.Latitude * Math.PI / 180);
        return Math.Atan2(y, x) + Math.PI / 2;
    }

    public static Vector3 GetPositionFromCoords(GpsManager.GpsLocation fromCoords, GpsManager.GpsLocation toCoords) {
        // Calculate the distance and bearing between us and the location
        double distance = Haversine(fromCoords, toCoords);
        double bearing = CalculateBearing(fromCoords, toCoords);

        //Debug.Log("Distance: " + distance + "\nBearing: " + bearing);
        // calculate the x and z offset between us and the location and update the x and z position
        float xPos = (float)(-Math.Cos(bearing) * distance);
        float zPos = (float)(Math.Sin(bearing) * distance);

        return new Vector3(xPos, 0, zPos);
    }

    public static Vector3 GetPositionFromCoords(GpsManager.GpsLocation toCoords) {
        return GetPositionFromCoords(GpsManager.MyLocation, toCoords);
    }
}
