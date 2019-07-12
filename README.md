# DynamicsScheduledTask
Dynamics CRM Scheduled Task Console Application

Customizations:
 
Create new Entity Registration Status          
•	Text field called Name - required  
•	Create record with Name “Open”  
•	Create record with Name “Cancelled”  
•	Create record with Name “Closed”  
 
Create new Entity Called Registration SubStatus  
•	Text field called “Name” - required  
•	Create record with name “Registered”  
•	Crete record with name “Cancelled by Task”  
 
Create new Entity Called Registrations  
•	Text field called “Name” - required  
•	Lookup field called Registration status, linked to registration Status entity  
•	Lookup field called Registration substatus, that is linked to registration SubStatus entity   
•	Lookup field that called account, that is linked to the account entity  
•	Number field called “Priority”  
•	Lookup field called “Modified By” that is linked to the User entity  
•	Date time field called “Modified On”
 
Add Date field to the System rule entity you created in the previous task called “Date to Run”.  
Create new record in the system rule called Date to run registration scheduled task (with a slug)   
and fill the date to be the day you want the task to run (update it every day you want to run the task)  
Create new record in the system rule called “Registration scheduled task owners” with rule value = “email1, email2, email3, etc.”
 
Task Logic:
 
•	The task will run daily but will continue only if today's date matches the date in the “date to run” system rule record  
•	The task shall load data offline before execution and minimize connections to the database.  
•	Get all of the contacts that have account linked to them, that have at least one registration 
with Open status and Registered sub status  
•	For each account found, find a case that is with case type = Request and case title  = Cancel one registration  
•	For each case found Update a registration with the lowest priority, 
if two registrations found with the same priority take the last created one 

The update will be:  
•	Registration status to be cancelled  
•	Registration sub status to be cancelled by task  
•	Modified by - the user you are using to run the task  
•	Modified on - current time  

Resolve the related case  
Send email template to the related contact with the registrations updated  
Send a report to the emails found in the system rule called Registration scheduled task owners with the following data:  
 • Total number of registrations updated  
 • Total number of issues found (for example issue on update the registration, issue on sending the email etc.)  
 • Table of registration updated successfully with the following data: Account name, Registration name, Case name  
 • Table of issues with the following data: Account name, Registration name, Issue description  
 
Please note that account entity might have more than 5000 records.
