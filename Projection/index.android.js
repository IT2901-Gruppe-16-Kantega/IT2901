"use strict";

import React, { Component } from 'react';
import {
	AppRegistry,
	StyleSheet,
	Text,
	View,
	DeviceEventEmitter,
	Dimensions
} from 'react-native';

import {
	SensorManager
} from "NativeModules";

import Parse from "wellknown";
import KalmanFilter from "kalmanjs";

import api from "./modules/nvdb"

var kalmanFilter_Screen_X = new KalmanFilter({R: 0.01, Q: 3});
var kalmanFilter_Heading = new KalmanFilter({R: 0.1, Q: 10});
var screen_X_data = [];
var screen_x_kalman_filter = [];
var heading_data = [];
var heading_kalman_filter = [];
const DATA_MAX_LENGTH = 512;

export default class Projection extends Component {
	constructor(props) {
		super(props);
		this.state = {
			latitude: 0.0,
			longitude: 0.0,
			heading: 0,
			magnetometerData: {
				x: 0,
				y: 0,
				z: 0
			},
			accelerometerData: {
				x: 0,
				y: 0,
				z: 0},
				orientationData: {
					azimuth: 0,
					pitch: 0,
					roll: 0},
					gyroData: {
						x: 0,
						y: 0,
						z: 0
					},
					data: [],
					distance: 0,
					xy: {
						left: 0,
						bottom: 0
					}
				}

				this.readGyro = this.readGyro.bind(this);
				this.haversine = this.haversine.bind(this);
				this.readMagnetometer = this.readMagnetometer.bind(this);
				this.readAccelerometer = this.readAccelerometer.bind(this);
				this.readOrientation = this.readOrientation.bind(this);
			}

			// Our watch id
			watchID: ?number = null;

			// When the component loads
			componentDidMount() {
				// Get current position
				navigator.geolocation.getCurrentPosition(
					// Use that position and set our initial position to that
					(position) => {
						this.setState({
							latitude: position.coords.latitude,
							longitude: position.coords.longitude,
							heading: position.coords.heading,
						});

						api.getRoadObjects(position.coords.latitude, position.coords.longitude, 96)
						.then((response) => {
							this.setState({
								data: response.objekter
							});
						});
						// // console.log(JSON.stringify(position.coords, null, 2));
					},
					// If there is an error, make an alert
					(error) => alert(JSON.stringify(error)),
					{enableHighAccuracy: true, timeout: 20000, maximumAge: 100}
				);
				// Watch our currrent position and set lastPosition to that
				// using watchID to know which particular handler is watching
				this.watchID = navigator.geolocation.watchPosition((position) => {
					this.setState({
						latitude: position.coords.latitude,
						longitude: position.coords.longitude,
						heading: position.coords.heading,
					});

					api.getRoadObjects(position.coords.latitude, position.coords.longitude, 96)
					.then((response) => {
						this.setState({
							data: response.objekter
						});
						// console.log(JSON.stringify(response.objekter[0], null, 2));
					});
					// console.log(JSON.stringify(position.coords, null, 2));
				},
				// If there is an error, make an alert
				(error) => alert(JSON.stringify(error)),
				{enableHighAccuracy: true, timeout: 20000, maximumAge: 1000}
			);

			// this.readGyro();
			// this.readAccelerometer();
			// this.readMagnetometer();
			this.readOrientation();
		}

		// When the component unloads, stop watching our position
		componentWillUnmount() {
			navigator.geolocation.clearWatch(this.watchID);
			ReactNativeHeading.stop();
			DeviceEventEmitter.removeAllListeners();
		}

		readGyro() {
			var _this = this;
			SensorManager.startGyroscope(500);

			DeviceEventEmitter.addListener('Gyroscope', function(data) {
				_this.setState({
					gyroData: data
				});
			});
		}

		readMagnetometer() {
			var _this = this;
			// Start to read magnetometer data with 100 ms delays
			SensorManager.startMagnetometer(100);
			// Whenever the data changes, update magX, magY and magZ
			DeviceEventEmitter.addListener('Magnetometer', function (data) {
				_this.setState({
					magnetometerData: data
				});
			});
		}

		readAccelerometer() {
			var _this = this;
			SensorManager.startAccelerometer(100); // To start the accelerometer with a minimum delay of 100ms between events.
			DeviceEventEmitter.addListener('Accelerometer', function (data) {
				_this.setState({
					accelerometerData: data
				});
			});
		}

