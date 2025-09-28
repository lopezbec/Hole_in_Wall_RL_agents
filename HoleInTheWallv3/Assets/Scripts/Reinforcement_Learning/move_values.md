movement types:



0 = move LEFT **hand				param: x pos, y pos, z pos**

1 = move RIGHT **hand				param: x pos, y pos, z pos**

2 = rotate **hips					param: x angle, y angle, z angle**

3 = move LEFT **leg				param: x pos, y pos, z pos**

4 = move RIGHT **leg				param: x pos, y pos, z pos**

5 = move **body					param: x pos, y angle, z pos**



\*\*Each entry in move\_direction.csv should follow:

movement type, param 1, param 2, param 3\*\*





### 

### About the files in this folder



move\_values.md : markdown file detailing how to perform movements for the values placed in move\_directions.csv



final\_position.csv : stores the final positions of the targets controlling the limbs for a level.



move\_directions.csv : the file that commands the avatar to move its limbs.



saved\_positions.csv : file that stores poses that can be copied and pasted into move\_directions.csv to perform 

&nbsp;		      the saved pose.



Energy Expenditure Poses.xlsx : spreadsheet that contains the test results for each pose when testing the 				energy expenditure function that determines the "difficulty" of a pose.

