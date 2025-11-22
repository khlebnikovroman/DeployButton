# Arduino Music Control System

This project provides a web-based administration panel for controlling music playback on an Arduino device. The system consists of an ASP.NET Core Web API backend and an Angular frontend.

## Components

1. **ASP.NET Core Web API**: Handles music file management, Arduino communication, and provides REST API endpoints
2. **Angular Frontend**: Web-based administration panel for controlling music playback
3. **Arduino Sketch**: Code for Arduino to receive commands and play music files from an SD card

## Prerequisites

- .NET 6 SDK
- Node.js and npm
- Arduino IDE (for uploading the sketch)
- Arduino board with SD card module and audio output capability
- Required Arduino libraries: SD, TMRpcm, SPI

## Setup Instructions

### 1. Backend Setup (ASP.NET Core API)

1. Navigate to the project directory
2. Install dependencies: `dotnet restore`
3. Build the project: `dotnet build`
4. Run the application: `dotnet run`

The API will be available at `http://localhost:5000`.

### 2. Frontend Setup (Angular)

1. Navigate to the `ClientApp` directory: `cd ClientApp`
2. Install dependencies: `npm install`
3. Start the development server: `npm start`
4. The Angular app will be available at `http://localhost:4200`

### 3. Arduino Setup

1. Install required libraries in Arduino IDE:
   - SD library
   - TMRpcm library
   - SPI library
2. Connect your Arduino to your computer via USB
3. Upload the `arduino_music_controller.ino` sketch to your Arduino
4. Connect an SD card module to your Arduino (CS pin 4 by default)
5. Connect audio output to pin 9 (or modify the sketch to use a different pin)

## API Endpoints

- `GET /api/music/tracks` - Get list of available tracks
- `POST /api/music/play` - Play a specific track (request body: `{ "trackName": "filename.mp3" }`)
- `POST /api/music/stop` - Stop current playback
- `GET /api/music/status` - Get current status (connection, playing state, current track)
- `POST /api/music/upload` - Upload a new MP3 file

## Usage

1. Start the ASP.NET Core backend
2. Start the Angular frontend
3. Open the web interface at `http://localhost:4200`
4. Upload MP3 files using the upload section
5. Select and play tracks using the interface
6. Monitor connection and playback status

## Arduino Connection

The system automatically detects available COM ports and attempts to connect to the Arduino. The connection status is displayed in the web interface. The Arduino must be connected before attempting to play tracks.

## Music Files

Place music files in the `wwwroot/music` directory relative to the API application, or upload them through the web interface. Only MP3 files are supported.