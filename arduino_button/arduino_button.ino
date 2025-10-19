// deploy_button.ino
const int buttonPin = 2;      // Куда подключим кнопку (пока не используется)
const int ledPin = 13;        // Встроенный светодиод

bool buttonState = false;
bool lastButtonState = false;
unsigned long lastDebounceTime = 0;
const unsigned long debounceDelay = 50;

void setup() {
  pinMode(buttonPin, INPUT_PULLUP); // Внутренний подтяжка к +5V, кнопка на GND
  pinMode(ledPin, OUTPUT);
  Serial.begin(9600);
}

void loop() {
  // Читаем состояние кнопки (с антидребезгом)
  bool reading = digitalRead(buttonPin);

  if (reading != lastButtonState) {
    lastDebounceTime = millis();
  }

  if ((millis() - lastDebounceTime) > debounceDelay) {
    if (reading != buttonState) {
      buttonState = reading;
      if (buttonState == LOW) { // Кнопка замыкает на GND → LOW
        Serial.println("DEPLOY");
        digitalWrite(ledPin, HIGH);
        delay(200);
        digitalWrite(ledPin, LOW);
      }
    }
  }
  lastButtonState = reading;
}