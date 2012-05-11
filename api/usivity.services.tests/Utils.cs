using System;
using System.Text;
using Usivity.Entities.Types;

namespace Usivity.Tests.Services {

    public class Utils {

        //--- Class Methods ---
        public static Source GetRandomSource() {
            var rnd = new Random();
            var sources = Enum.GetValues(typeof(Source)); 
            var count = sources.Length;
            return (Source) sources.GetValue(rnd.Next(0, count));
        }

        public static string GetRandomText(int countOfSymbols) {
            var builder = new StringBuilder(countOfSymbols);
            var rnd = new Random();
            for(int i = 0; i < countOfSymbols; i++) {
                try {
                    int symbolAsInt = rnd.Next(0x10ffff);
                    while(0xD800 <= symbolAsInt && symbolAsInt <= 0xDFFF)
                        symbolAsInt = rnd.Next(0x10ffff);
                    char symbol = char.ConvertFromUtf32(symbolAsInt)[0];
                    if(char.IsDigit(symbol) || char.IsLetter(symbol) || char.IsPunctuation(symbol))
                        builder.Append(symbol);
                    else
                        i--;
                } catch(ArgumentOutOfRangeException) {
                    i--;
                }
            }
            return builder.ToString();
        }
    }
}
