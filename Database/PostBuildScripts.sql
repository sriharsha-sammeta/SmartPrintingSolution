----------------------Scripts for Creating Tables for PrinterSide--------------------------
use SmartPrinter

CREATE TABLE PrintJob
(
ID int primary key,
UserName varchar(255) Not Null,
FileName varchar(255) Not Null,
NumberOfCopies int Not Null Check(NumberOfCopies > 0),
DelegatedTo varchar(255),
Status int Not Null Check(Status >= 0 and Status <=2),
CreatedDateTime DateTime,
PrintedDateTime DateTime
); 

CREATE TABLE PrintLog
(
ID int primary key,
UserName varchar(255) Not Null,
FileName varchar(255) Not Null,
NumberOfCopies int Not Null Check(NumberOfCopies > 0),
DelegatedBy varchar(255),
PrintedOn DateTime
);

CREATE TABLE Delegate
(
ID int primary key,
UserName varchar(255) Not Null,
TrustedUser varchar(255) Not Null,
Status int Not Null Check(Status >= 0 and Status <=2)
);


----------------------Scripts for Creating Tables for RaspiSide--------------------------
CREATE TABLE Printer
(
ID int primary key,
Name varchar(255),
NoOfPagesPrintedInTotal int,
NoOfPagesPrintedSinceLastReboot int,
Status varchar(50),
StatusMessage varchar(50),
StatusCode int,
Black int,
BlackTotal int,
Yellow int,
YellowTotal int,
Cyan int,
CyanTotal int,
Magenta int,
MagentaTotal int
); 


Create Table PrinterOIDMapping 
(
OID varchar(100) primary key,
Data varchar(500)
);

Insert into PrinterOIDMapping(OID, Data) Values ('1.3.6.1.2.1.43.10.2.1.4.1.1', 'Pages printed in lifetime')
Insert into PrinterOIDMapping(OID, Data) Values ('1.3.6.1.2.1.43.10.2.1.5.1.1', 'Pages printed since last reboot')
Insert into PrinterOIDMapping(OID, Data) Values ('1.3.6.1.2.1.43.11.1.1.8.1.1', 'Black toner max level')
Insert into PrinterOIDMapping(OID, Data) Values ('1.3.6.1.2.1.43.11.1.1.9.1.1', 'Black toner level')
Insert into PrinterOIDMapping(OID, Data) Values ('1.3.6.1.2.1.43.11.1.1.8.1.2', 'Yellow toner max level')
Insert into PrinterOIDMapping(OID, Data) Values ('1.3.6.1.2.1.43.11.1.1.9.1.2', 'Yellow toner level')
Insert into PrinterOIDMapping(OID, Data) Values ('1.3.6.1.2.1.43.11.1.1.8.1.3', 'Cyan toner max level')
Insert into PrinterOIDMapping(OID, Data) Values ('1.3.6.1.2.1.43.11.1.1.9.1.3', 'Cyan toner level')
Insert into PrinterOIDMapping(OID, Data) Values ('1.3.6.1.2.1.43.11.1.1.8.1.4', 'Magenta toner max level')
Insert into PrinterOIDMapping(OID, Data) Values ('1.3.6.1.2.1.43.11.1.1.9.1.4', 'Magenta toner level')
Insert into PrinterOIDMapping(OID, Data) Values ('1.3.6.1.2.1.25.3.5.1.2.1.1', 'Status')

Update PrinterOIDMapping
Set OID='1.3.6.1.2.1.43.16.5.1.2.1.1'
Where Data='Status'

Update PrinterOIDMapping
Set OID='1.3.6.1.2.1.43.10.2.1.4.1.1'
Where Data='Pages printed in lifetime'

Update PrinterOIDMapping
Set OID='1.3.6.1.2.1.43.10.2.1.5.1.1'
Where Data='Pages printed since last reboot'


Create Table PrinterIP
(
ID int primary key IDENTITY(1,1),
IPAddress varchar(100),
PrinterName varchar(1000)
);

Insert Into PrinterIP(IPAddress, PrinterName) values('10.171.34.102', 'B2_HP725_1FWAZ2')

CREATE TABLE NotificationRecepient
(
ID int primary key IDENTITY(1,1),
EmailAddress varchar(255),
PrinterName varchar(255)
); 

Insert into NotificationRecepient(EmailAddress) Values('rajsha@microsoft.com')

CREATE TABLE NotificationSender
(
ID int primary key IDENTITY(1,1),
EmailAddress varchar(255),
Password varchar(255)
);

Insert into NotificationSender(EmailAddress, Password) Values('smart_printer@outlook.com', 'SmartPrinter1')
