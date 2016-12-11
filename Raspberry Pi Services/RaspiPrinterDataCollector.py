import time
import pypyodbc
from pysnmp.entity.rfc3413.oneliner import cmdgen
import collections
import signal
import sys
import smtplib
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText


def signal_handler(signal, frame):
  connection.close()
  sys.exit(0)

signal.signal(signal.SIGINT, signal_handler)
f = open("printer.txt")
printerName = [line for line in f]
printerIP = ''
errorCode = {'0': 'Low Paper', '1' : 'No Paper', '2' : 'Low Toner', '3' : 'No Toner', '4' : 'Door Open', '5' : 'Jammed', '6' : 'Offline', '7' : 'Service Requested', '8' : 'Input Tray Missing', '9' : 'Output Tray Missing', '10' : 'Marker Supply Missing', '11' : 'Output Near Full', '12' : 'Output Full', '13' : 'Input Tray Empty', '14': 'Overdue Preventative Maintenance'}
printerStates = {'0' : 'Warning', '1' : 'Error', '2' : 'Warning', '3' : 'Error', '4' : 'Error', '5' : 'Error', '6' : 'Warning', '7' : 'Warning', '8' : 'Warning', '9' : 'Warning', '10' : 'Error', '11' : 'Warning', '12' : 'Error', '13' : 'Warning', '14' : 'Warning'}
printerStatusCode = {'Active' : '0', 'Warning' : '1', 'Error' : '2'}

connection = pypyodbc.connect("DRIVER={SQL Server};SERVER=fh3i5fqqif.database.windows.net;UID=rajsha@fh3i5fqqif.database.windows.net;PWD=SmartPrinter1;DATABASE=SmartPrinterData")
#connection = pypyodbc.connect('DSN=SmartPrinterDataSource;' 'uid=rajsha@fh3i5fqqif;' 'pwd=SmartPrinter1')
 
cursor = connection.cursor()
dataToPull = {'Pages printed in lifetime': '', 'Pages printed since last reboot' : '', 'Black toner max level' : '', 'Black toner level' : '', 'Yellow toner max level' : '', 'Yellow toner level': '',
'Cyan toner max level' : '', 'Cyan toner level': '', 'Magenta toner max level': '', 'Magenta toner level' : '', 'Status' : ''}
 
pullSqlCommand = ("Select OID, Data from PrinterOIDMapping")
cursor.execute(pullSqlCommand)
results = cursor.fetchone()
 
while results:
    if results[1] in dataToPull:
        dataToPull[results[1]] = results[0]
    results = cursor.fetchone()
 
ipAddressSqlCommand = ("Select IPAddress from PrinterIP where PrinterName = ?")
cursor.execute(ipAddressSqlCommand, printerName)
printerIP = cursor.fetchone()[0]
 
dataToPull = dataToPull.items()
dataToPull = collections.OrderedDict(dataToPull)
 
updateQuery = ("Update Printer Set YellowTotal = ?, NoOfPagesPrintedSinceLastReboot = ?, BlackTotal = ?, Status = ?, Yellow = ?, MagentaTotal = ?, CyanTotal = ?, Black = ?, NoOfPagesPrintedInTotal = ?, Cyan = ?, Magenta = ?, StatusMessage = ?, StatusCode = ? Where Name = ?")
errorToReport = "" 
prevErrorReported = ""

notificationRecepientAddressQuery = ('Select EmailAddress from NotificationRecepient')
notificationRecepients = []
cursor.execute(notificationRecepientAddressQuery)
results = cursor.fetchone()

while results:
  notificationRecepients.append(results[0])
  results = cursor.fetchone()

notificationSenderAddressQuery = ('Select EmailAddress, Password from NotificationSender')
notificationSenderAddress, notificationSenderPassword =  '', ''
cursor.execute(notificationSenderAddressQuery)
results = cursor.fetchone()
notificationSenderAddress, notificationSenderPassword = results[0], results[1] 

def sendAlertMail(pName, pIP, errorMsg):
  if errorToReport == "" or prevErrorReported == errorToReport:
    return
  SERVER = "smtp-mail.outlook.com"
  FROM = notificationSenderAddress
  TO = notificationRecepients # must be a list

  SUBJECT = "Smart Printer Error Report"
  TEXT = """Printer Error Log:
            => Printer Name: %s
            => Printer IP  : %s
            => ErrorMessage: %s """ % (pName, pIP, errorMsg)

  # Prepare actual message
  message = """From: %s\r\nTo: %s\r\nSubject: %s\r\n\

  %s
  """ % (FROM, ", ".join(TO), SUBJECT, TEXT)

  # Send the mail
  server = smtplib.SMTP(SERVER, 587)
  server.starttls()

  server.login(notificationSenderAddress, notificationSenderPassword)
  server.sendmail(FROM, TO, message)
  server.quit()  

while True:
    inputValues = []
    statusMessageToSet = "Warning"
    statusCodeToSet = "1"
    for key in dataToPull.keys():
        printerOID = dataToPull[key]
        printerOID = tuple([int(x) for x in printerOID.split(".")])
        errorIndication, errorStatus, errorIndex, varBinds = cmdgen.CommandGenerator().getCmd(cmdgen.CommunityData('my-agent', 'public', 0),  cmdgen.UdpTransportTarget((printerIP, 161)), printerOID)
        print varBinds
        if key == "Status":
          statusVals = str(varBinds[0][1])
          if statusVals  == 'Ready' or  statusVals == 'Sleep mode on' or 'Processing' in statusVals or 'Initializing Scanner' in statusVals:
            statusMessageToSet = 'Working'
            statusCodeToSet = "0"
            errorToReport = ""
            prevErrorReported = ""
          else:
            statusMessageToSet = "Error"
            statusCodeToSet = "2"
            errorToReport = statusVals          
          inputValues.append(statusVals)
        else:
          inputValues.append(str(varBinds[0][1]))
 
    inputValues.append(statusMessageToSet)
    inputValues.append(statusCodeToSet)
    inputValues.append(printerName[0])
    #print "Updating the table with values, ", inputValues
    cursor.execute(updateQuery, inputValues)  
    cursor.commit()
    sendAlertMail(printerName, printerIP, errorToReport)
    prevErrorReported = errorToReport
    errorToReport = ""
    time.sleep(90)



  
