
#include <DHT.h>
#include <LiquidCrystal_I2C.h>
#include <Wire.h>
#include <Keypad.h>
#include <SPI.h>
#include <Servo.h>
#include <String.h>
#include <RFID.h>
#include <ArduinoJson.h>
#include <SimpleTimer.h>


SimpleTimer timer;
StaticJsonDocument<200> SendDoc;
StaticJsonDocument<200> ReceivedDoc;;

//*********************** Servo;
#define SERVOPIN 2
Servo myservo;

//***********************DHT11
#define DHTPIN 5         //Set pin of DHT sensor 
#define DHTTYPE DHT11     
DHT dht(DHTPIN, DHTTYPE);
void DHTSENSOR()
{
  float h = dht.readHumidity();
  float t = dht.readTemperature(); // or dht.readTemperature(true) for Fahrenheit

  if (isnan(h) || isnan(t)) {
    Serial.println("Failed to read from DHT sensor!");
    return;
  }

  SendDoc["Temperature"] = t;
  SendDoc["Humidity"] = h;
}

//***********************Flame sensor
#define FLAMEPIN A2     //Set pin of flame sensor 
int FlameVal = 0;       
void FLAMESENSOR() {
  FlameVal = analogRead(FLAMEPIN);
  SendDoc["Flame"] = FlameVal;
}

//***********************Light sensor
#define LIGHTSENSORPIN A0
int LightVal = 0;
void LIGHTSENSOR() {
  LightVal = analogRead(LIGHTSENSORPIN);
  SendDoc["Light"] = LightVal;
}

//***********************Light sensor
#define BUZZERPIN 31
void BuzzerOn(int duration){
  digitalWrite(BUZZERPIN,LOW);
  delay(duration);
  digitalWrite(BUZZERPIN,HIGH);
}

//***********************UltraSonic sensor
#define ECHOPIN 29
#define TRIGPIN 27
float DistanceVal = 0;
void ULTRASONIC() {
  float duration;
  digitalWrite(TRIGPIN, LOW); 
  delayMicroseconds(2);
 
  digitalWrite(TRIGPIN, HIGH);
  delayMicroseconds(10);
  digitalWrite(TRIGPIN, LOW);
  duration = pulseIn(ECHOPIN, HIGH);
  DistanceVal = (duration / 2) * 0.0344;

  SendDoc["Distance"] = DistanceVal;
}

//***********************MQ2 Gas sensor
#define SMOKEPIN A1
int SmokeVal = 0;
void SMOKESENSOR() {
  SmokeVal = analogRead(SMOKEPIN);
  SendDoc["Smoke"] = SmokeVal;
}

//***********************PIR sensor
#define PIRPIN 24
bool pirState = LOW;             
int pirVal = 0;                    
void PIRSENSOR() {
  pirVal = digitalRead(PIRPIN);  
  if(pirVal == HIGH) {            
    if (pirState == LOW) {
      pirState = HIGH;
    }
  } 
  else{
    if(pirState == HIGH){
      pirState = LOW;
    }
  }
  SendDoc["PIRState"] = pirState;
}


//*********************** LCD
#define ROW_NUM 2                             
#define COL_NUM 16                            
LiquidCrystal_I2C lcd(0x27,COL_NUM,ROW_NUM); 
char* LCDMsg = "";


//***********************Key Pad
const byte ROWS = 4; //four rows
const byte COLS = 4; //four columns
char hexaKeys[ROWS][COLS] = {
  {'1','2','3','A'},
  {'4','5','6','B'},
  {'7','8','9','C'},
  {'*','0','#','D'}
};
byte rowPins[ROWS] = {33, 35, 37, 39}; //connect to the row pinouts of the keypad
byte colPins[COLS] = {41, 43, 45, 47}; //connect to the column pinouts of the keypad

Keypad keypad= Keypad(makeKeymap(hexaKeys), rowPins, colPins, ROWS, COLS);
String inputCode = "";
bool acceptKey = true;

void KEYPADSENSOR() {
  char key = keypad.getKey();
  if (acceptKey && key != NO_KEY) {
    if (key == '*') {   
      clearRow(4);  // 從第4個字元開始清除
      inputCode = "";
    }else if (key == '#') {  // 比對輸入密碼
      checkPinCode();
    }else{
      inputCode += key;  // 儲存用戶的按鍵字元
      lcd.print('*');
    }
  }
}

