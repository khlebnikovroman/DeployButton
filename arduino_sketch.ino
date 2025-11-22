/*
 * Arduino Music Control Receiver
 * 
 * This sketch receives commands from the serial port to play different songs
 * on a piezo buzzer connected to pin 8.
 * 
 * Connections:
 * - Piezo buzzer positive pin to digital pin 8
 * - Piezo buzzer negative pin to GND
 */

#include "pitches.h"

// Pin for the piezo buzzer
const int BUZZER_PIN = 8;

// Define notes for some songs
int twinkle_twinkle_melody[] = {
  NOTE_C4, NOTE_C4, NOTE_G4, NOTE_G4, NOTE_A4, NOTE_A4, NOTE_G4,
  NOTE_F4, NOTE_F4, NOTE_E4, NOTE_E4, NOTE_D4, NOTE_D4, NOTE_C4,
  NOTE_G4, NOTE_G4, NOTE_F4, NOTE_F4, NOTE_E4, NOTE_E4, NOTE_D4,
  NOTE_G4, NOTE_G4, NOTE_F4, NOTE_F4, NOTE_E4, NOTE_E4, NOTE_D4,
  NOTE_C4, NOTE_C4, NOTE_G4, NOTE_G4, NOTE_A4, NOTE_A4, NOTE_G4,
  NOTE_F4, NOTE_F4, NOTE_E4, NOTE_E4, NOTE_D4, NOTE_D4, NOTE_C4
};

int twinkle_twinkle_notes[] = {
  4, 4, 4, 4, 4, 4, 2,
  4, 4, 4, 4, 4, 4, 2,
  4, 4, 4, 4, 4, 4, 2,
  4, 4, 4, 4, 4, 4, 2,
  4, 4, 4, 4, 4, 4, 2,
  4, 4, 4, 4, 4, 4, 2
};

int happy_birthday_melody[] = {
  NOTE_C4, NOTE_C4, NOTE_D4, NOTE_C4, NOTE_F4, NOTE_E4,
  NOTE_C4, NOTE_C4, NOTE_D4, NOTE_C4, NOTE_G4, NOTE_F4,
  NOTE_C4, NOTE_C4, NOTE_C5, NOTE_A4, NOTE_F4, NOTE_E4, NOTE_D4,
  NOTE_AS4, NOTE_AS4, NOTE_A4, NOTE_F4, NOTE_G4, NOTE_F4
};

int happy_birthday_notes[] = {
  8, 8, 4, 4, 4, 2,
  8, 8, 4, 4, 4, 2,
  8, 8, 4, 4, 4, 4, 2,
  8, 8, 4, 4, 4, 2
};

int jingle_bells_melody[] = {
  NOTE_E4, NOTE_E4, NOTE_E4, 
  NOTE_E4, NOTE_E4, NOTE_E4, 
  NOTE_E4, NOTE_G4, NOTE_C4, NOTE_D4, NOTE_E4,
  NOTE_F4, NOTE_F4, NOTE_F4, NOTE_F4, NOTE_F4, NOTE_E4, NOTE_E4, NOTE_E4, NOTE_E4,
  NOTE_E4, NOTE_D4, NOTE_D4, NOTE_E4, NOTE_D4, NOTE_G4
};

int jingle_bells_notes[] = {
  4, 4, 2,
  4, 4, 2,
  4, 4, 4, 4, 2,
  4, 4, 4, 4, 4, 4, 4, 4, 2,
  4, 4, 4, 4, 2
};

int ode_to_joy_melody[] = {
  NOTE_E4, NOTE_E4, NOTE_F4, NOTE_G4, NOTE_G4, NOTE_F4, NOTE_E4, NOTE_D4, NOTE_C4, NOTE_C4, NOTE_D4, NOTE_E4, NOTE_E4, NOTE_D4, NOTE_D4,
  NOTE_E4, NOTE_E4, NOTE_F4, NOTE_G4, NOTE_G4, NOTE_F4, NOTE_E4, NOTE_D4, NOTE_C4, NOTE_C4, NOTE_D4, NOTE_E4, NOTE_D4, NOTE_C4, NOTE_C4
};

int ode_to_joy_notes[] = {
  4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 2, 4, 2,
  4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 2, 4, 2
};

void setup() {
  pinMode(BUZZER_PIN, OUTPUT);
  Serial.begin(9600);
  Serial.println("Arduino Music Player Ready");
}

void loop() {
  if (Serial.available() > 0) {
    String command = Serial.readStringUntil('\n');
    command.trim(); // Remove any whitespace
    
    if (command == "twinkle_twinkle") {
      playTwinkleTwinkle();
    } else if (command == "happy_birthday") {
      playHappyBirthday();
    } else if (command == "jingle_bells") {
      playJingleBells();
    } else if (command == "ode_to_joy") {
      playOdeToJoy();
    } else if (command == "STOP") {
      // Stop playing by turning off the buzzer
      digitalWrite(BUZZER_PIN, LOW);
    } else {
      Serial.println("Unknown command: " + command);
    }
  }
}

void playTwinkleTwinkle() {
  int size = sizeof(twinkle_twinkle_melody) / sizeof(int);
  for (int i = 0; i < size; i++) {
    playNote(twinkle_twinkle_melody[i], twinkle_twinkle_notes[i]);
  }
}

void playHappyBirthday() {
  int size = sizeof(happy_birthday_melody) / sizeof(int);
  for (int i = 0; i < size; i++) {
    playNote(happy_birthday_melody[i], happy_birthday_notes[i]);
  }
}

void playJingleBells() {
  int size = sizeof(jingle_bells_melody) / sizeof(int);
  for (int i = 0; i < size; i++) {
    playNote(jingle_bells_melody[i], jingle_bells_notes[i]);
  }
}

void playOdeToJoy() {
  int size = sizeof(ode_to_joy_melody) / sizeof(int);
  for (int i = 0; i < size; i++) {
    playNote(ode_to_joy_melody[i], ode_to_joy_notes[i]);
  }
}

void playNote(int pitch, int note) {
  if (pitch == 0) {
    delay(1000 / note); // Rest
    return;
  }
  
  tone(BUZZER_PIN, pitch, 1000 / note);
  int pause = 1000 / note * 1.30;
  delay(pause);
  noTone(BUZZER_PIN);
  delay(1000 / note * 0.30); // Add short pause between notes
}