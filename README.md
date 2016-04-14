# MsSQLKit. Microsoft SQL Server Kit for sublime text 3

MsSQLKit is a sublime text 3 plugin providing interaction with an SQLServer database.

## Features

- Interactive query execution
- Autocompletion

## Concept

MsSQLKit consist of a winform application, written in dotnet c#. It act as a dataBase IO layer between sublime text and SQLServer and provide a gui for data visaulization.
Communication between Python and .net is done via namedPipe.

## Requierment
- pywin32.setup plugin for sublime text
- dotnet 4.5 framework [TODO, should work with 4.0]
- Some SQLServer Management Assembly :
  + Microsoft.SqlServer.Management.SmoMetadataProvider and Microsoft.SqlServer.Management.SqlParser assembly, available in "Microsoft® SQL Server® Transact-SQL Language Service". tsqllanguageservice.msi 
  + Microsoft.SqlServer.Management.sdk.sfc available in Microsoft® SQL Server® 2014 Shared Management Objects SharedManagementObjects.msi
  + Those 2 msi are available in the Microsoft SQLServer Feature Pack here https://www.microsoft.com/en-us/download/details.aspx?id=42295

## Planned/Possible features/amelioration
- Linter
- Object search
- Grid Action (copy with header/copy headers/export/script)
- SqlAgent management (sqljob view/restart etc.)
- Diff with database
- crypt and possibliy compress the communication between Sublime text and .net

## Usage

- Edit your prefered configuration in MsSQLKit.sublime-settings
- Ctrl-F5 : connect to a database
- F5 : Execute the selected text, or all the file if nothing is selected
- F1 : sp_help selectioned object
- F2 : Script the selected object in a new tab