void checkPinCode() {
  acceptKey = false;  // 暫時不接受用戶按鍵輸入
  clearRow(0);        // 從第0個字元開始清除LCD畫面
  lcd.noCursor();
  lcd.setCursor(0, 1);  // 切換到第2行
  lcd.print("Send!");
  RGB_outSetColor(255, 255, 255);
  BuzzerOn(200);
  SendDoc["Pin"] = inputCode;
  SendJson();
  delay(1000);

  //Check the PIN code. Open door if the PIN Correct.
  if(inputCode == "123A"){
    lcd.clear();
    lcd.print("Welcome Home");
    myservo.write(90);
    RGB_outSetColor(0, 255, 0);
    delay(2000);
    myservo.write(0);
    RGB_outSetColor(0,0,0);
    resetLCD();    
  }
  else{
    lcd.clear();
    lcd.print("Wrong PIN");
    RGB_outSetColor(255, 0, 0);
    delay(2000);
    RGB_outSetColor(0,0,0);
    resetLCD();  
    }
}

void resetLCD() {
  lcd.clear();
  lcd.print("Enter PIN");
  lcd.setCursor(0, 1);  // 切換到第2行
  lcd.print("PIN:");
  lcd.cursor();
 
  acceptKey = true;
  inputCode = "";
}

void clearRow(byte n) {
  byte last = COL_NUM - n;
  lcd.setCursor(n, 1); // 移動到第2行，"PIN:"之後
  for (byte i = 0; i < last; i++) {
    lcd.print(" ");
  }
  lcd.setCursor(n, 1);
}


//*********************** RFID
#define SDAPIN 48
#define RSTPIN 49
RFID rfid(SDAPIN,RSTPIN);
String CardNumber="";
void RFIDSENSOR() {
  if (rfid.isCard( )) {
    BuzzerOn(100); 
    if (rfid.readCardSerial()) {
      for(int i=0; i<=4; i++)
      {
        CardNumber = CardNumber+String(rfid.serNum[i],HEX);
      }
      CardNumber.toUpperCase();
      SendDoc["RFID"] = CardNumber;
      SendJson();
      ShowUser(CardNumber);    
    }
  }
  CardNumber = "";
  rfid.halt();
}

void ShowUser(String id)
{
  if(id == "26FD7DF751") {
    lcd.clear();
    lcd.print("Hello Tommy");
    myservo.write(90);
    RGB_outSetColor(0, 255, 0);
    delay(2000);
    myservo.write(0);
    RGB_outSetColor(0,0,0);
    resetLCD();
  } 
  else if(id == "1BE67A78") {
    lcd.clear();
    lcd.print("Hello York");
    myservo.write(90);
    RGB_outSetColor(0, 255, 0);
    delay(2000);
    myservo.write(0);
    RGB_outSetColor(0,0,0);
    resetLCD();
  }
  else{
    lcd.clear();
    lcd.print("Who are you!");
    RGB_outSetColor(255, 0, 0);
    delay(2000);
    RGB_outSetColor(0,0,0);
    resetLCD();
  }
}

//*********************** Message
String ReceivedString;
void SendJson(){
  DHTSENSOR();
  FLAMESENSOR();
  LIGHTSENSOR();
  ULTRASONIC();
  SMOKESENSOR();
  PIRSENSOR();
  serializeJson(SendDoc, Serial);
  Serial.println();
  SendDoc.clear();
}


//*********************** Received Command
int RGB_in[3]={0,0,0};     
int RGB_out[3]={0,0,0};
int ServoDegree_Cmd=0;
bool Relay1_Cmd = 0;
bool Relay2_Cmd = 0;
int Buzzer_Cmd = 0;
char* LCD_Cmd[2]={"",""};

void CommandRun(){
  RGB_inSetColor(RGB_in[0],RGB_in[1],RGB_in[2]);
  RGB_outSetColor(RGB_out[0],RGB_out[1],RGB_out[2]);
  myservo.write(ServoDegree_Cmd);
  setRelay(Relay1_Cmd,Relay2_Cmd);
  lcd.clear(); 
  lcd.setCursor(0, 0);
  lcd.print(LCD_Cmd[0]);
  lcd.setCursor(0, 1);
  lcd.print(LCD_Cmd[1]);
  BuzzerOn(Buzzer_Cmd);
  timer.setTimeout(10000,CommandReset);
}

