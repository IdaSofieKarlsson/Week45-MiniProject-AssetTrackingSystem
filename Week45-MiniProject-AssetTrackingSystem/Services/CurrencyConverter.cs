using System;
using System.Collections.Generic;
using System.Text;
// System.Net.Http.Json gives us GetFromJsonAsync (HttpClient + JSON in one step).
// System.Text.Json gives us JsonException (thrown if the JSON is broken).
using System.Net.Http.Json;
using System.Text.Json;
using Week45_MiniProject_AssetTrackingSystem.Models;

namespace Week45_MiniProject_AssetTrackingSystem.Services
{
    // Handles everything to do with currencies:
    //   1. Fetching live exchange rates from the Frankfurter API (ECB data)
    //   2. Knowing which currency each office uses
    //   3. Converting prices from EUR to a local currency

    // "static" means we never create a CurrencyConverter object with "new".
    // There is only ONE set of exchange rates in the whole program, and we
    // call the methods directly on the class:
    //     CurrencyConverter.ConvertFromEur(1500m, Currency.SEK)

    // FLOW WHEN THE PROGRAM STARTS:
    //   Program.cs calls LoadRatesAsync() once
    //     -> success: live rates replace the backup rates
    //     -> failure: the backup rates stay, and the program still works

    public static class CurrencyConverter
    {
        // Frankfurter is free, needs no API key, and uses the European Central Bank's (ECB) official daily rates, with EUR as base.
        private const string ApiBaseUrl = "https://api.frankfurter.app/latest";

        // HttpClient - C#'s equivalent of fetch() in JavaScript
        // HttpClient sends HTTP requests (GET, POST, ...) and receives responses.
        //
        // IMPORTANT: create ONE HttpClient and reuse it for the whole program.
        // Creating a new one for every request can use up the computer's
        // network connections. "static readonly" means: one shared instance,
        // created once, never replaced.
        private static readonly HttpClient Client = new HttpClient
        {
            // If the API has not answered within 10 seconds, give up.
            // Without this the program could hang for a long time (the default timeout is 100 seconds).
            Timeout = TimeSpan.FromSeconds(10)
        };

        // Exchange rates: how much 1 EUR is worth in each currency
        // A Dictionary stores key/value pairs, like a JS object or Map:
        //     Currency.SEK -> 11.00
        //
        // These starting values are BACKUP rates (approximate). They are
        // only used if the API cannot be reached. When LoadRatesAsync()
        // succeeds, this whole dictionary is replaced with live rates.
        //
        // Not "readonly" this time, because we replace it after the API call.
        private static Dictionary<Currency, decimal> ratesFromEur = new Dictionary<Currency, decimal>
        {
            { Currency.EUR, 1.00m },  // EUR -> EUR is always 1
            { Currency.SEK, 11.00m },
            { Currency.NOK, 11.70m }
        };

        // Information about the rates, for Program.cs to display
        // "{ get; private set; }" = anyone can READ the value,
        // but only code inside this class can CHANGE it.

        // True if live rates were fetched, false if backup rates are used
        public static bool IsUsingLiveRates { get; private set; } = false;

        // The date the ECB published the rates (null = no live rates)
        // "DateTime?" (with ?) means the value is allowed to be null.
        public static DateTime? RatesDate { get; private set; } = null;

        // If the API call failed: a short explanation of why
        public static string? ErrorMessage { get; private set; } = null;

        // LoadRatesAsync - fetches live exchange rates from the API
        // ASYNC / AWAIT (same idea as in JavaScript):
        // Calling an API takes time - maybe 0.2 seconds, maybe 10.
        // "async" marks a method that contains "await".
        // "await" means: wait here until the result has arrived, without
        // freezing the rest of the program.
        //
        // RETURN TYPE "Task":
        // In JavaScript an async function returns a Promise.
        // In C# an async method returns a Task:
        //     Task        = Promise<void>   (no value returned)
        //     Task<int>   = Promise<number> (returns an int)
        // The caller writes "await CurrencyConverter.LoadRatesAsync();"
        //
        // NAMING: async methods end in "Async" by C# convention.

