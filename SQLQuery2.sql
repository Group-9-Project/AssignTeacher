USE [SchoolTimetable];
GO
UPDATE EntranceTests
SET 
    QuetionOne = 'What is the value of x in the equation: 2x=5=11',
    QuetionTwo = 'A book costs R150. If a 10% discount is applied, what is the new price',
    QuetionThree = 'What is the perimeter of a rectangle with length 8cm and width 5 cm',
    QuetionFour = 'Solve for y: y-3=7',
    QuetionFive = 'A car travels 250km in 5 hours. what is its average speed'
WHERE TestId = 1;
GO