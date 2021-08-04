# Salesforce API samples

 A few examples of using Salesforce API
 
 - Run Plain Query: gets SOQL query result from Salesforce database
 - Get Objects: gets [list of Api objects](https://developer.salesforce.com/docs/atlas.en-us.api_tooling.meta/api_tooling/reference_objects_list.htm) from tooling API
 - Get Descriptions: gets list of database objects names 
 - Get Attachments: gets list of files attached to the first contact fetched from contacts table
 - Upload attachment: attaches a file to first contact fetched (<4Mb)
 
 
 # Configuration settings:
  
  - SalesforceLoginEndpoint:\
  login endpoint to get access token from Salesforce Api along with access token response contains instance url for requests
  looks like "https://\[xxxxx\].salesforce.com/services/oauth2/token" (no trailing strike)
  - SalesforceServicesPath:\
  a part of Api services path containing Salesforce Api version
  looks like "/services/data/v52.0"
  
  - SalesforceClientId:     
  - SalesforceClientSecret:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Credentials used for Authentication request
  - SalesforceUserName:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;(all fields mandatory)
  - SalesforcePassword:      