        public static async Task LoadRatesAsync()
        {
            // ----------------------------------------------------------
            // Step 1: Build the URL
            // ----------------------------------------------------------
            // We want rates for every currency in our Currency enum except
            // EUR (EUR is the base, so its rate is always 1).
            // Enum.GetValues<Currency>() returns all values: EUR, SEK, NOK.
            // If we add a new currency to the enum later, the URL updates
            // automatically - no need to change this code.
            List<Currency> targetCurrencies = Enum.GetValues<Currency>()
                .Where(currency => currency != Currency.EUR)
                .ToList();

            // string.Join puts a comma between the items: "SEK,NOK"
            string currencyCodes = string.Join(",", targetCurrencies);

            // The finished URL:
            // https://api.frankfurter.app/latest?from=EUR&to=SEK,NOK
            // ("$" before a string lets us insert values with {...},
            //  just like `${...}` template literals in JavaScript)
            string url = $"{ApiBaseUrl}?from=EUR&to={currencyCodes}";

            // ----------------------------------------------------------
            // Step 2: Call the API - inside try/catch
            // ----------------------------------------------------------
            // Lots of things outside our control can go wrong: no internet,
            // the server is down, the request times out, the JSON is broken.
            // When that happens C# THROWS an exception. Without try/catch the
            // whole program would crash. With try/catch we "catch" the error
            // and keep running with the backup rates instead.
            try
            {
                // GetFromJsonAsync does three things in one line:
                //   1. Sends a GET request to the URL
                //   2. Checks that the response is a success (status code 2xx)
                //   3. Deserialises the JSON body into a FrankfurterResponse
                //
                // The JavaScript equivalent would be:
                //   const response = await fetch(url);
                //   const data = await response.json();
                //
                // The <FrankfurterResponse> part tells C# which class to
                // fill with the JSON data. The "?" after the type means the
                // result might be null (e.g. if the body was "null").
                FrankfurterResponse? response = await Client.GetFromJsonAsync<FrankfurterResponse>(url);

                // ------------------------------------------------------
                // Step 3: Check the data before trusting it
                // ------------------------------------------------------
                // Even if the call itself worked, the content could be
                // missing or incomplete. "is null" is the C# way to check
                // for null.
                if (response is null)
                {
                    ErrorMessage = "The API returned no data.";
                    return; // Leave the method - backup rates stay in place
                }

                // Build the NEW set of rates in a separate dictionary first.
                // Only when everything is OK do we swap it in. That way we
                // never end up with half live rates and half backup rates.
                Dictionary<Currency, decimal> newRates = new Dictionary<Currency, decimal>
                {
                    { Currency.EUR, 1.00m }
                };

                foreach (Currency currency in targetCurrencies)
                {
                    // The API uses text keys ("SEK"), our code uses the enum
                    // (Currency.SEK). currency.ToString() turns the enum into
                    // the text "SEK" so we can look it up.
                    //
                    // TryGetValue tries to find the key:
                    //   found     -> returns true, and puts the value in "rate"
                    //   not found -> returns false (no crash)
                    if (response.Rates.TryGetValue(currency.ToString(), out decimal rate))
                    {
                        newRates[currency] = rate;
                    }
                    else
                    {
                        ErrorMessage = $"The API did not return a rate for {currency}.";
                        return; // Backup rates stay in place
                    }
                }

                // ------------------------------------------------------
                // Step 4: Everything worked - start using the live rates
                // ------------------------------------------------------
                ratesFromEur = newRates;
                RatesDate = response.Date;
                IsUsingLiveRates = true;
                ErrorMessage = null;
            }
            // ----------------------------------------------------------
            // The catch blocks: one per kind of problem
            // ----------------------------------------------------------
            // C# checks them from top to bottom and runs the FIRST one that
            // matches the exception type. "ex" is the exception object,
            // and ex.Message contains a description of what went wrong.
            catch (HttpRequestException ex)
            {
                // No internet, server not found, or an error status code
                // from the server (e.g. 404 Not Found, 500 Server Error)
                ErrorMessage = $"Could not reach the currency API ({ex.Message}).";
            }
            catch (TaskCanceledException)
            {
                // The request took longer than the 10-second Timeout
                ErrorMessage = "The currency API did not answer in time.";
            }
            catch (JsonException)
            {
                // The response was not valid JSON, or did not match
                // the shape of FrankfurterResponse
                ErrorMessage = "The currency API sent data in an unexpected format.";
            }
            // Note: we do NOT catch "Exception" (all errors). Catching only
            // the errors we expect means real bugs in our own code still
            // show up instead of being silently hidden.
        }

        // ==============================================================
        // GetCurrencyForOffice - which currency each office uses
        // ==============================================================
        // A switch expression picks one result based on the value.
        public static Currency GetCurrencyForOffice(Office office) => office switch
        {
            Office.Helsingborg => Currency.SEK,
            Office.Oslo => Currency.NOK,
            Office.Mariehamn => Currency.EUR,
            // Throwing an exception here means we find out immediately
            // if someone adds a new office but forgets to give it a currency.
            _ => throw new ArgumentOutOfRangeException(nameof(office), $"No currency set for office {office}")
        };

        // ==============================================================
        // ConvertFromEur - converts an amount in EUR to another currency
        // ==============================================================
        // This method is NOT async: the rates are already loaded into the
        // dictionary, so converting is just a quick multiplication.
        public static decimal ConvertFromEur(decimal amountEur, Currency toCurrency)
        {
            decimal rate = ratesFromEur[toCurrency];

            // Round to 2 decimals, like real money
            return Math.Round(amountEur * rate, 2);
        }

        // Returns the rate for one currency, so Program.cs can display it
        public static decimal GetRate(Currency currency) => ratesFromEur[currency];
    }
}
