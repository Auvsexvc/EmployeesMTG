# MTG
1.	MTGWebApi - Backend application serving REST API ASP.NET Core 6 using file as data storage ( because of task constraints 😉)
2.	MTGWebApiTests - API test project including sequence of unit tests using NUnit
3.	MTGWebUI - Beautiful and functional frontend application ASP.NET Core 6 MVC (no third party packages) compliant with task description with some additional features

Implement an application that displays a table of people containing the following information: First Name,Last Name,Street Name,House Number,Apartment Number (optional),Postal Code,Town,Phone Number,Date of Birth,Age (read-only)

2 Applications:backend serving REST API, frontend application showing UI with all the necessary information as described below.

Complete application (FE and BE) should allow a user to edit the data, add new users and delete existing ones. The data should be persisted on disk - it can be a file or local DB, but the application must be self-contained (no external DB engines or libraries required). Below the table, there should be two buttons: "Save" and "Cancel". When the Save button is pressed changes made by a user should be persisted on disk. Pressing the "Cancel" button discards user changes and causes the table to be refreshed based on persisted already data. The buttons should be active only if the table contains unsaved data. After the first startup of the application, the table should be empty.
