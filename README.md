 ```
Author:		Victoria Locke
Partner:	Sasha Rybalkina
Start Date: 17-Jan-2023
Course:		CS 3500, University of Utah, School of Computing
GitHub ID:	u1366757
Repo:		https://github.com/uofu-cs3500-spring23/spreadsheet-u1366757
Commit Date: 3-March-2023 2:23PM
Solution:	Spreadsheet
Copyright:	CS 3500 and Victoria Locke - This work may not be copied for use in 
Academic Coursework
```

# Overview of the Spreadsheet functionality

The Spreadsheet program is currently capable of producing a visual spreadsheet model. Internally,
the program can set cell values of type double, string, and a Formula object. It evaluates 
expressions between doubles and variables which correspond to double values and handles the 
dependencies of the cells by tracking the relationships between the dependents and dependees. 
The cells are visually updated to show their values when they are not focused on and their
contents when they are focused on (by being clicked on). The widgets at the top of the spreadsheet
also show the cell name, value, and contents, when the cell is focused on. It is capable of 
determining, adding, replacing, and removing dependent and dependee variables to a dependency graph.
It can determine if the formula expressions are valid before beginning the evaluation 
process.These errors are thrown when a cell has a null name, there is a circular dependency, a 
value input is null, an expression that is entered is not complete, there is a null variable 
value or null cell name, division by 0 occurs, or if a formula contains an invalid token. In the 
visual spreadsheet respresentation, however, those errors do not cause it to crash rather, an error
window pops up or a warning window pops up. After all of these actions are completed, it can save the 
Spreadsheet contents to an XML file, load previously saved files, or clear the spreadsheet and begin
a new one.

# Time Expenditures:
Hours Estimated/Worked		Assignment							Notes
	10	/	15			- Assignment 1 - Formula Evaluator		N/A
	11	/	11			- Assignment 2 - Dependency Graph		Spent awhile wrapping my head around the add method 
																(difference between dependee & dependent)
	11	/	12			- Assignment 3 - Formula				Debugging & writing tests took about 6 out of the 12 hours
	10	/	13			- Assignment 4 - Spreadsheet			Took awhile to figure out what the Visit method did and how
	13	/	19			- Assignment 5 - Complete Spreadsheet	XML files were very difficult to figure out	to test it 
																(prevented me from achieving 100% line/branch coverage)
	14	/	14			- Assignment 6 - Spreadsheet Front-End	Had an issue figuring out how to get the git team repo to work