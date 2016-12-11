import adal
import requests

context = adal.AuthenticationContext('https://login.microsoftonline.com/microsoft.onmicrosoft.com')
token = context.acquire_token_with_client_credentials(
    "https://employeeInformationAPI",
    "94d2abb1-45f6-4efb-b3c6-7038a6d7c102", 
    "UlON495DQNspjv/ESeqhTwuv9DaC4glr7R0Zvm/80Jw=")

access_token = token.get('accessToken')
bearerToken = "Bearer " + access_token
headers = {"Authorization": bearerToken}
cachedIds = {}

while True:	
	cardNumber = raw_input()
	userAlias = ''
	if cardNumber not in cachedIds:
		payload = {'cardNumber' : cardNumber}	
		userAlias = requests.get('https://azcardsw01.redmond.corp.microsoft.com/api/GetEmailId', params=payload, headers=headers)
		cachedIds[cardNumber] = userAlias
	else:
		userAlias = cachedIds[cardNumber]
		#Make service call for getting the documents to be printed
	userPayload = {'alias' : userAlias}
	requests.get('Service URL', payload=userPayload, headers=headers)
		

