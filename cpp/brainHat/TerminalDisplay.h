#pragma once
#include <stdio.h>
#include <string>

//  Terminal Display Class
//  neat little class to wrap up using colours on terminal stdout
//
//  this class was built on the information provided here:
//  -  http://www.linuxjournal.com/article/8603
//
//  for a deeper understanding of ascii escape codes, see here:
//  - http://en.wikipedia.org/wiki/ANSI_escape_code#graphics
//


//  Attribute / Colour Definitions
#define RESET		0
#define BRIGHT 		1
#define DIM			2
#define UNDERLINE 	3
#define BLINK		4
#define REVERSE		7
#define HIDDEN		8
//
#define BLACK 		0
#define RED			1
#define GREEN		2
#define YELLOW		3
#define BLUE		4
#define MAGENTA		5
#define CYAN		6
#define	WHITE		7


//  Set the attribute and colour to the terminal
void inline textcolour(int attr, int fg, int bg)
{
	char command[13];

	/* Command is the control command to the terminal */
	sprintf(command, "%c[%d;%d;%dm", 0x1B, attr, fg + 30, bg + 40);
	printf("%s", command);
}



//  TerminalDisplay 
//  class to wrap up colour and size management of terminal stdout display
//
class TerminalDisplay
{
public:  

	TerminalDisplay();
	virtual ~TerminalDisplay();

	//  initialize the dimensions of the terminal
	void InitDimensions();

	//  set the entire terminal to the colour specified, then rewind file pointer to beginning
	void SetBackground(int colour);
	 
	//  set the foreground and background colour
	void SetColour(int fg, int bg);

	//  set the foreground and background colour with attribute
	void SetColour(int attributes, int fg, int bg);

	//  print a single line in the current colour
	//  line will be wrapped with a " -" if it is larger than terminal dimensions
	void PrintLine(std::string lineToPrint);

	//  print a single line with bookends in the current colour
	//  line will be wrapped with a " -" if it is larger than terminal dimensions
	void PrintLine(std::string bookends, std::string lineToPrint);

	//  print a single line in the specified colour
	//  line will be wrapped with a " -" if it is larger than terminal dimensions
	void PrintLine(std::string lineToPrint, int fg, int bg);
	
	//  print a single line with bookends in the specified colour
	//  line will be wrapped with a " -" if it is larger than terminal dimensions
	void PrintLine(std::string bookends, std::string lineToPrint, int fg, int bg);

	//  print a single line with bookends in the specified colour
	//  line will be wrapped with a " -" if it is larger than terminal dimensions
	void PrintLine(std::string bookends, int bookendFg, int bookendBg, std::string lineToPrint, int lineFg, int lineBg);

	//  print the charsToPrint sequence across the screen in the specified colours
	//  if sequence is longer than 1 char, it may be truncated for last repetition before end
	void PrintAcross(std::string charsToPrint, int fg, int bg);

	//  print the charsToPrint sequence across the screen with bookends in the specified colours
	//  if sequence is longer than 1 char, it may be truncated for last repetition before end
	void PrintAcross(std::string bookends, int bookendFg, int bookendBg, std::string charsToPrint, int fg, int bg);

protected:


	void PrintBookendLine(std::string bookends, int bookendFg, int bookendBg, std::string lineToPrint, int lineFg, int lineBg);


	std::string BlankSpaces;

	int NumberOfRows;
	int NumberOfColumns;

	int ForegroundColour;
	int BackgroundColour;
};