		readOrientation() {
			var _this = this;
			SensorManager.startOrientation(100);
			DeviceEventEmitter.addListener('Orientation', function (data) {
				if(heading_data.push(data.roll) > DATA_MAX_LENGTH) {
					heading_data.shift();
				}

				heading_kalman_filter = heading_data.map(function(v) {
					return kalmanFilter_Heading.filter(v);
				});

				_this.setState({
					orientationData: data,
					heading: heading_kalman_filter[heading_kalman_filter.length - 1],
				});
				if(_this.state.data.length > 0) {
					var wkt = Parse(_this.state.data[0].geometri.wkt);
					var xy = _this.relationToNorth(wkt.coordinates[0], wkt.coordinates[1]);
					_this.setState({
						xy: xy
					});

					// console.log(xy);
				}
			});
		}

		// haversine formula takes in the latitude and longitude of the other object
		// Returns distance from your current position to objects position in meters
		// Divide by 1000 for kilometers
		haversine(lat2, lon2){
			var _that = this;
			var R, phi1, phi2, deltaPhi, deltaLambda, a, c, d;
			R = 6371e3;
			// Convert lat and lon to radians
			phi1 = _that.state.latitude * (Math.PI / 180);
			phi2 = lat2 * (Math.PI / 180);
			deltaPhi = (lat2 - _that.state.latitude) * (Math.PI / 180);
			deltaLambda = (lon2 - _that.state.longitude) * (Math.PI / 180);
			a = Math.sin(deltaPhi / 2) * Math.sin(deltaPhi / 2) +
			Math.cos(phi1) * Math.cos(phi2) *
			Math.sin(deltaLambda / 2) * Math.sin(deltaLambda / 2);
			c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
			d = R * c;
			return d;
		}

		relationToNorth(lat2, lon2) {
			var _this = this;
			var lat1 = this.state.latitude * (Math.PI / 180);
			var lon1 = this.state.longitude * (Math.PI / 180);
			lat2 *=  (Math.PI / 180);
			lon2 *= (Math.PI / 180)
			var dLat = (lat2 - lat1);
			var dLon = (lon2 - lon1);
			var y = Math.sin(dLon) * Math.cos(lat2);
			var x = Math.cos(lat1) * Math.sin(lat2) - Math.sin(lat1) * Math.cos(lat2) * Math.cos(dLon);
			var angle = Math.atan2(y, x);
			var headingDeg = this.state.heading;
			var angleDeg = angle * 180 / Math.PI; // To 360 degrees
			var heading = headingDeg * Math.PI / 180; // To radians
			angle = ((angleDeg + 360) % 360) * Math.PI / 180; // Normalize to 0 to 360 (instead of -180 to 180), then convert back into radians)
			angleDeg = angle * 180 / Math.PI;
			var distance = this.haversine(lat2, lon2);
			x = Math.sin(angle - heading) * distance;
			y = Math.cos(angle - heading) * distance;
			var screenX = (x * 256) / distance;
			var screenY = (y * 256) / distance

			if(screen_X_data.push(screenX) > DATA_MAX_LENGTH) {
				screen_X_data.shift();
			}

			screen_x_kalman_filter = screen_X_data.map(function(v) {
				return kalmanFilter_Screen_X.filter(v);
			});
			console.log(screen_x_kalman_filter[screen_x_kalman_filter.length - 1]);

			return {
				right: screen_x_kalman_filter[screen_x_kalman_filter.length - 1],
			};
		}

		render() {
			return (
				<View style={ styles.container }>
					<Text style={ styles.welcome }>
						Welcome to React Native!
					</Text>
					<Text style={ styles.instructions }>
						Hallo please
					</Text>
					<View>
						<Text style={ this.state.xy }>
							boop
						</Text>
					</View>
				</View>
			);
		}
	}

	const styles = StyleSheet.create({
		container: {
			flex: 1,
			justifyContent: 'center',
			alignItems: 'center',
			backgroundColor: '#F5FCFF',
		},
		welcome: {
			fontSize: 20,
			textAlign: 'center',
			margin: 10,
		},
		instructions: {
			color: '#333333',
			marginBottom: 5,
		},
	});

	AppRegistry.registerComponent('Projection', () => Projection);
