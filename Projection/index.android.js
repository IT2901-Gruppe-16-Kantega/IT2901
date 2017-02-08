"use strict";

import React, { Component } from 'react';
import {
	AppRegistry,
	StyleSheet,
	Text,
	View,
	DeviceEventEmitter
} from 'react-native';

import {
	SensorManager,
	ReactNativeHeading
} from "NativeModules";
// import {
// 	ReactNativeHeading
// } from "react-native-heading";

export default class Projection extends Component {
	constructor(props) {
		super(props);
		this.state = {
			latitude: 0.0,
			longitude: 0.0,
			heading: 0,
			gyroX: 0,
			gyroY: 0,
			gyroZ: 0
		}

		this.readHeading = this.readHeading.bind(this);
		this.readGyro = this.readGyro.bind(this);
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
				});
			},
			// If there is an error, make an alert
			(error) => alert(JSON.stringify(error)),
			{enableHighAccuracy: true, timeout: 20000, maximumAge: 1000}
		);
		// Watch our currrent position and set lastPosition to that
		// using watchID to know which particular handler is watching
		this.watchID = navigator.geolocation.watchPosition((position) => {
			this.setState({
				latitude: position.coords.latitude,
				longitude: position.coords.longitude,
			});
		});

		//this.readHeading();
		this.readGyro();
	}

	// When the component unloads, stop watching our position
	componentWillUnmount() {
		navigator.geolocation.clearWatch(this.watchID);
		ReactNativeHeading.stop();
		DeviceEventEmitter.removeAllListeners('headingUpdated');
	}

	readHeading() {
		var _this = this;

		SensorManager.startOrientation(100);

		DeviceEventEmitter.addListener('Orientation', function(data) {
			_this.setState({
				heading: data.azimuth
			});
			console.log('New heading is:', data.heading);
		});
	}

	readGyro() {
		var _this = this;
		SensorManager.startGyroscope(100);

		DeviceEventEmitter.addListener('Gyroscope', function(data) {
			_this.setState({
				gyroX: data.x,
				gyroY: data.y,
				gyroZ: data.z,
			});
		});
	}

	render() {
		return (
			<View>
				<Text style={styles.welcome}>
					Welcome to React Native!
				</Text>
				<Text style={styles.instructions}>
					Hallo
				</Text>
				<Text style={styles.instructions}>
					{this.state.gyroX}
				</Text>
				<Text style={styles.instructions}>
					{this.state.gyroY}
				</Text>
				<Text style={styles.instructions}>
					{this.state.gyroZ}
				</Text>
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
