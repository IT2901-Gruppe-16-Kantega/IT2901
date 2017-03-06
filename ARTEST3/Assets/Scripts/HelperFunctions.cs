using System;
using UnityEngine;

public class HelperFunctions {

    // The earth's mean radius in meters
    //const double EARTH_MEAN_RADIUS = 6371001;
    const double EARTH_MEAN_RADIUS = 6361181;

    // The haversine formula calculates the distance between two gps locations by air (ignoring altitude).
    // Parameters:
    //      GPSLocation startLocation   -> The location where we are
    //      GPSLocation endLocation     -> The location where the object is
    // Returns the distance between the startLocation and endLocation in meters (1 Unit = 1 meter for simplicity)
    public static double Haversine(GPSManager.GPSLocation startLocation, GPSManager.GPSLocation endLocation) {
        double dLat = (endLocation.latitude - startLocation.latitude) * System.Math.PI / 180;
        double dLon = (endLocation.longitude - startLocation.longitude) * System.Math.PI / 180;
        startLocation.latitude *= System.Math.PI / 180;
        endLocation.latitude *= System.Math.PI / 180;
        // a = Sin(dLat/2)^2 + Sin(dLon/2)^2 * Cos(sLat) * Cos(eLat)
        double a = System.Math.Pow(System.Math.Sin(dLat / 2), 2)
            + System.Math.Pow(System.Math.Sin(dLon / 2), 2)
            * System.Math.Cos(startLocation.latitude)
            * System.Math.Cos(endLocation.latitude);
        double c = 2 * System.Math.Asin(System.Math.Sqrt(a));
        double d = EARTH_MEAN_RADIUS * 2 * c;
        return d;
    }

    // The formula that calculates the bearing when travelling from startLocation to endLocation
    // Parameters:
    //      GPSLocation startLocation -> The location where we are
    //      GPSLocation endLocation     -> The location where the object is
    // Returns the bearing from startLocation to endLocation in radians
    public static double CalculateBearing(GPSManager.GPSLocation startLocation, GPSManager.GPSLocation endLocation) {
        double x = System.Math.Cos(startLocation.latitude * System.Math.PI / 180)
            * System.Math.Sin(endLocation.latitude * System.Math.PI / 180)
            - System.Math.Sin(startLocation.latitude * System.Math.PI / 180)
            * System.Math.Cos(endLocation.latitude * System.Math.PI / 180)
            * System.Math.Cos((endLocation.longitude - startLocation.longitude) * System.Math.PI / 180);
        double y = System.Math.Sin((endLocation.longitude - startLocation.longitude) * System.Math.PI / 180)
            * System.Math.Cos(endLocation.latitude * System.Math.PI / 180);
        return System.Math.Atan2(y, x) + System.Math.PI / 2;
    }

    public static Vector3 GetPositionFromCoords(GPSManager.GPSLocation fromCoords, GPSManager.GPSLocation toCoords) {
        // Calculate the distance and bearing between us and the location
        double distance = HelperFunctions.Haversine(fromCoords, toCoords);
        double bearing = HelperFunctions.CalculateBearing(fromCoords, toCoords);

        //Debug.Log("Distance: " + distance + "\nBearing: " + bearing);
        // calculate the x and z offset between us and the location and update the x and z position
        float xPos = (float)(-System.Math.Cos(bearing) * distance);
        float zPos = (float)(System.Math.Sin(bearing) * distance);

        return new Vector3(xPos, 0, zPos);
    }

    public static Vector3 GetPositionFromCoords(GPSManager.GPSLocation toCoords) {
        return GetPositionFromCoords(GPSManager.myLocation, toCoords);
    }
}
