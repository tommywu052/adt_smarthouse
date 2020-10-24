# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license.

import iotc
from iotc import IOTConnectType, IOTLogLevel
from random import randint
import serial  # 引用pySerial模組
import json
 
COM_PORT = 'COM4'    # 指定通訊埠名稱
BAUD_RATES = 9600    # 設定傳輸速率
ser = serial.Serial(COM_PORT, BAUD_RATES)   # 初始化序列通訊埠

deviceId = "yourdeviceid"
scopeId = "yourscopeid"
deviceKey = "youriotcentralprimarykey"

iotc = iotc.Device(scopeId, deviceKey, deviceId, IOTConnectType.IOTC_CONNECT_SYMM_KEY)
iotc.setLogLevel(IOTLogLevel.IOTC_LOGGING_API_ONLY)

gCanSend = False
gCounter = 0

def onconnect(info):
  global gCanSend
  print("- [onconnect] => status:" + str(info.getStatusCode()))
  if info.getStatusCode() == 0:
     if iotc.isConnected():
       gCanSend = True

def onmessagesent(info):
  print("\t- [onmessagesent] => " + str(info.getPayload()))

def oncommand(info):
  print("- [oncommand] => " + info.getTag() + " => " + str(info.getPayload()))
  dstr = bytes('{"Servo":120}\n', encoding= 'utf-8')
  print(dstr)
  ser.write(dstr) 
def onsettingsupdated(info):
  print("- [onsettingsupdated] => " + info.getTag() + " => " + info.getPayload())
  d = json.loads(info.getPayload())
  dstr = bytes('{"Buzzer":'+str(d['value'])+'}\n', encoding= 'utf-8')
  #dstr = b'{"Buzzer":'+d['value']+'}\n'
  
  if info.getTag() == "Buzzer":
    print(dstr)
    ser.write(dstr)
  if info.getTag() == "LCD_Msg":
    dstr = bytes('{"LCD_Msg":["'+str(d['value'])+'"]}\n', encoding= 'utf-8')
    print(dstr)
    ser.write(dstr)
  if info.getTag() == "RGB_out":
    dstr = bytes('{"RGB_out":'+str(d['value'])+'}\n', encoding= 'utf-8')
    print(dstr)
    ser.write(dstr)  
  if info.getTag() == "RGB_in":
    dstr = bytes('{"RGB_in":'+str(d['value'])+'}\n', encoding= 'utf-8')
    print(dstr)
    ser.write(dstr)  
  if info.getTag() == "Servo":
    dstr = bytes('{"Servo":'+str(d['value'])+'}\n', encoding= 'utf-8')
    print(dstr)
    ser.write(dstr)  
  if info.getTag() == "Relay2":
    if d['value'] :
      dstr = bytes('{"Relay2":1}\n', encoding= 'utf-8')
    else:
      dstr = bytes('{"Relay2":0}\n', encoding= 'utf-8')
    print(dstr)
    ser.write(dstr)  

iotc.on("ConnectionStatus", onconnect)
iotc.on("MessageSent", onmessagesent)
iotc.on("Command", oncommand)
iotc.on("SettingsUpdated", onsettingsupdated)

iotc.connect()

while iotc.isConnected():
  iotc.doNext() # do the async work needed to be done for MQTT
  if gCanSend == True:
    if gCounter % 20 == 0:
      gCounter = 0
      print("Sending telemetry..")
      while ser.in_waiting:        # 若收到序列資料…
        data_raw = ser.readline()  # 讀取一行
        data = data_raw.decode()   # 用預設的UTF-8解碼
        #print('接收到的原始資料：', data_raw)
        print('接收到的資料：', data)
      if data.strip() !='Start':
        #print('IoT：', data)
        #print('length:',len(data.strip()))
        iotc.sendTelemetry(data)
      #iotc.sendTelemetry("{ \
#\"temp\": " + str(randint(20, 45)) + ", \
#\"accelerometerX\": " + str(randint(2, 15)) + ", \
#\"accelerometerY\": " + str(randint(3, 9)) + ", \
#\"accelerometerZ\": " + str(randint(1, 4)) + "}")
        iotc.sendProperty('{"Distance":'+str(json.loads(data)['Distance'])+'}')
        if 'RFID' in json.loads(data):
          iotc.sendProperty('{"RFID":"'+str(json.loads(data)['RFID'])+'"}')
        iotc.sendState('{"PIRState":'+str(json.loads(data)['PIRState'])+'}')
    gCounter += 1
ser.close()    # 清除序列通訊物件
