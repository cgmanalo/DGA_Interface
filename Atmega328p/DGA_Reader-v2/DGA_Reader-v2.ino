//Transformer DGA Monitoring and Analysis
//EE200D- Capstone Project
//Copyright (c) 2019-2020, F. Ferrer, P. Paterno, Cesar G. Manalo, Jr. (Adviser)

#include <EEPROM.h>
#include <avr/wdt.h>

String SerialData;
void(* resetFunc) (void) = 0;
#define DataPin A0
#define S0 3
#define S1 4
#define S2 5
#define S3 6
#define EN 7
#define Data A0
byte brightness = B00000000;
int fadeamount = 1;
const byte IndicatorLight = 11;
byte H2AMask1 = B00000111;
byte H2AMask2 = B00000000;  //0
byte CH4AMask1 = B00001111;
byte CH4AMask2 = B00001000; //1
byte C2H4AMask1 = B00010111;
byte C2H4AMask2 = B00010000; //2
byte C2H6AMask1 = B00011111;
byte C2H6AMask2 = B00011000; //3
byte COAMask1 = B00100111;
byte COAMask2 = B00100000; //4
byte CO2AMask1 = B00101111;
byte CO2AMask2 = B00101000; //5
byte C2H2AMask1 = B00110111;
byte C2H2aMask2 = B00110000; //6

byte H2B =    B00000111|B01000000; //8
byte CH4B =   B00001111|B01001000; //9
byte C2H4B =  B00010111|B01010000; //10
byte C2H6B =  B00011111|B01011000; //11
byte COB =    B00100111|B01100000; //12
byte CO2B =   B00101111|B01101000; //13
byte C2H2B =  B00110111|B01110000; //14

//>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>


void setup() {
 Serial.begin(9600);
 pinMode(S0, OUTPUT);
 pinMode(S1, OUTPUT);
 pinMode(S2, OUTPUT);
 pinMode(S3, OUTPUT);
 pinMode(EN, OUTPUT);
 pinMode(IndicatorLight, OUTPUT);

 //PORTD = (PORTD & B11101111) | B00001100; //set comms. channel to UART/PLC
 
// getConfiguration();
 Preamble();
 //watchdogSetup();
}

//>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

void loop() {
  analogWrite(IndicatorLight,brightness++);
  SerialData = GetSerialData();
  String CommandField = ExtractField(SerialData,1);
  if (CommandField == "T1"){
    String gasName = ExtractField(SerialData,2);
    SR("T1|" + gasName + "|" + (String) ReadT1GasLevel(gasName));
  }
  else if (CommandField == "T2"){
    String gasName = ExtractField(SerialData,2);
    SR("T2|" + gasName + "|" + (String) ReadT2GasLevel(gasName));
  }

  //...add code here
  
  else if (CommandField == "HB"){
    SR("OK.");
  }
  

}

//***** SUBROUTINES *****//

//*****TRANSFORMER 1*****//
float ReadT1GasLevel(String gas){
  if (gas == "H2"){
    PORTD = PORTD & B11000111; //set multiplexer to read H2
    delay(100);
    return 1000.0*(analogRead(A0)/1023.0);
  }
  else if (gas == "CH4"){
    PORTD = (PORTD & B11001111)|B00001000; //set multiplexer to read CH4
    delay(100);
    return 80.0*(analogRead(A0)/1023.0);
  }
  else if (gas == "C2H4"){
    PORTD = (PORTD & B11001111)|B00001000; //set multiplexer to read C2H4
    delay(100);
    return 80.0*(analogRead(A0)/1023.0);
  }
  else if (gas == "C2H6"){
    PORTD = (PORTD & B11011111)|B00011000; //set multiplexer to read C2H6
    delay(100);
    return 35.0*(analogRead(A0)/1023.0);
  }
  else
    return -1;
}

