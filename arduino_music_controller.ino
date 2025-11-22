#include "SD.h"
#include "TMRpcm.h"
#include "SPI.h"

// SD card chip select pin
#define SD_CS 4

// Variable to store incoming serial commands
String incomingCommand = "";
bool commandComplete = false;

// Track name currently being played
String currentTrack = "";

void setup() {
  Serial.begin(9600);
  
  // Initialize SD card
  if (!SD.begin(SD_CS)) {
    Serial.println("SD Card initialization failed!");
    return;
  }
  
  // Set speaker output pin (use pin 9 for example)
  tmrpcm.speakerPin = 9;
  
  // Print ready message
  Serial.println("Arduino Music Controller Ready");
}

void loop() {
  // Check for incoming serial commands
  serialEvent();
  
  // Process command if complete
  if (commandComplete) {
    processCommand(incomingCommand);
    incomingCommand = "";
    commandComplete = false;
  }
  
  // Add a small delay to prevent overwhelming the processor
  delay(10);
}

void serialEvent() {
  while (Serial.available()) {
    char inChar = (char)Serial.read();
    
    // If newline is received, command is complete
    if (inChar == '\n') {
      commandComplete = true;
    } 
    else {
      // Add character to command string
      incomingCommand += inChar;
    }
  }
}

void processCommand(String command) {
  // Remove any trailing whitespace
  command.trim();
  
  // Check if command starts with PLAY:
  if (command.startsWith("PLAY:")) {
    String trackName = command.substring(5); // Get the track name after "PLAY:"
    
    // Stop any currently playing track
    tmrpcm.stopPlayback();
    
    // Play the requested track
    if (tmrpcm.play(trackName.c_str())) {
      currentTrack = trackName;
      Serial.println("Playing: " + trackName);
    } else {
      Serial.println("Failed to play: " + trackName);
    }
  }
  // Check if command is STOP
  else if (command == "STOP") {
    tmrpcm.stopPlayback();
    currentTrack = "";
    Serial.println("Playback stopped");
  }
  // Check if command is TEST
  else if (command == "TEST") {
    Serial.println("Arduino is connected and ready");
  }
  // Unknown command
  else {
    Serial.println("Unknown command: " + command);
  }
}