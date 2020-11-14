#include <sys/ioctl.h>
#include "TerminalDisplay.h"

using namespace std;


//  TerminalDisplay
//  constructor
//
TerminalDisplay::TerminalDisplay()
{
	NumberOfRows = 0;
	NumberOfColumns = 0;

	ForegroundColour = WHITE;
	BackgroundColour = BLACK;

}


//  Destructor
//
TerminalDisplay::~TerminalDisplay()
{
	//  reset the display
	system("clear");
}



//  InitDimensions
//
void TerminalDisplay:: InitDimensions()
{
	struct winsize w;
    ioctl(0, TIOCGWINSZ, &w);

    NumberOfRows =  w.ws_row;
    NumberOfColumns = w.ws_col;

	BlankSpaces = "";

	for ( int i = 0; i < NumberOfColumns; i ++ )
	{
		BlankSpaces += " ";
	}

}



//  SetBackground
//
void TerminalDisplay::SetBackground(int colour)
{
	SetColour(colour,colour);

	for ( int i = 0; i < NumberOfRows; i++ )
		PrintLine(BlankSpaces);

	//  back up to the beginning
	for ( int i = 0; i < NumberOfRows; i++ )
		fputs("\033[A\033[2K",stdout);
	rewind(stdout);

	BackgroundColour = colour;
}



//  SetColour
//
void TerminalDisplay:: SetColour(int fg, int bg)
{
	textcolour(BRIGHT, fg, bg);	
}



//  SetColour
//
void TerminalDisplay:: SetColour(int attributes, int fg, int bg)
{
	textcolour(attributes, fg, bg);	
}


//  PrintLine
//
void TerminalDisplay::PrintLine(std::string lineToPrint, int fg, int bg)
{
	textcolour(BRIGHT,fg,bg);
	PrintLine(lineToPrint);
}



//  PrintLine
//
void TerminalDisplay::PrintLine(std::string bookends, std::string lineToPrint, int fg, int bg)
{
	textcolour(BRIGHT,fg,bg);
	PrintLine(bookends,lineToPrint);
}


//  PrintLine
//
void TerminalDisplay:: PrintLine(std::string lineToPrint)
{
	PrintLine("",lineToPrint);
}


//  PrintLine
//
void TerminalDisplay:: PrintLine(std::string bookends, std::string lineToPrint)
{
	int blankCount = NumberOfColumns - (bookends.size()*2 + lineToPrint.size());
	int printableSize = (NumberOfColumns - 2*bookends.size());

	//  if blank count is less than 0, we have to wrap the line
	if ( blankCount < 0 )
	{
		std::string nextLineToPrint = lineToPrint;

		//  print first part of the line
		std::string nextLine = nextLineToPrint.substr(0, printableSize);
		nextLineToPrint = nextLineToPrint.substr(printableSize);

		printf("%s%s%s\n", bookends.c_str(), nextLine.c_str(), bookends.c_str());

		while ( blankCount < 0 )
		{
			//  make the next line with a " -", we will subtract 2 from printaable size for this
			nextLine = " -" + nextLineToPrint.substr(0, printableSize-2);
			if ( nextLineToPrint.size() > (printableSize-2) )
			{
				//  we are still wrapping
				nextLineToPrint = nextLineToPrint.substr(printableSize-2);
				printf("%s%s%s\n", bookends.c_str(), nextLine.c_str(), bookends.c_str());
			}
		
			//  see how many chars to end of line left
			blankCount = NumberOfColumns - (bookends.size()*2 + nextLineToPrint.size());
		}

		//  print any remaining part of the line, also with the " -" prefix
		if ( nextLineToPrint.size() != 0 )
		{
			nextLine = " -" + nextLineToPrint;
			printf("%s%s%s%s", bookends.c_str(), nextLine.c_str(),  BlankSpaces.substr(0, blankCount-2).c_str(), bookends.c_str());
		}
		
		printf("\n");
	}
	else
	{
		//  print the line padded with blank spaces
		printf("%s%s%s%s", bookends.c_str(), lineToPrint.c_str(), BlankSpaces.substr(0, blankCount).c_str(), bookends.c_str());

		printf("\n");
	}
}



