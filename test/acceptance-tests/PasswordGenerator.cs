namespace Splunk.Client.AcceptanceTests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

public class PasswordGenerator
{
    public int MinimumLengthPassword { get; private set; }
    public int MaximumLengthPassword { get; private set; }
    public int MinimumLowerCaseChars { get; private set; }
    public int MinimumUpperCaseChars { get; private set; }
    public int MinimumNumericChars { get; private set; }
    public int MinimumSpecialChars { get; private set; }

    public static string AllLowerCaseChars { get; private set; }
    public static string AllUpperCaseChars { get; private set; }
    public static string AllNumericChars { get; private set; }
    public static string AllSpecialChars { get; private set; }
    private readonly string _allAvailableChars;

    private readonly RandomSecureVersion _randomSecure = new();
    private readonly int _minimumNumberOfChars;

    static PasswordGenerator()
    {
        // Define characters that are valid and reject ambiguous characters such as ilo, IO and 1 or 0
        AllLowerCaseChars = GetCharRange('a', 'z', exclusiveChars: "ilo");
        AllUpperCaseChars = GetCharRange('A', 'Z', exclusiveChars: "IO");
        AllNumericChars = GetCharRange('2', '9');
        AllSpecialChars = "!@#%*()$?+-=";

    }

    public PasswordGenerator(
        int minimumLengthPassword = 15,
        int maximumLengthPassword = 20,
        int minimumLowerCaseChars = 2,
        int minimumUpperCaseChars = 2,
        int minimumNumericChars = 2,
        int minimumSpecialChars = 2)
    {
        if (minimumLengthPassword < 15)
        {
            throw new ArgumentException("The minimumlength is smaller than 15.",
                nameof(minimumLengthPassword));
        }

        if (minimumLengthPassword > maximumLengthPassword)
        {
            throw new ArgumentException("The minimumLength is bigger than the maximum length.",
                nameof(minimumLengthPassword));
        }

        if (minimumLowerCaseChars < 2)
        {
            throw new ArgumentException("The minimumLowerCase is smaller than 2.",
                nameof(minimumLowerCaseChars));
        }

        if (minimumUpperCaseChars < 2)
        {
            throw new ArgumentException("The minimumUpperCase is smaller than 2.",
                nameof(minimumUpperCaseChars));
        }

        if (minimumNumericChars < 2)
        {
            throw new ArgumentException("The minimumNumeric is smaller than 2.",
                nameof(minimumNumericChars));
        }

        if (minimumSpecialChars < 2)
        {
            throw new ArgumentException("The minimumSpecial is smaller than 2.",
                nameof(minimumSpecialChars));
        }

        this._minimumNumberOfChars = minimumLowerCaseChars + minimumUpperCaseChars +
                                minimumNumericChars + minimumSpecialChars;

        if (minimumLengthPassword < this._minimumNumberOfChars)
        {
            throw new ArgumentException(
                "The minimum length of the password is smaller than the sum " +
                "of the minimum characters of all catagories.",
                nameof(maximumLengthPassword));
        }

        this.MinimumLengthPassword = minimumLengthPassword;
        this.MaximumLengthPassword = maximumLengthPassword;

        this.MinimumLowerCaseChars = minimumLowerCaseChars;
        this.MinimumUpperCaseChars = minimumUpperCaseChars;
        this.MinimumNumericChars = minimumNumericChars;
        this.MinimumSpecialChars = minimumSpecialChars;

        this._allAvailableChars =
            this.OnlyIfOneCharIsRequired(minimumLowerCaseChars, AllLowerCaseChars) +
            this.OnlyIfOneCharIsRequired(minimumUpperCaseChars, AllUpperCaseChars) +
            this.OnlyIfOneCharIsRequired(minimumNumericChars, AllNumericChars) +
            this.OnlyIfOneCharIsRequired(minimumSpecialChars, AllSpecialChars);
    }

    private string OnlyIfOneCharIsRequired(int minimum, string allChars) => minimum > 0 || this._minimumNumberOfChars == 0 ? allChars : string.Empty;

    public string Generate()
    {
        var lengthOfPassword = this._randomSecure.Next(this.MinimumLengthPassword, this.MaximumLengthPassword);

        // Get the required number of characters of each catagory and
        // add random charactes of all catagories
        var minimumChars = this.GetRandomString(AllLowerCaseChars, this.MinimumLowerCaseChars) +
                        this.GetRandomString(AllUpperCaseChars, this.MinimumUpperCaseChars) +
                        this.GetRandomString(AllNumericChars, this.MinimumNumericChars) +
                        this.GetRandomString(AllSpecialChars, this.MinimumSpecialChars);
        var rest = this.GetRandomString(this._allAvailableChars, lengthOfPassword - minimumChars.Length);
        var unshuffeledResult = minimumChars + rest;

        // Shuffle the result so the order of the characters are unpredictable
        var result = unshuffeledResult.ShuffleTextSecure();
        return result;
    }

    private string GetRandomString(string possibleChars, int lenght)
    {
        var result = string.Empty;
        for (var position = 0; position < lenght; position++)
        {
            var index = this._randomSecure.Next(possibleChars.Length);
            result += possibleChars[index];
        }

        return result;
    }

    private static string GetCharRange(char minimum, char maximum, string exclusiveChars = "")
    {
        var result = string.Empty;
        for (var value = minimum; value <= maximum; value++)
        {
            result += value;
        }

        if (!string.IsNullOrEmpty(exclusiveChars))
        {
            var inclusiveChars = result.Except(exclusiveChars).ToArray();
            result = new string(inclusiveChars);
        }

        return result;
    }
}

internal static class ExtensionMethods
{
    private static readonly Lazy<RandomSecureVersion> RandomSecure =
        new(() => new RandomSecureVersion());
    public static IEnumerable<T> ShuffleSecure<T>(this IEnumerable<T> source)
    {
        var sourceArray = source.ToArray();
        for (var counter = 0; counter < sourceArray.Length; counter++)
        {
            var randomIndex = RandomSecure.Value.Next(counter, sourceArray.Length);
            yield return sourceArray[randomIndex];

            sourceArray[randomIndex] = sourceArray[counter];
        }
    }

    public static string ShuffleTextSecure(this string source)
    {
        var shuffeldChars = source.ShuffleSecure().ToArray();
        return new string(shuffeldChars);
    }
}

internal class RandomSecureVersion
{
    //Never ever ever never use Random() in the generation of anything that requires true security/randomness
    //and high entropy or I will hunt you down with a pitchfork!! Only RNGCryptoServiceProvider() is safe.
    private readonly RNGCryptoServiceProvider _rngProvider = new();

    public int Next()
    {
        var randomBuffer = new byte[4];
        this._rngProvider.GetBytes(randomBuffer);
        var result = BitConverter.ToInt32(randomBuffer, 0);
        return result;
    }

    public int Next(int maximumValue) =>
        // Do not use Next() % maximumValue because the distribution is not OK
        this.Next(0, maximumValue);

    public int Next(int minimumValue, int maximumValue)
    {
        var seed = this.Next();

        //  Generate uniformly distributed random integers within a given range.
        return new Random(seed).Next(minimumValue, maximumValue);
    }
}
