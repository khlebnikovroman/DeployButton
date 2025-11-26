// Combined deploy + sound + PING/PONG
// Supported commands: PLAYSOUND x, SETVOLUME x, PAUSE, PLAY, PING
// Button events: DEPLOY (press), RELEASE (release)

#include "SoftwareSerial.h"
#include "DFRobotDFPlayerMini.h"

// DFPlayer
SoftwareSerial mySoftwareSerial(10, 11); // RX, TX
DFRobotDFPlayerMini myDFPlayer;

// Button
const int buttonPin = 2;
bool lastButtonState = HIGH; // начальное состояние (INPUT_PULLUP → HIGH при отжатой)
unsigned long lastDebounceTime = 0;
const unsigned long debounceDelay = 50;

void setup() {
  pinMode(buttonPin, INPUT_PULLUP);

  Serial.begin(115200);
  mySoftwareSerial.begin(9600);

  if (!myDFPlayer.begin(mySoftwareSerial)) {
    // Silent failure – optional: blink LED or hang
    while (true);
  }

  myDFPlayer.setTimeOut(500);
  myDFPlayer.volume(15);
  myDFPlayer.EQ(0);
}

void loop() {
  bool currentButtonState = digitalRead(buttonPin);

  if (currentButtonState != lastButtonState) {
    lastDebounceTime = millis();
  }

  if ((millis() - lastDebounceTime) > debounceDelay) {
    static bool wasPressed = false;

    if (currentButtonState == LOW && !wasPressed) {
      wasPressed = true;
      Serial.println("BUTTONPRESS");
    } else if (currentButtonState == HIGH && wasPressed) {
      wasPressed = false;
      Serial.println("BUTTONRELEASE");
    }
  }

  lastButtonState = currentButtonState;

  // --- Serial command handling ---
  if (Serial.available()) {
    String cmd = Serial.readStringUntil('\n');
    cmd.trim();

    if (cmd.startsWith("PLAYSOUND ")) {
      int num = cmd.substring(10).toInt();
      if (num >= 1 && num <= 255) {
        myDFPlayer.play(num);
      }

    } else if (cmd.startsWith("SETVOLUME ")) {
      int vol = cmd.substring(10).toInt();
      if (vol >= 0 && vol <= 30) {
        myDFPlayer.volume(vol);
      }

    } else if (cmd == "PAUSE") {
      myDFPlayer.pause();

    } else if (cmd == "PLAY") {
      myDFPlayer.start();

    } else if (cmd == "PING") {
      Serial.println("PONG");
    }
    // All other commands are silently ignored
  }
}