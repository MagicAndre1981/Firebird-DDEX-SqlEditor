#pragma once
#include "pch.h"
#include "DslOptions.h"
#include "AbstractParser.h"


using namespace C5;


namespace BlackbirdDsl {


public ref class Parser : public AbstractParser
{

public:

	property bool ConsistentSubtrees
	{
		bool get() { return DslOptions::ConsistentSubtrees(_Options); }
	};

	property bool AnsiQuotes
	{
		bool get() { return DslOptions::AnsiQuotes(_Options); }
	};

	property bool OffsetCapture
	{
		bool get() { return DslOptions::OffsetCapture(_Options); }
	};

	Parser() : AbstractParser()
	{
	};

	Parser(FlagsOptions options) : AbstractParser(options)
	{
	};



	virtual StringCell^ Execute(SysStr^ sql) override;

	virtual StringCell^ Parse(StringCell^ root) override;

	/*
	* Add a custom function to the parser.  no return value
	*
	* @param SysStr $token The name of the function to add
	*
	* @return null
	*/
	/*
	AddCustomFunction($token) {
		PHPSQLParserConstants::getInstance()->addCustomFunction($token);
	}
	*/

	/**
	* Remove a custom function from the parser.  no return value
	*
	* @param SysStr $token The name of the function to remove
	*
	* @return null
	*/
	/*
	public function removeCustomFunction($token) {
		PHPSQLParserConstants::getInstance()->removeCustomFunction($token);
	}
	*/

	/**
	* Returns the list of custom functions
	*
	* @return array Returns an array of all custom functions
	*/
	/*
	public function getCustomFunctions() {
		return PHPSQLParserConstants::getInstance()->getCustomFunctions();
	}
	*/

};


}