//*****TRANSFORMER 2*****//
float ReadT2GasLevel(String gas){
  if (gas == "H2"){
    PORTD = PORTD & B11000111; //set multiplexer to read H2
    delay(100);
    return 1000.0*(analogRead(A1)/1023.0);
  }
  else if (gas == "CH4"){
    PORTD = (PORTD & B11001111)|B00001000; //set multiplexer to read CH4
    delay(100);
    return 80.0*(analogRead(A1)/1023.0);
  }
  else if (gas == "C2H4"){
    PORTD = (PORTD & B11010111)|B00010000; //set multiplexer to read C2H4
    delay(100);
    return 100.0*(analogRead(A1)/1023.0);
  }
  else if (gas == "C2H6"){
    PORTD = (PORTD & B11011111)|B00011000; //set multiplexer to read C2H6
    delay(100);
    return 35.0*(analogRead(A1)/1023.0);
  }
  else
    return -1;
}

float ReadDGA(byte Mask1, byte Mask2){
  PORTD = (PORTD & Mask1) | Mask2;
  delay(100);
  return analogRead(A0);
}

void serialFlush(){
  while(Serial.available() > 0) {
    char t = Serial.read();
  }
}

void SR(String replyMessage){
  Serial.println(replyMessage);
}

void setChan(byte mask){
  PORTD = (PORTD & mask);
  delay(100);
}

String GetSerialData(){
  Serial.setTimeout(10);
  return Serial.readStringUntil('\n');
}

String ExtractField(String rawString, int fieldNum){
   char dataArray[30];
   char fieldArray[25];   
   char inChar;
   
   rawString.toCharArray(dataArray,rawString.length()+1);

   //first field
   if (fieldNum == 1){
     int i=0;
     for(i;i<rawString.length();i++){
       inChar = dataArray[i];
       if (inChar == '|'){
         break;
       }
       fieldArray[i] = inChar;
     } 
     fieldArray[i]='\0';
     return fieldArray;
   }

   //second field
   else if (fieldNum == 2) {
     int i = rawString.indexOf('|')+1;
     int j=i;
     for(i;i<rawString.length();i++){
       inChar = dataArray[i];
       if (inChar == '|'){
         break;
       }
       fieldArray[i-j] = inChar;
     }
     fieldArray[i-j]='\0';
     return fieldArray;
   }

  //third field
   else if (fieldNum == 3) {
     int i = rawString.indexOf('|',rawString.indexOf('|')+1)+1;
     int j=i;
     for(i;i<rawString.length();i++){
       inChar = dataArray[i];
       if (inChar == '|'){
         break;
       }
       fieldArray[i-j] = inChar;
     }
     fieldArray[i-j]='\0';
     return fieldArray;
   }
   else if (fieldNum == 4) {
     int i = rawString.indexOf('|',rawString.indexOf('|',rawString.indexOf('|')+1)+1)+1;
     int j=i;
     for(i;i<rawString.length();i++){
       inChar = dataArray[i];
       if (inChar == '|'){
         break;
       }
       fieldArray[i-j] = inChar;
     }
     fieldArray[i-j]='\0';
     return fieldArray;
   }
     
   return "";
}

void watchdogSetup(void)
{
cli();       // disable all interrupts
wdt_reset(); // reset the WDT timer
/*
 WDTCSR configuration:
 WDIE = 1: Interrupt Enable
 WDE = 1 :Reset Enable
 WDP3 = 1 :For 8000ms Time-out
 WDP2 = 0 :For 8000ms Time-out
 WDP1 = 0 :For 8000ms Time-out
 WDP0 = 1 :For 8000ms Time-out
*/
// Enter Watchdog Configuration mode:
WDTCSR |= (1<<WDCE) | (1<<WDE);
// Set Watchdog settings:
 WDTCSR = (1<<WDIE) | (1<<WDE) | (1<<WDP3) | (0<<WDP2) | (0<<WDP1) | (1<<WDP0);
sei();
}

void Preamble()
{
  Serial.println("\n Transformer DGA Monitoring A\n");
  Serial.println("EE200D-2019 Capstone Project");
  Serial.println("Copyright (c) 2019-2020, F. Ferrer, G. Paterno, Cesar G. Manalo, Jr. (Adviser)");
  delay(2000);
}