void CommandReset(){
  RGB_in[0]=0;RGB_in[1]=0;RGB_in[2]=0;
  RGB_out[0]=0;RGB_out[1]=0;RGB_out[2]=0;
  ServoDegree_Cmd=0;
  Relay1_Cmd=0;
  Relay2_Cmd=0;
  RGB_inSetColor(RGB_in[0],RGB_in[1],RGB_in[2]);
  RGB_outSetColor(RGB_out[0],RGB_out[1],RGB_out[2]);
  myservo.write(ServoDegree_Cmd);
  setRelay(Relay1_Cmd,Relay2_Cmd);
  resetLCD();
}
//*********************** RGB;
#define RGB_INPIN_G 11
#define RGB_INPIN_R 12
#define RGB_INPIN_B 13
void RGB_inSetColor(int r,int g,int b){
  analogWrite(RGB_INPIN_R, r);
  analogWrite(RGB_INPIN_G, g);
  analogWrite(RGB_INPIN_B, b); 
}

#define RGB_OUTPIN_G 8
#define RGB_OUTPIN_R 9
#define RGB_OUTPIN_B 10
void RGB_outSetColor(int r,int g,int b){
  analogWrite(RGB_OUTPIN_R, r);
  analogWrite(RGB_OUTPIN_G, g);
  analogWrite(RGB_OUTPIN_B, b); 
}

//*********************** 2 CHANNEL RELAYS;
#define RELAYPIN1 4
#define RELAYPIN2 3
void setRelay(bool Relay1, bool Relay2){
  if(Relay1 == 1)
    digitalWrite(RELAYPIN1, LOW);
  else if(Relay1 == 0)
    digitalWrite(RELAYPIN1, HIGH);
  if(Relay2 == 1)
    digitalWrite(RELAYPIN2, LOW);
  else if(Relay2 == 0)
    digitalWrite(RELAYPIN2, HIGH);  
}



void setup()
{
  pinMode(TRIGPIN, OUTPUT);
  pinMode(ECHOPIN, INPUT);
  pinMode(PIRPIN, INPUT);
  pinMode(RELAYPIN1, OUTPUT);
  pinMode(RELAYPIN2, OUTPUT);
  setRelay(Relay1_Cmd,Relay2_Cmd);
  pinMode(BUZZERPIN, OUTPUT);
  digitalWrite(BUZZERPIN,HIGH);
  
  Serial.begin(9600);
  Serial.println("Start");
  SPI.begin();
  dht.begin();
  rfid.init();
  myservo.attach(SERVOPIN);
  myservo.write(ServoDegree_Cmd);
  
  lcd.init();                      // initialize the lcd 
  lcd.backlight();
  lcd.clear(); 
  lcd.setCursor(0, 0);
  resetLCD();

  RGB_inSetColor(RGB_in[0], RGB_in[1], RGB_in[2]);
  RGB_outSetColor(RGB_out[0], RGB_out[1], RGB_out[2]);
  timer.setInterval(3000,SendJson);
  
}


//**********************void loop
void loop() 
{
  while(Serial.available()){
    ReceivedString = Serial.readString();
    DeserializationError error = deserializeJson(ReceivedDoc, ReceivedString);
    if (error) {
      Serial.print(F("deserializeJson() failed: "));
      Serial.println(error.c_str());
      return;
    }

    RGB_in[0] = ReceivedDoc["RGB_in"][0];
    RGB_in[1] = ReceivedDoc["RGB_in"][1];
    RGB_in[2] = ReceivedDoc["RGB_in"][2];
    RGB_out[0] = ReceivedDoc["RGB_out"][0];
    RGB_out[1] = ReceivedDoc["RGB_out"][1];
    RGB_out[2] = ReceivedDoc["RGB_out"][2];
    ServoDegree_Cmd = ReceivedDoc["Servo"];
    Relay1_Cmd = ReceivedDoc["Relay1"];
    Relay2_Cmd = ReceivedDoc["Relay2"];
    LCD_Cmd[0]= ReceivedDoc["LCD_Msg"][0];
    LCD_Cmd[1]= ReceivedDoc["LCD_Msg"][1];
    Buzzer_Cmd= ReceivedDoc["Buzzer"];
    CommandRun();
  }
  RFIDSENSOR();
  KEYPADSENSOR();
  timer.run(); 

////LCDMsg = ReceivedDoc["LCD"];
////lcd.print(LCDMsg);
//  DHTSENSOR();
//  FLAMESENSOR();
//  LIGHTSENSOR();
//  ULTRASONIC();
//  SMOKESENSOR();
//  PIRSENSOR();
////  timer.run();
//  KEYPADSENSOR();
//

}
