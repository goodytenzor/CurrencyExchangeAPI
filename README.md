# CurrencyExchangeStatisticsAPI

Currency Exchange Statistics

The project uses the open API described here --> https://exchangeratesapi.io/

The project's api allows the user to query exchange rates for two currencies and a list of interesting dates and thereby get some
statistics on when the target currency was at its lowest and maximum rates. There is also the statistics on the average.

Prerequisites

Visual Studio 19 with .NET core 3.0 or later.
PostMan v7.x

Usage

Run the project from your Visual Studio instance.
The api will welcome you at the endpoint /api/v1/welcome. If you see the welcome message in the screen then you are good to go!.

Use PostMan to post your queries to the statistics API.

Open PostMan, create a new POST request.
* Set the headers
  Content-Type : application/json
* Set the body
  - Choose the "raw" radio button
  - The body has two parameters
    1. CurrencyTypes
        - The value should contain text that denotes the three letter code of the currency you want to check from followed by the
        symbol -> and then the currency code you want to check against.
        Example: USD->EUR
        
    2. Dates
        - The dates use the Year-Month-Day format. Many dates can be specified with a comma separator.
        Example: 2000-01-01, 2020-02-05, 2015-03-23
        
Example POST request:

{
"CurrencyTypes" : "SEK->NOK",
"Dates" : "2018-01-02,2018-01-03,2018-01-05"
}
        
Versioning

V 0.1

Author

Anand G

License

This project is licensed under the MIT License - see the LICENSE.md file for details

Acknowledgments

Thanks John : https://johnthiriet.com/efficient-api-calls/