/*
void getConfiguration()
{
 ROOM_CONTROLLER_ADDR = EEPROM.read(ctlrAddrMap); //retrieve first address
 if (ROOM_CONTROLLER_ADDR < 65 || ROOM_CONTROLLER_ADDR > 90){ROOM_CONTROLLER_ADDR = 65;}
 ROOM_COUNT = EEPROM.read(ctlrAddrCountMap)-48; //retrieve number of addresses
 if (ROOM_COUNT < 1 || ROOM_COUNT > 3){ROOM_COUNT = 3;}
 timeDelayLighting = (EEPROM.read(timeDelayLightingAddr)-48)*500;
 if (timeDelayLighting < 0 || timeDelayLighting > 2500){timeDelayLighting = 0;}
 timeDelayACU = (EEPROM.read(timeDelayACUAddr)-48)*500;
 if (timeDelayACU < 0 || timeDelayACU > 2500){timeDelayACU = 0;}
 transmitWait = (EEPROM.read(transmitWaitAddr)-48)*200;
 if (transmitWait < 0 || transmitWait > 1800){transmitWait = 0;}
 receiveWait = (EEPROM.read(receiveWaitAddr)-48)*200;
 if (receiveWait < 200 || receiveWait > 1000){receiveWait = 200;}
}

String dayAsString(const Time::Day day) {
  switch (day) {
    case Time::kSunday: return "Sun";
    case Time::kMonday: return "Mon";
    case Time::kTuesday: return "Tue";
    case Time::kWednesday: return "Wed";
    case Time::kThursday: return "Thu";
    case Time::kFriday: return "Fri";
    case Time::kSaturday: return "Sat";
  }
  return "???";  //unknown day
}
String getWeekday (int dayNumber){
  String Weekday;  
  switch (dayNumber) {
      case 0:
        Weekday = "Sun";
        break;
      case 1:
        Weekday = "Mon";
        break;
      case 2:
        Weekday = "Tue";
        break;
      case 3:
        Weekday = "Wed";
        break;
      case 4:
        Weekday = "Thu";
        break;
      case 5:
        Weekday = "Fri";
        break;
      case 6:
        Weekday = "Sat";
        break;
      default:
        break;
  }
  return Weekday;
}
String getAMPM(int hourNumber){
  if (hourNumber < 12)
  {
      return "AM";
  }
  else
  {
      return "PM";
  }
}
String getNormalizedHour(int hourNumber){
  String hourString;
 
  if (hourNumber < 13)
  {
    hourString = String(hourNumber); 
  }
  else
  {
    hourString = String(hourNumber-12); 
  }
  if (hourString.length()==1)
  {
      return "0"+hourString;
  }
  else
  {
      return hourString;
  }
}

String getNormalizedMinute(int minuteNumber){
  String minuteString;
 
  minuteString = String(minuteNumber);
  if (minuteString.length()==1)
  {
      return "0"+minuteString;
  }
  else
  {
      return minuteString;
  }
}

String getNormalizedSecond(int secondNumber){
  String secondString;
 
  secondString = String(secondNumber);
  if (secondString.length()==1)
  {
      return "0"+secondString;
  }
  else
  {
      return secondString;
  }
}

unsigned int timeValue(String timeString){

   int AMPMAddress = timeString.length()-2;
   //return timeString.substring(3,5).toInt();

   if (timeString.substring(AMPMAddress)=="AM"){
     return 60*timeString.substring(0,2).toInt()+timeString.substring(3,5).toInt();
   }
   else{
     if (timeString.substring(0,2).toInt()==12){
       return 60*12+timeString.substring(3,5).toInt();
     }
     else{
       return 60*(12+timeString.substring(0,2).toInt())+timeString.substring(3,5).toInt();
     }
   }
}

void blink() {
  if ((long)(micros()-last_micros) > debouncingTime*1000){
    digitalWrite(lighting1Pin,!digitalRead(lighting1Pin));
    last_micros = micros();
  }
}
*/
