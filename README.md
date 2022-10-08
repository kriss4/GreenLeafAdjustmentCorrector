# GreenLeafAdjustmentCorrector
GreenLeaf Adjustments for failed Invoiced. Extract input json/Collect data from BioTrack and Cova/Adjust BioTrack Qty via their API 

# User Secret json shape needed for PROD call
{
	"ProcessingInput": {
		"ClientID": "<from Cova.Tracability web.config>",
		"ClientSecret": "<from Cova.Tracability web.config>",

		"CompanyId": <PROD Company ID>,
		"LocationId": <PROD Location ID>,

		"LicenseNumber": "<BioTrack info from Azure>",
		"UserName": "<BioTrack info from Azure>",
		"Password": "<BioTrack info from Azure>",

		"AccessToken": "<This is a Client Token - see below>",

		"AvailabilityBaseUrl": "<Availability Service URL>",
		"AccountsBaseUrl": "Accounts Service URL"
	}
}

Client Token can be found via loging to Hub with credentians for the company you are interested in. Via Chrome/DevTool get the Token and set under AccessToken.