//  PrintBookendLine
//
void TerminalDisplay::PrintBookendLine(std::string bookends, int bookendFg, int bookendBg, std::string lineToPrint, int lineFg, int lineBg)
{
	int blankCount = NumberOfColumns - (bookends.size()*2 + lineToPrint.size());

	textcolour(BRIGHT, bookendFg, bookendBg);
	printf("%s", bookends.c_str());

	textcolour(BRIGHT, lineFg, lineBg);
	printf("%s%s", lineToPrint.c_str(), BlankSpaces.substr(0, blankCount).c_str());

	textcolour(BRIGHT, bookendFg, bookendBg);
	printf("%s", bookends.c_str());

	SetColour(BLACK,BackgroundColour);
	
	printf("\n");
}



//  PrintLine
//
void TerminalDisplay::PrintLine(std::string bookends, int bookendFg, int bookendBg, std::string lineToPrint, int lineFg, int lineBg)
{
	int blankCount = NumberOfColumns - (bookends.size()*2 + lineToPrint.size());
	int printableSize = (NumberOfColumns - 2*bookends.size());

	if ( blankCount < 0 )
	{
		std::string nextLineToPrint = lineToPrint;

		//  print the first part of the line
		std::string nextLine = nextLineToPrint.substr(0, printableSize);
		nextLineToPrint = nextLineToPrint.substr(printableSize);

		PrintBookendLine(bookends, bookendFg, bookendBg, nextLine, lineFg, lineBg);

		while ( blankCount < 0 )
		{
			//  make the next line with a " -", we will subtract 2 from printaable size for this
			nextLine = " -" + nextLineToPrint.substr(0, printableSize-2);
			if ( nextLineToPrint.size() > (printableSize-2) )
			{
				//  we are still wrapping
				nextLineToPrint = nextLineToPrint.substr(printableSize-2);
				PrintBookendLine(bookends, bookendFg, bookendBg, nextLine.c_str(), lineFg, lineBg);
			}

			//  see how many chars to end of line left
			blankCount = NumberOfColumns - (bookends.size()*2 + nextLineToPrint.size());
		}

		//  print any remaining part of the line, also with the " -" prefix
		if ( nextLineToPrint.size() != 0 )
		{
			nextLine = " -" + nextLineToPrint;
			PrintBookendLine(bookends, bookendFg, bookendBg, nextLine, lineFg, lineBg);
		}
	}
	else
	{
		//  print the line with no wrapping
		PrintBookendLine(bookends, bookendFg, bookendBg, lineToPrint, lineFg, lineBg);
	}
}


//  PrintAcross
//
void TerminalDisplay::PrintAcross(std::string charsToPrint, int fg, int bg)
{

	textcolour(BRIGHT, fg, bg);

	int numberPrinted = 0;

	while ( numberPrinted < NumberOfColumns )
	{
		int sizeToPrint = ( NumberOfColumns - numberPrinted ) > charsToPrint.size() ? charsToPrint.size() : (NumberOfColumns - numberPrinted);
		printf("%s",charsToPrint.substr(0,sizeToPrint).c_str());
		numberPrinted += sizeToPrint;
	}

	SetColour(BLACK,BackgroundColour);

	printf("\n");
}


//  PrintAcross
//
void TerminalDisplay::PrintAcross(string bookends, int bookendFg, int bookendBg, string charsToPrint, int fg, int bg)
{
	int countAcross = NumberOfColumns - (bookends.size()*2);
	
	SetColour(bookendFg, bookendBg);
	printf("%s", bookends.c_str());

	SetColour(fg,bg);

	int numberPrinted = 0;
	while ( numberPrinted < countAcross )
	{
		int sizeToPrint = ( countAcross - numberPrinted ) > charsToPrint.size() ? charsToPrint.size() : (countAcross - numberPrinted);
		printf("%s",charsToPrint.substr(0,sizeToPrint).c_str());
		numberPrinted += sizeToPrint;
	}

	SetColour(bookendFg, bookendBg);
	printf("%s", bookends.c_str());

	SetColour(BLACK,BackgroundColour);

	printf("\n");
}
