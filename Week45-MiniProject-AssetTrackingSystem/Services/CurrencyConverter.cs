using System;
using System.Collections.Generic;
using System.Text;
using Week45_MiniProject_AssetTrackingSystem.Models;

namespace Week45_MiniProject_AssetTrackingSystem.Services
{
    // Handles everything to do with currencies:
    // which currency each office uses, and converting from EUR.
    //
    // "static" means we never create a CurrencyConverter object with "new" -
    // we call the methods directly: CurrencyConverter.ConvertFromEur(...)
    //
    // The rest of the program only talks to this class, so when we switch
    // from hardcoded rates to the teacher's API code, only this file changes.
    public static class CurrencyConverter
    {
        // Hardcoded exchange rates: how much 1 EUR is worth in each currency.
        // These are approximate values - they will be replaced by live rates from an API.
        // A Dictionary stores key/value pairs: here Currency -> rate.
        private static readonly Dictionary<Currency, decimal> RatesFromEur = new Dictionary<Currency, decimal>
        {
            { Currency.EUR, 1.00m },
            { Currency.SEK, 11.00m },
            { Currency.NOK, 11.70m }
        };

        // Which currency each office uses
        public static Currency GetCurrencyForOffice(Office office) => office switch
        {
            Office.Helsingborg => Currency.SEK,
            Office.Oslo => Currency.NOK,
            Office.Mariehamn => Currency.EUR,
            // Throwing an exception here means we find out immediately
            // if someone adds a new office but forgets to give it a currency.
            _ => throw new ArgumentOutOfRangeException(nameof(office), $"No currency set for office {office}")
        };

        // Converts an amount in EUR to the given currency
        public static decimal ConvertFromEur(decimal amountEur, Currency toCurrency)
        {
            decimal rate = RatesFromEur[toCurrency];

            // Round to 2 decimals, like real money
            return Math.Round(amountEur * rate, 2);
        }
    }
}
